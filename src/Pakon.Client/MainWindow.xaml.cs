using Microsoft.Win32;
using Pakon.Client.Scanning;
using Pakon.Scanner;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace Pakon.Client;

public partial class MainWindow : Window
{
    private readonly IScannerWorkflow scannerWorkflow = ScannerWorkflowFactory.CreateDefault();
    private readonly ObservableCollection<FrameItem> frames = [];
    private readonly DispatcherTimer adjustmentTimer;
    private readonly string sessionDirectory = Path.Combine(Path.GetTempPath(), "Pakon", Guid.NewGuid().ToString("N"));
    private readonly AppSettings settings;
    private CancellationTokenSource? operationCancellation;
    private int page;
    private bool scannerReady;
    private bool updatingSliders;
    private bool closing;
    private bool faultRecoveryActive;
    private FilmKind activeFilmKind = FilmKind.ColorNegative;
    private FrameLayout activeFrameLayout = FrameLayout.Standard;

    private bool IsBlackAndWhite => BlackWhiteRadio.IsChecked == true;
    private bool IsColorNegative => ColorNegativeRadio.IsChecked == true;
    private bool IsPositiveSlide => ColorPositiveRadio.IsChecked == true;
    private bool ActiveBlackAndWhite => activeFilmKind == FilmKind.BlackAndWhite;
    private bool ActiveColorNegative => activeFilmKind == FilmKind.ColorNegative;
    // The scanner pipeline already produces a positive image for color
    // negatives. Only B&W negative output still needs software inversion.
    private bool ActiveRequiresSoftwareInversion => activeFilmKind == FilmKind.BlackAndWhite;
    private int PageCount => Math.Max(1, (frames.Count + 8) / 9);
    private IEnumerable<FrameItem> SelectedFrames => frames.Where(x => x.IsSelected);

    public MainWindow()
    {
        InitializeComponent();
        settings = AppSettings.Load();
        Directory.CreateDirectory(sessionDirectory);
        ApplySettings();
        adjustmentTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(180) };
        adjustmentTimer.Tick += async (_, _) =>
        {
            adjustmentTimer.Stop();
            await RefreshSelectedPreviewsAsync();
        };
        Loaded += async (_, _) => await InitializeScannerAsync();
        Closing += async (_, args) =>
        {
            if (closing) return;
            args.Cancel = true;
            closing = true;
            operationCancellation?.Cancel();
            SaveSettings();
            try
            {
                using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(3));
                await scannerWorkflow.CloseAsync(timeout.Token);
            }
            catch { }
            scannerWorkflow.Dispose();
            try { Directory.Delete(sessionDirectory, true); } catch { }
            Close();
        };
    }

    private async Task InitializeScannerAsync()
    {
        ShowPage(ScanningPage);
        ProgressTitle.Text = "Starting Pakon";
        ScanStatusText.Text = "Connecting to the scanner…";
        ProgressCancelButton.Visibility = Visibility.Collapsed;
        StartScanButton.IsEnabled = false;
        SetConnection("Initializing scanner…", "#C59134");
        try
        {
            ScannerStatus? status = null;
            using (var existingBackendTimeout = new CancellationTokenSource(TimeSpan.FromMilliseconds(800)))
            {
                try
                {
                    status = await scannerWorkflow.GetStatusAsync(existingBackendTimeout.Token);
                }
                catch (OperationCanceledException) { }
                catch (IOException) { }
            }

            if (status == null)
            {
                scannerWorkflow.EnsureAvailable();
                await Task.Delay(450);
                status = await scannerWorkflow.GetStatusAsync();
            }

            var state = status.State;
            if (state is ScannerState.Faulted or ScannerState.Capturing or ScannerState.CancellingCapture)
            {
                await RecoverFromInterruptedScanAsync();
                if (scannerReady) return;
                throw new InvalidOperationException("The previous scan did not release the scanner.");
            }

            if (state == ScannerState.Closed)
                await scannerWorkflow.InitializeAsync();
            await PollUntilReadyAsync("Initializing scanner", CancellationToken.None, TimeSpan.FromMinutes(4));
            scannerReady = true;
            StartScanButton.IsEnabled = true;
            SetConnection("Scanner ready", "#2D7356");
            ShowPage(SetupPage);
        }
        catch (ScannerPrerequisiteException ex)
        {
            scannerReady = false;
            SetConnection("Pakon installation incomplete", "#B44131");
            ShowPage(SetupPage);
            MessageBox.Show(
                ex.Message,
                "Pakon software installation incomplete",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
        catch (Exception ex)
        {
            scannerReady = false;
            SetConnection("Scanner unavailable", "#B44131");
            ShowPage(SetupPage);
            var retry = MessageBox.Show($"{ex.Message}\n\nCheck scanner power and USB, then choose Retry.",
                "Scanner initialization failed", MessageBoxButton.RetryCancel, MessageBoxImage.Warning);
            if (retry == MessageBoxResult.Retry) await InitializeScannerAsync();
        }
    }

    private async void StartScanClicked(object sender, RoutedEventArgs e)
    {
        if (!scannerReady) return;
        SaveSettings();
        activeFilmKind = IsColorNegative
            ? FilmKind.ColorNegative
            : IsPositiveSlide
                ? FilmKind.ColorPositive
                : FilmKind.BlackAndWhite;
        activeFrameLayout = FrameLayoutCombo.SelectedIndex switch
        {
            1 => FrameLayout.HalfFrame,
            2 => FrameLayout.Panorama,
            _ => FrameLayout.Standard
        };
        ShowPage(ScanningPage);
        ProgressTitle.Text = "Scanning your roll";
        ScanStatusText.Text = "The scanner is finding and capturing each frame.";
        ProgressCancelButton.Visibility = Visibility.Visible;
        operationCancellation = new CancellationTokenSource();
        try
        {
            await scannerWorkflow.BeginCaptureAsync(
                new CaptureRequest(activeFilmKind, IceCheckBox.IsChecked == true, activeFrameLayout),
                operationCancellation.Token);
            await PollUntilReadyAsync("Scanning roll", operationCancellation.Token, TimeSpan.FromMinutes(20));
            ScanStatusText.Text = "Preparing previews…";
            await LoadFramesAndPreviewsAsync(operationCancellation.Token);
            ShowReviewPage();
        }
        catch (OperationCanceledException)
        {
            await RecoverFromInterruptedScanAsync();
        }
        catch (Exception ex)
        {
            var message = ex.Message;
            var tailFirst = IsFilmTailFirstError(message);
            if (tailFirst)
            {
                message = "The film was inserted tail first.\n\n" +
                    "Remove the film, turn the strip around, and insert the leader/head end first. " +
                    "The scanner is being returned to its ready state so you can try again.";
            }
            else if (IceCheckBox.IsChecked == true &&
                (message.Contains("COMException: 15", StringComparison.OrdinalIgnoreCase) ||
                 message.Contains("invalid parameter", StringComparison.OrdinalIgnoreCase)))
            {
                message += "\n\nThe scanner rejected the Digital ICE acquisition option. Disable Digital ICE and retry the scan.";
            }

            await RecoverFromInterruptedScanAsync();
            MessageBox.Show(
                message,
                tailFirst ? "Film inserted tail first" : "Scan failed",
                MessageBoxButton.OK,
                tailFirst ? MessageBoxImage.Warning : MessageBoxImage.Error);
        }
        finally
        {
            operationCancellation?.Dispose();
            operationCancellation = null;
            ProgressCancelButton.IsEnabled = true;
        }
    }

    private async Task RecoverFromInterruptedScanAsync()
    {
        ProgressCancelButton.Visibility = Visibility.Collapsed;
        StartScanButton.IsEnabled = false;
        scannerReady = false;
        ShowPage(SetupPage);
        SetConnection("Recovering scanner…", "#C59134");

        try
        {
            var status = await GetScannerStatusWithTimeoutAsync();
            var state = status.State;
            if (state != ScannerState.Ready)
            {
                try
                {
                    using var cancelTimeout = new CancellationTokenSource(TimeSpan.FromSeconds(2));
                    await scannerWorkflow.CancelCaptureAsync(cancelTimeout.Token);
                }
                catch { }
            }

            var started = Stopwatch.StartNew();
            while (started.Elapsed < TimeSpan.FromSeconds(20))
            {
                status = await GetScannerStatusWithTimeoutAsync();
                state = status.State;
                if (state == ScannerState.Ready)
                {
                    scannerReady = true;
                    StartScanButton.IsEnabled = true;
                    SetConnection("Scanner ready", "#2D7356");
                    return;
                }
                await Task.Delay(300);
            }

            await RestartScannerAsync();
        }
        catch
        {
            try
            {
                await RestartScannerAsync();
            }
            catch
            {
                scannerReady = false;
                StartScanButton.IsEnabled = false;
                SetConnection("Scanner unavailable", "#B44131");
            }
        }
    }

    private async Task<ScannerStatus> GetScannerStatusWithTimeoutAsync()
    {
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(2));
        return await scannerWorkflow.GetStatusAsync(timeout.Token);
    }

    private async Task RestartScannerAsync()
    {
        SetConnection("Restarting scanner connection…", "#C59134");
        scannerWorkflow.Restart();
        await Task.Delay(600);

        var status = await GetScannerStatusWithTimeoutAsync();
        var state = status.State;
        if (state != ScannerState.Closed)
        {
            using var closeTimeout = new CancellationTokenSource(TimeSpan.FromSeconds(4));
            await scannerWorkflow.CloseAsync(closeTimeout.Token);
        }

        using (var initializeTimeout = new CancellationTokenSource(TimeSpan.FromSeconds(4)))
        {
            await scannerWorkflow.InitializeAsync(initializeTimeout.Token);
        }

        var started = Stopwatch.StartNew();
        while (started.Elapsed < TimeSpan.FromSeconds(65))
        {
            status = await GetScannerStatusWithTimeoutAsync();
            state = status.State;
            if (state == ScannerState.Ready)
            {
                scannerReady = true;
                StartScanButton.IsEnabled = true;
                SetConnection("Scanner ready", "#2D7356");
                return;
            }
            if (state == ScannerState.Faulted)
            {
                throw new InvalidOperationException(
                    status.Failure ?? "Scanner initialization failed.");
            }
            await Task.Delay(350);
        }

        throw new TimeoutException("The scanner did not become ready after restarting.");
    }

    private async Task<bool> RecoverIfScannerFaultedAsync(Exception operationException)
    {
        ScannerStatus? status = null;
        Exception? statusException = null;
        try
        {
            status = await GetScannerStatusWithTimeoutAsync();
            if (status.State != ScannerState.Faulted) return false;
        }
        catch (Exception exception)
        {
            // A bridge that no longer answers after a scanner operation fails is
            // just as unusable as an explicitly faulted TLX session.
            statusException = exception;
        }

        if (faultRecoveryActive) return true;
        faultRecoveryActive = true;
        scannerReady = false;
        adjustmentTimer.Stop();
        operationCancellation?.Cancel();
        StartScanButton.IsEnabled = false;
        ReviewPage.IsEnabled = false;
        SavePage.IsEnabled = false;
        ProgressCancelButton.Visibility = Visibility.Collapsed;
        ProgressTitle.Text = "Recovering scanner connection";
        ScanStatusText.Text = "The TLX image pipeline stopped. Restarting the isolated bridge…";
        ScanProgress.IsIndeterminate = true;
        SetConnection("Scanner pipeline stopped", "#B44131");
        ShowPage(ScanningPage);

        var failure = status?.Failure;
        if (string.IsNullOrWhiteSpace(failure)) failure = operationException.Message;
        if (statusException != null)
            failure += $"\nStatus check failed: {statusException.Message}";

        try
        {
            await RestartScannerAsync();
            frames.Clear();
            page = 0;
            FrameGrid.ItemsSource = null;
            ReviewPage.IsEnabled = true;
            SavePage.IsEnabled = true;
            ShowPage(SetupPage);
            MessageBox.Show(
                "The TLX image pipeline stopped and could not continue using the current scan. " +
                "The scanner connection has been restarted; the roll must be scanned again.\n\n" + failure,
                "Scanner connection restarted",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
        }
        catch (Exception recoveryException)
        {
            scannerReady = false;
            StartScanButton.IsEnabled = false;
            ReviewPage.IsEnabled = true;
            SavePage.IsEnabled = true;
            SetConnection("Scanner unavailable", "#B44131");
            ShowPage(SetupPage);
            MessageBox.Show(
                "The TLX image pipeline stopped and the scanner connection could not be restarted. " +
                "Restart the application after checking scanner power and USB.\n\n" +
                failure + "\n\nRecovery failed: " + recoveryException.Message,
                "Scanner unavailable",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
        finally
        {
            faultRecoveryActive = false;
        }

        return true;
    }

    private static bool IsFilmTailFirstError(string message) =>
        message.Contains("film tail first", StringComparison.OrdinalIgnoreCase) ||
        message.Contains("0xE0000000", StringComparison.OrdinalIgnoreCase);

    private async Task LoadFramesAndPreviewsAsync(CancellationToken cancellationToken)
    {
        var capturedFrames = await scannerWorkflow.CompleteCaptureAsync(cancellationToken);
        await PopulateFramesAndPreviewsAsync(capturedFrames, [], cancellationToken);
    }

    private async Task ReloadFramesAndPreviewsAsync(CancellationToken cancellationToken)
    {
        var previousFrames = frames.ToArray();
        var capturedFrames = await scannerWorkflow.GetFramesAsync(cancellationToken);
        await PopulateFramesAndPreviewsAsync(capturedFrames, previousFrames, cancellationToken);
    }

    private async Task PopulateFramesAndPreviewsAsync(
        IReadOnlyList<CapturedFrame> capturedFrames,
        IReadOnlyList<FrameItem> previousFrames,
        CancellationToken cancellationToken)
    {
        frames.Clear();
        for (var index = 0; index < capturedFrames.Count; index++)
        {
            var capturedFrame = capturedFrames[index];
            var frameNumber = capturedFrame.FrameNumber;
            var frameName = capturedFrame.FrameName;
            var sourcePath = Path.Combine(sessionDirectory, $"preview-source-{index:000}.jpg");
            var render = await scannerWorkflow.RenderFrameAsync(
                new FrameRenderRequest(
                    capturedFrame.Index,
                    sourcePath,
                    FrameRenderFormat.Jpeg,
                    activeFilmKind,
                    IceCheckBox.IsChecked == true,
                    LowResolution: true,
                    Width: 900,
                    Height: 620,
                    JpegQuality: 95),
                cancellationToken);
            await PollUntilReadyAsync($"Preparing preview {index + 1} of {capturedFrames.Count}", cancellationToken, TimeSpan.FromMinutes(3));
            sourcePath = render.OutputPath;
            var frame = new FrameItem
            {
                Index = capturedFrame.Index,
                StripIndex = capturedFrame.StripIndex,
                FrameNumber = frameNumber <= 0 ? index + 1 : frameNumber,
                FrameName = frameName,
                SourcePath = sourcePath,
                Framing = capturedFrame.Framing
            };
            var previous = FindPreviousFrame(previousFrames, capturedFrame);
            if (previous != null)
            {
                frame.IsIncluded = previous.IsIncluded;
                frame.Rotation = previous.Rotation;
                frame.Exposure = previous.Exposure;
                frame.Contrast = previous.Contrast;
                frame.RedBalance = previous.RedBalance;
                frame.GreenBalance = previous.GreenBalance;
                frame.BlueBalance = previous.BlueBalance;
            }
            frame.PropertyChanged += (_, args) =>
            {
                if (args.PropertyName == nameof(FrameItem.IsIncluded)) UpdateReviewSummary();
            };
            var adjustedPath = Path.Combine(
                sessionDirectory,
                $"preview-{frame.Index:000}-{Guid.NewGuid():N}.jpg");
            frame.Preview = await ImageAdjustmentService.CreatePreviewAsync(
                frame,
                ActiveRequiresSoftwareInversion,
                ActiveBlackAndWhite,
                adjustedPath);
            frames.Add(frame);
        }
        page = 0;
        UpdatePage();
    }

    private static FrameItem? FindPreviousFrame(
        IReadOnlyList<FrameItem> previousFrames,
        CapturedFrame capturedFrame) =>
        previousFrames
            .Where(x => x.StripIndex == capturedFrame.StripIndex &&
                Contains(x.Framing.Current, capturedFrame.Framing.Current))
            .OrderBy(x => x.Framing.Current.Width * (long)x.Framing.Current.Height)
            .FirstOrDefault();

    private static bool Contains(FrameBounds outer, FrameBounds inner) =>
        outer.Left <= inner.Left && outer.Top <= inner.Top &&
        outer.Right >= inner.Right && outer.Bottom >= inner.Bottom;

    private async Task PollUntilReadyAsync(string activity, CancellationToken cancellationToken, TimeSpan timeout)
    {
        var started = Stopwatch.StartNew();
        while (started.Elapsed < timeout)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var status = await scannerWorkflow.GetStatusAsync(cancellationToken);
            var state = status.State;
            var meaning = status.ProgressMeaning;
            Dispatcher.Invoke(() =>
            {
                ScanStatusText.Text = string.IsNullOrWhiteSpace(meaning) ? $"{activity}…" : $"{activity} · {meaning}";
                FooterText.Text = $"{activity} · {started.Elapsed:mm\\:ss}";
                if (status.Progress is > 0 and <= 100)
                {
                    ScanProgress.IsIndeterminate = false;
                    ScanProgress.Value = status.Progress.Value;
                }
                else
                {
                    ScanProgress.IsIndeterminate = true;
                }
            });
            if (state == ScannerState.Ready) return;
            if (state == ScannerState.Faulted)
            {
                var failure = status.Failure;
                throw new InvalidOperationException(string.IsNullOrWhiteSpace(failure)
                    ? "The Pakon scanner reported an error. Check the scanner diagnostics for details."
                    : failure);
            }
            await Task.Delay(300, cancellationToken);
        }
        throw new TimeoutException($"{activity} did not finish within {timeout.TotalMinutes:0} minutes.");
    }

    private void ShowReviewPage()
    {
        ShowPage(ReviewPage);
        ColorBalancePanel.Visibility = ActiveBlackAndWhite ? Visibility.Collapsed : Visibility.Visible;
        UpdateColorBalanceLabels();
        UpdateReviewSummary();
        FooterText.Text = "Review · Select frames to edit together";
    }

    private void FrameCardClicked(object sender, MouseButtonEventArgs e)
    {
        if ((sender as FrameworkElement)?.DataContext is not FrameItem frame) return;
        if (e.ClickCount == 2)
        {
            e.Handled = true;
            var preview = new PreviewWindow(
                frames.ToArray(), frame, RefreshFramePreviewAsync, ApplyFrameFramingAsync,
                RecoverIfScannerFaultedAsync) { Owner = this };
            preview.ShowDialog();
            return;
        }
        if ((Keyboard.Modifiers & ModifierKeys.Control) == 0)
            foreach (var item in frames) item.IsSelected = false;
        frame.IsSelected = !frame.IsSelected;
        SyncSlidersFromSelection();
    }

    private void FrameSelectionChanged(object sender, RoutedEventArgs e)
    {
        if (IsLoaded) SyncSlidersFromSelection();
    }

    private void SyncSlidersFromSelection()
    {
        var first = SelectedFrames.FirstOrDefault();
        EditSelectionTitle.Text = first == null ? "Select a frame" :
            SelectedFrames.Skip(1).Any() ? $"{SelectedFrames.Count()} frames selected" : first.DisplayName;
        if (first == null) return;
        updatingSliders = true;
        ExposureSlider.Value = first.Exposure;
        ContrastSlider.Value = first.Contrast;
        var colorDirection = ActiveColorNegative ? -1 : 1;
        RedSlider.Value = first.RedBalance * colorDirection;
        GreenSlider.Value = first.GreenBalance * colorDirection;
        BlueSlider.Value = first.BlueBalance * colorDirection;
        updatingSliders = false;
    }

    private void AdjustmentChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (updatingSliders || !IsLoaded) return;
        foreach (var frame in SelectedFrames)
        {
            frame.Exposure = ExposureSlider.Value;
            frame.Contrast = ContrastSlider.Value;
            if (!ActiveBlackAndWhite)
            {
                var colorDirection = ActiveColorNegative ? -1 : 1;
                frame.RedBalance = RedSlider.Value * colorDirection;
                frame.GreenBalance = GreenSlider.Value * colorDirection;
                frame.BlueBalance = BlueSlider.Value * colorDirection;
            }
        }
        adjustmentTimer.Stop();
        adjustmentTimer.Start();
    }

    private async Task RefreshSelectedPreviewsAsync()
    {
        foreach (var frame in SelectedFrames.ToArray())
            await RefreshFramePreviewAsync(frame);
    }

    private async Task RefreshFramePreviewAsync(FrameItem frame)
    {
        var path = Path.Combine(sessionDirectory, $"adjusted-{frame.Index:000}-{Guid.NewGuid():N}.jpg");
        frame.Preview = await ImageAdjustmentService.CreatePreviewAsync(
            frame,
            ActiveRequiresSoftwareInversion,
            ActiveBlackAndWhite,
            path);
    }

    private async Task ApplyFrameFramingAsync(FrameItem frame, FrameBounds bounds)
    {
        await scannerWorkflow.UpdateFrameFramingAsync(frame.Index, bounds);
        frame.Framing = frame.Framing with { Current = bounds };

        await RenderFramedPreviewAsync(frame);
    }

    private async Task RenderFramedPreviewAsync(FrameItem frame)
    {
        var sourcePath = Path.Combine(
            sessionDirectory,
            $"preview-source-{frame.Index:000}-{Guid.NewGuid():N}.jpg");
        var render = await scannerWorkflow.RenderFrameAsync(
            new FrameRenderRequest(
                frame.Index,
                sourcePath,
                FrameRenderFormat.Jpeg,
                activeFilmKind,
                IceCheckBox.IsChecked == true,
                LowResolution: true,
                Width: 900,
                Height: 620,
                JpegQuality: 95));
        await PollUntilReadyAsync(
            $"Updating framing for {frame.DisplayName}",
            CancellationToken.None,
            TimeSpan.FromMinutes(3));
        frame.SourcePath = render.OutputPath;
        await RefreshFramePreviewAsync(frame);
    }

    private async void RotateLeftClicked(object sender, RoutedEventArgs e)
    {
        foreach (var frame in SelectedFrames) frame.Rotation -= 90;
        await RefreshSelectedPreviewsAsync();
    }

    private async void RotateRightClicked(object sender, RoutedEventArgs e)
    {
        foreach (var frame in SelectedFrames) frame.Rotation += 90;
        await RefreshSelectedPreviewsAsync();
    }

    private void IncludeSelectedClicked(object sender, RoutedEventArgs e)
    {
        foreach (var frame in SelectedFrames) frame.IsIncluded = true;
    }

    private void ExcludeSelectedClicked(object sender, RoutedEventArgs e)
    {
        foreach (var frame in SelectedFrames) frame.IsIncluded = false;
    }

    private static bool TryReadInteger(TextBox textBox, out int value) =>
        int.TryParse(
            textBox.Text,
            NumberStyles.Integer,
            CultureInfo.InvariantCulture,
            out value);

    private void AdjustBulkFramingValue(TextBox textBox, int delta)
    {
        if (!TryReadInteger(textBox, out var value)) value = 0;
        textBox.Text = (value + delta).ToString(CultureInfo.InvariantCulture);
        BulkFramingStatus.Text = "Not applied";
    }

    private void BulkNarrowerClicked(object sender, RoutedEventArgs e) =>
        AdjustBulkFramingValue(BulkWidthDeltaBox, -32);

    private void BulkWiderClicked(object sender, RoutedEventArgs e) =>
        AdjustBulkFramingValue(BulkWidthDeltaBox, 32);

    private void BulkNudgeLeftClicked(object sender, RoutedEventArgs e) =>
        AdjustBulkFramingValue(BulkNudgeBox, -16);

    private void BulkNudgeRightClicked(object sender, RoutedEventArgs e) =>
        AdjustBulkFramingValue(BulkNudgeBox, 16);

    private async void ApplySelectedFramingClicked(object sender, RoutedEventArgs e)
    {
        var selected = SelectedFrames.ToArray();
        if (selected.Length == 0)
        {
            BulkFramingStatus.Text = "Select at least one frame first.";
            return;
        }
        await ApplyFramingAdjustmentAsync(selected, "selected frames");
    }

    private async void ApplyBulkFramingClicked(object sender, RoutedEventArgs e) =>
        await ApplyFramingAdjustmentAsync(frames.ToArray(), "all frames");

    private async Task ApplyFramingAdjustmentAsync(
        IReadOnlyList<FrameItem> targetFrames,
        string targetDescription)
    {
        if (!TryReadInteger(BulkWidthDeltaBox, out var widthDelta) ||
            !TryReadInteger(BulkNudgeBox, out var horizontalNudge))
        {
            BulkFramingStatus.Text = "Enter whole-number pixel adjustments.";
            return;
        }
        if (widthDelta == 0 && horizontalNudge == 0)
        {
            BulkFramingStatus.Text = "No framing change to apply.";
            return;
        }

        var changes = new List<(FrameItem Frame, FrameBounds Original, FrameBounds Updated)>();
        foreach (var frame in targetFrames)
        {
            var original = frame.Framing.Current;
            var leftWidthChange = widthDelta / 2;
            var rightWidthChange = widthDelta - leftWidthChange;
            var updated = new FrameBounds(
                original.Left - leftWidthChange + horizontalNudge,
                original.Top,
                original.Right + rightWidthChange + horizontalNudge,
                original.Bottom);
            if (updated.Left < 0 || updated.Right >= frame.Framing.StripWidth)
            {
                BulkFramingStatus.Text = $"{frame.DisplayName} would extend beyond the scanned strip.";
                return;
            }
            if (updated.Width < 128)
            {
                BulkFramingStatus.Text = $"{frame.DisplayName} would become narrower than 128 pixels.";
                return;
            }
            changes.Add((frame, original, updated));
        }

        BulkApplyFramingButton.IsEnabled = false;
        SelectedApplyFramingButton.IsEnabled = false;
        var applied = new List<(FrameItem Frame, FrameBounds Original)>();
        try
        {
            BulkFramingStatus.Text = "Updating native frame boundaries…";
            foreach (var change in changes)
            {
                await scannerWorkflow.UpdateFrameFramingAsync(change.Frame.Index, change.Updated);
                applied.Add((change.Frame, change.Original));
            }
        }
        catch (Exception exception)
        {
            var rollbackSucceeded = true;
            foreach (var change in applied.AsEnumerable().Reverse())
            {
                try { await scannerWorkflow.UpdateFrameFramingAsync(change.Frame.Index, change.Original); }
                catch { rollbackSucceeded = false; }
            }
            BulkFramingStatus.Text = rollbackSucceeded
                ? "No changes were kept."
                : "Rollback was incomplete; reload the scan before making more framing changes.";
            if (await RecoverIfScannerFaultedAsync(exception))
            {
                BulkApplyFramingButton.IsEnabled = true;
                SelectedApplyFramingButton.IsEnabled = true;
                return;
            }
            MessageBox.Show(exception.Message, $"Could not adjust {targetDescription}", MessageBoxButton.OK, MessageBoxImage.Error);
            BulkApplyFramingButton.IsEnabled = true;
            SelectedApplyFramingButton.IsEnabled = true;
            return;
        }

        foreach (var change in changes)
            change.Frame.Framing = change.Frame.Framing with { Current = change.Updated };

        try
        {
            for (var index = 0; index < changes.Count; index++)
            {
                BulkFramingStatus.Text = $"Refreshing preview {index + 1} of {changes.Count}…";
                await RenderFramedPreviewAsync(changes[index].Frame);
            }
            BulkWidthDeltaBox.Text = "0";
            BulkNudgeBox.Text = "0";
            BulkFramingStatus.Text = $"Applied to {changes.Count} frame{(changes.Count == 1 ? string.Empty : "s")}.";
            FooterText.Text = $"Framing updated for {targetDescription}";
        }
        catch (Exception exception)
        {
            BulkFramingStatus.Text = "Framing was applied, but one or more previews could not be refreshed.";
            if (await RecoverIfScannerFaultedAsync(exception)) return;
            MessageBox.Show(exception.Message, "Preview refresh failed", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
        finally
        {
            BulkApplyFramingButton.IsEnabled = true;
            SelectedApplyFramingButton.IsEnabled = true;
        }
    }

    private FrameItem? GetSingleSelectedFrame(string operation)
    {
        var selected = SelectedFrames.Take(2).ToArray();
        if (selected.Length == 1) return selected[0];
        MessageBox.Show(
            $"Select exactly one frame to {operation}.",
            "Select one frame",
            MessageBoxButton.OK,
            MessageBoxImage.Information);
        return null;
    }

    private async void AddFrameClicked(object sender, RoutedEventArgs e)
    {
        var frame = GetSingleSelectedFrame("add a frame");
        if (frame == null) return;
        try
        {
            FooterText.Text = "Adding frame…";
            var insertBefore = frame.Index + 1 < frames.Count ? frame.Index + 1 : int.MaxValue;
            await scannerWorkflow.InsertFrameAsync(insertBefore, frame.StripIndex, frame.Framing.Current);
            await ReloadFramesAndPreviewsAsync(CancellationToken.None);
            if (frame.Index + 1 < frames.Count) frames[frame.Index + 1].IsSelected = true;
            SyncSlidersFromSelection();
            FooterText.Text = "Frame added · Double-click it to adjust framing";
        }
        catch (Exception exception)
        {
            if (await RecoverIfScannerFaultedAsync(exception)) return;
            MessageBox.Show(exception.Message, "Could not add frame", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async void SplitFrameClicked(object sender, RoutedEventArgs e)
    {
        var frame = GetSingleSelectedFrame("split it");
        if (frame == null) return;
        var bounds = frame.Framing.Current;
        var middle = bounds.Left + bounds.Width / 2;
        var first = bounds with { Right = middle };
        var second = bounds with { Left = middle };
        if (first.Width < 128 || second.Width < 128)
        {
            MessageBox.Show(
                "This frame is too narrow to split into two valid TLX pictures.",
                "Cannot split frame",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
            return;
        }

        var insertedIndex = frame.Index + 1;
        var insertBefore = insertedIndex < frames.Count ? insertedIndex : int.MaxValue;
        var inserted = false;
        var originalUpdated = false;
        try
        {
            FooterText.Text = "Splitting half-frame pair…";
            await scannerWorkflow.InsertFrameAsync(insertBefore, frame.StripIndex, second);
            inserted = true;
            await scannerWorkflow.UpdateFrameFramingAsync(frame.Index, first);
            originalUpdated = true;
            await ReloadFramesAndPreviewsAsync(CancellationToken.None);
            foreach (var item in frames) item.IsSelected = false;
            if (frame.Index < frames.Count) frames[frame.Index].IsSelected = true;
            if (insertedIndex < frames.Count) frames[insertedIndex].IsSelected = true;
            SyncSlidersFromSelection();
            FooterText.Text = "Frame split into two half-frame pictures";
        }
        catch (Exception exception)
        {
            if (originalUpdated)
            {
                try { await scannerWorkflow.UpdateFrameFramingAsync(frame.Index, bounds); } catch { }
            }
            if (inserted)
            {
                try { await scannerWorkflow.DeleteFrameAsync(insertedIndex); } catch { }
            }
            if (await RecoverIfScannerFaultedAsync(exception)) return;
            MessageBox.Show(exception.Message, "Could not split frame", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async void DeleteFrameClicked(object sender, RoutedEventArgs e)
    {
        var frame = GetSingleSelectedFrame("delete it");
        if (frame == null) return;
        if (frames.Count == 1)
        {
            MessageBox.Show("The last frame cannot be deleted.", "Cannot delete frame", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }
        if (MessageBox.Show(
                $"Delete {frame.DisplayName} from this scan?",
                "Delete frame",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning) != MessageBoxResult.Yes) return;
        try
        {
            FooterText.Text = "Deleting frame…";
            await scannerWorkflow.DeleteFrameAsync(frame.Index);
            await ReloadFramesAndPreviewsAsync(CancellationToken.None);
            FooterText.Text = "Frame deleted";
        }
        catch (Exception exception)
        {
            if (await RecoverIfScannerFaultedAsync(exception)) return;
            MessageBox.Show(exception.Message, "Could not delete frame", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async void ResetAdjustmentsClicked(object sender, RoutedEventArgs e)
    {
        foreach (var frame in SelectedFrames)
        {
            frame.Exposure = frame.Contrast = frame.RedBalance = frame.GreenBalance = frame.BlueBalance = 0;
            frame.Rotation = 0;
        }
        SyncSlidersFromSelection();
        await RefreshSelectedPreviewsAsync();
    }

    private void SelectAllClicked(object sender, RoutedEventArgs e)
    {
        foreach (var frame in frames) frame.IsSelected = true;
        SyncSlidersFromSelection();
    }

    private void ClearSelectionClicked(object sender, RoutedEventArgs e)
    {
        foreach (var frame in frames) frame.IsSelected = false;
        SyncSlidersFromSelection();
    }

    private void PreviousPageClicked(object sender, RoutedEventArgs e) { if (page > 0) { page--; UpdatePage(); } }
    private void NextPageClicked(object sender, RoutedEventArgs e) { if (page + 1 < PageCount) { page++; UpdatePage(); } }

    private void UpdatePage()
    {
        FrameGrid.ItemsSource = frames.Skip(page * 9).Take(9).ToArray();
        PageText.Text = $"{page + 1} / {PageCount}";
        PreviousPageButton.IsEnabled = page > 0;
        NextPageButton.IsEnabled = page + 1 < PageCount;
        UpdateReviewSummary();
    }

    private void UpdateReviewSummary()
    {
        ReviewSummaryText.Text = $"{frames.Count(x => x.IsIncluded)} of {frames.Count} frames included · Page {page + 1} of {PageCount}";
    }

    private void ContinueToSavingClicked(object sender, RoutedEventArgs e)
    {
        if (!frames.Any(x => x.IsIncluded))
        {
            MessageBox.Show("Select at least one frame to save.", "Nothing selected", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }
        SaveSummaryText.Text = $"{frames.Count(x => x.IsIncluded)} selected frames are ready. Choose maximum-quality 16-bit PNG or compact JPEG.";
        ShowPage(SavePage);
    }

    private void BackToReviewClicked(object sender, RoutedEventArgs e) => ShowReviewPage();

    private void BrowseClicked(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFolderDialog { Title = "Choose a folder for scanned images", Multiselect = false };
        if (dialog.ShowDialog() == true) OutputFolderTextBox.Text = dialog.FolderName;
    }

    private void ExportFormatChanged(object sender, RoutedEventArgs e)
    {
        if (Png16ColorNotice == null || Png16Radio == null) return;
        Png16ColorNotice.Visibility = Png16Radio.IsChecked == true
            ? Visibility.Visible
            : Visibility.Collapsed;
    }

    private void OpenOutputFolderClicked(object sender, RoutedEventArgs e)
    {
        var directory = OutputFolderTextBox.Text.Trim();
        if (!Directory.Exists(directory)) return;
        Process.Start(new ProcessStartInfo
        {
            FileName = "explorer.exe",
            ArgumentList = { directory },
            UseShellExecute = true
        });
    }

    private async void ScanAnotherRollClicked(object sender, RoutedEventArgs e)
    {
        SaveSettings();
        ShowPage(ScanningPage);
        ProgressTitle.Text = "Preparing another roll";
        ScanStatusText.Text = "Clearing the previous scanner session…";
        ProgressCancelButton.Visibility = Visibility.Collapsed;
        ScanProgress.IsIndeterminate = true;
        try
        {
            await scannerWorkflow.CloseAsync();
            await scannerWorkflow.InitializeAsync();
            await PollUntilReadyAsync("Initializing scanner", CancellationToken.None, TimeSpan.FromMinutes(4));
            frames.Clear();
            page = 0;
            FrameGrid.ItemsSource = null;
            SaveProgress.Visibility = Visibility.Collapsed;
            SaveStatusText.Text = "";
            PostSaveActionsPanel.Visibility = Visibility.Collapsed;
            SaveButton.Visibility = Visibility.Visible;
            scannerReady = true;
            StartScanButton.IsEnabled = true;
            SetConnection("Scanner ready", "#2D7356");
            ShowPage(SetupPage);
        }
        catch (Exception ex)
        {
            scannerReady = false;
            StartScanButton.IsEnabled = false;
            SetConnection("Scanner unavailable", "#B44131");
            MessageBox.Show(ex.Message, "Could not prepare another roll", MessageBoxButton.OK, MessageBoxImage.Error);
            ShowPage(SetupPage);
        }
    }

    private async void SaveClicked(object sender, RoutedEventArgs e)
    {
        var outputDirectory = OutputFolderTextBox.Text.Trim();
        if (string.IsNullOrWhiteSpace(outputDirectory))
        {
            MessageBox.Show("Choose a destination folder.", "Destination required", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }
        Directory.CreateDirectory(outputDirectory);
        SaveSettings();
        var included = frames.Where(x => x.IsIncluded).ToArray();
        var usePng = Png16Radio.IsChecked == true;
        SaveButton.IsEnabled = false;
        SaveButton.Visibility = Visibility.Visible;
        SaveProgress.Visibility = Visibility.Visible;
        SaveProgress.Maximum = included.Length;
        SaveProgress.Value = 0;
        PostSaveActionsPanel.Visibility = Visibility.Collapsed;
        try
        {
            for (var position = 0; position < included.Length; position++)
            {
                var frame = included[position];
                SaveStatusText.Text = $"Saving {position + 1} of {included.Length} · {frame.DisplayName}";
                var stem = BuildOutputStem(PrefixTextBox.Text, frame, position + 1);
                var outputPath = UniquePath(outputDirectory, stem, usePng ? ".png" : ".jpg");
                if (usePng)
                    await SavePng16Async(frame, outputPath);
                else
                    await SaveJpegAsync(frame, outputPath);
                SaveProgress.Value = position + 1;
            }
            SaveStatusText.Text = $"Saved {included.Length} frames";
            FooterText.Text = $"Complete · {outputDirectory}";
            SaveButton.Visibility = Visibility.Collapsed;
            PostSaveActionsPanel.Visibility = Visibility.Visible;
        }
        catch (Exception ex)
        {
            if (await RecoverIfScannerFaultedAsync(ex)) return;
            MessageBox.Show(ex.Message, "Saving failed", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            SaveButton.IsEnabled = true;
        }
    }

    private async Task SavePng16Async(FrameItem frame, string outputPath)
    {
        var rawPath = Path.Combine(sessionDirectory, $"frame-{frame.Index:000}.raw");
        await scannerWorkflow.RenderFrameAsync(new FrameRenderRequest(
            frame.Index,
            rawPath,
            FrameRenderFormat.Planar16,
            activeFilmKind,
            IceCheckBox.IsChecked == true));
        await PollUntilReadyAsync($"Rendering {frame.DisplayName}", CancellationToken.None, TimeSpan.FromMinutes(5));
        var converter = RawConverterLocator.Find();
        var args = new List<string>
        {
            converter, "--input", rawPath, "--output", outputPath, "--format", "png",
            // The scanner's rendered planar data already has its display tone curve.
            // Applying another 1/2.2 gamma here makes the PNG much brighter than
            // the scanner-rendered JPEG used by the preview.
            "--gamma", "1",
            "--contrast", "1", "--saturation", "1", "--brightness", "1",
            "--exposure", Number(frame.Exposure), "--contrast-adjustment", Number(frame.Contrast),
            "--red-balance", Factor(frame.RedBalance), "--green-balance", Factor(frame.GreenBalance),
            "--blue-balance", Factor(frame.BlueBalance), "--rotation", frame.Rotation.ToString(CultureInfo.InvariantCulture)
        };
        if (ActiveBlackAndWhite) args.Insert(1, "--bw");
        if (ActiveRequiresSoftwareInversion) args.Insert(1, "--invert");
        var startInfo = new ProcessStartInfo
        {
            FileName = "dotnet",
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardError = true
        };
        foreach (var arg in args) startInfo.ArgumentList.Add(arg);
        var process = Process.Start(startInfo) ?? throw new InvalidOperationException("Could not start the raw converter.");
        await process.WaitForExitAsync();
        var error = await process.StandardError.ReadToEndAsync();
        if (process.ExitCode != 0) throw new InvalidOperationException("Raw conversion failed: " + error);
    }

    private async Task SaveJpegAsync(FrameItem frame, string outputPath)
    {
        var sourceRequest = Path.Combine(sessionDirectory, $"full-source-{frame.Index:000}.jpg");
        var render = await scannerWorkflow.RenderFrameAsync(new FrameRenderRequest(
            frame.Index,
            sourceRequest,
            FrameRenderFormat.Jpeg,
            activeFilmKind,
            IceCheckBox.IsChecked == true,
            ApplyStoredRotation: false,
            JpegQuality: 100));
        await PollUntilReadyAsync($"Rendering {frame.DisplayName}", CancellationToken.None, TimeSpan.FromMinutes(5));
        await ImageAdjustmentService.SaveJpegAsync(
            render.OutputPath,
            outputPath,
            frame,
            ActiveRequiresSoftwareInversion,
            ActiveBlackAndWhite);
    }

    private static string Factor(double percent) => (1 + percent / 100d).ToString("0.####", CultureInfo.InvariantCulture);

    private static string Number(double value) => value.ToString("0.####", CultureInfo.InvariantCulture);

    private static string BuildOutputStem(string prefix, FrameItem frame, int sequence)
    {
        var baseName = !frame.HasUsableDxName
            ? sequence.ToString("000", CultureInfo.InvariantCulture)
            : frame.FrameName.Trim();
        baseName = SanitizeFilePart(Path.GetFileNameWithoutExtension(baseName));
        if (string.IsNullOrWhiteSpace(baseName)) baseName = sequence.ToString("000", CultureInfo.InvariantCulture);
        prefix = SanitizeFilePart(prefix.Trim());
        return string.IsNullOrWhiteSpace(prefix) ? baseName : $"{prefix}-{baseName}";
    }

    private static string SanitizeFilePart(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return string.Empty;
        var invalid = Regex.Escape(new string(Path.GetInvalidFileNameChars()));
        return Regex.Replace(value, $"[{invalid}]+", "-").Trim(' ', '-', '.');
    }

    private static string UniquePath(string directory, string stem, string extension)
    {
        var path = Path.Combine(directory, stem + extension);
        for (var suffix = 2; File.Exists(path); suffix++) path = Path.Combine(directory, $"{stem}-{suffix}{extension}");
        return path;
    }

    private void CancelScanClicked(object sender, RoutedEventArgs e)
    {
        if (operationCancellation == null || ProgressCancelButton.IsEnabled == false) return;
        ProgressCancelButton.IsEnabled = false;
        ScanStatusText.Text = "Cancelling scan…";
        operationCancellation.Cancel();
    }

    private void FilmTypeChanged(object sender, RoutedEventArgs e)
    {
        if (ColorBalancePanel != null && ReviewPage.Visibility == Visibility.Visible)
            ColorBalancePanel.Visibility = ActiveBlackAndWhite ? Visibility.Collapsed : Visibility.Visible;
        UpdateColorBalanceLabels();
        UpdateBwIceNotice();
    }

    private void UpdateColorBalanceLabels()
    {
        if (RedBalanceLabel == null) return;
        var colorNegative = ReviewPage.Visibility == Visibility.Visible
            ? ActiveColorNegative
            : IsColorNegative;
        RedBalanceLabel.Text = colorNegative ? "Cyan" : "Red";
        GreenBalanceLabel.Text = colorNegative ? "Magenta" : "Green";
        BlueBalanceLabel.Text = colorNegative ? "Yellow" : "Blue";
    }

    private void IceSelectionChanged(object sender, RoutedEventArgs e) => UpdateBwIceNotice();

    private void UpdateBwIceNotice()
    {
        if (BwIceNotice != null)
            BwIceNotice.Visibility = IsBlackAndWhite && IceCheckBox.IsChecked == true
                ? Visibility.Visible
                : Visibility.Collapsed;
    }

    private void ShowPage(UIElement page)
    {
        SetupPage.Visibility = page == SetupPage ? Visibility.Visible : Visibility.Collapsed;
        ScanningPage.Visibility = page == ScanningPage ? Visibility.Visible : Visibility.Collapsed;
        ReviewPage.Visibility = page == ReviewPage ? Visibility.Visible : Visibility.Collapsed;
        SavePage.Visibility = page == SavePage ? Visibility.Visible : Visibility.Collapsed;
    }

    private void SetConnection(string text, string color)
    {
        ConnectionText.Text = text;
        ConnectionDot.Fill = (Brush)new BrushConverter().ConvertFromString(color)!;
        FooterText.Text = text;
    }

    private void ApplySettings()
    {
        OutputFolderTextBox.Text = settings.OutputFolder;
        PrefixTextBox.Text = settings.Prefix;
        ColorNegativeRadio.IsChecked = settings.FilmType == "ColorNegative";
        ColorPositiveRadio.IsChecked = settings.FilmType == "ColorPositive";
        BlackWhiteRadio.IsChecked = settings.FilmType == "BlackAndWhite";
        IceCheckBox.IsChecked = settings.DigitalIce;
        Png16Radio.IsChecked = settings.OutputFormat != "Jpeg";
        JpegRadio.IsChecked = settings.OutputFormat == "Jpeg";
    }

    private void SaveSettings()
    {
        settings.OutputFolder = OutputFolderTextBox.Text.Trim();
        settings.Prefix = PrefixTextBox.Text.Trim();
        settings.FilmType = IsColorNegative ? "ColorNegative" :
            ColorPositiveRadio.IsChecked == true ? "ColorPositive" : "BlackAndWhite";
        settings.DigitalIce = IceCheckBox.IsChecked == true;
        settings.OutputFormat = JpegRadio.IsChecked == true ? "Jpeg" : "Png16";
        try { settings.Save(); } catch { }
    }

}
