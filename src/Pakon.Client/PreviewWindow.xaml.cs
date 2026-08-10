using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Pakon.Scanner;

namespace Pakon.Client;

public partial class PreviewWindow : Window
{
    private readonly IReadOnlyList<FrameItem> frames;
    private readonly Func<FrameItem, Task> refreshPreview;
    private readonly Func<FrameItem, FrameBounds, Task> applyFraming;
    private readonly Func<Exception, Task<bool>> recoverIfScannerFaulted;
    private int index;
    private double zoom = 1;
    private bool framingBusy;

    public PreviewWindow(
        IReadOnlyList<FrameItem> frames,
        FrameItem initialFrame,
        Func<FrameItem, Task> refreshPreview,
        Func<FrameItem, FrameBounds, Task> applyFraming,
        Func<Exception, Task<bool>> recoverIfScannerFaulted)
    {
        InitializeComponent();
        this.frames = frames;
        this.refreshPreview = refreshPreview;
        this.applyFraming = applyFraming;
        this.recoverIfScannerFaulted = recoverIfScannerFaulted;
        index = Math.Max(0, frames.ToList().IndexOf(initialFrame));
        Loaded += (_, _) =>
        {
            UpdateFrame();
            Focus();
        };
    }

    private FrameItem Current => frames[index];

    private void UpdateFrame()
    {
        PreviewImage.Source = Current.Preview;
        FrameTitle.Text = Current.DisplayName;
        FramePosition.Text = $"Frame {index + 1} of {frames.Count} · {(Current.IsIncluded ? "Included" : "Excluded")}";
        Title = $"{Current.DisplayName} — Pakon preview";
        SetFramingFields(Current.Framing.Current);
        FramingStatus.Text = DescribeFraming(Current.Framing.Current);
    }

    private void Move(int offset)
    {
        if (framingBusy) return;
        index = (index + offset + frames.Count) % frames.Count;
        UpdateFrame();
        ImageScroller.ScrollToHorizontalOffset(0);
        ImageScroller.ScrollToVerticalOffset(0);
    }

    private async Task RotateAsync(int degrees)
    {
        Current.Rotation += degrees;
        await refreshPreview(Current);
        UpdateFrame();
    }

    private void SetZoom(double value)
    {
        zoom = Math.Clamp(value, 0.25, 4);
        PreviewScale.ScaleX = PreviewScale.ScaleY = zoom;
        ZoomText.Text = $"{zoom * 100:0}%";
    }

    private void WindowKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Left) Move(-1);
        else if (e.Key == Key.Right) Move(1);
        else if (e.Key == Key.L) _ = RotateAsync(-90);
        else if (e.Key == Key.R) _ = RotateAsync(90);
        else if (e.Key is Key.Add or Key.OemPlus) SetZoom(zoom + 0.25);
        else if (e.Key is Key.Subtract or Key.OemMinus) SetZoom(zoom - 0.25);
        else if (e.Key is Key.D0 or Key.NumPad0) SetZoom(1);
        else if (e.Key == Key.Escape) Close();
    }

    private void PreviewImageMouseWheel(object sender, MouseWheelEventArgs e)
    {
        if ((Keyboard.Modifiers & ModifierKeys.Control) == 0) return;
        SetZoom(zoom + (e.Delta > 0 ? 0.25 : -0.25));
        e.Handled = true;
    }

    private void PreviousClicked(object sender, RoutedEventArgs e) => Move(-1);
    private void NextClicked(object sender, RoutedEventArgs e) => Move(1);
    private async void RotateLeftClicked(object sender, RoutedEventArgs e) => await RotateAsync(-90);
    private async void RotateRightClicked(object sender, RoutedEventArgs e) => await RotateAsync(90);
    private void ZoomOutClicked(object sender, RoutedEventArgs e) => SetZoom(zoom - 0.25);
    private void ZoomInClicked(object sender, RoutedEventArgs e) => SetZoom(zoom + 0.25);
    private void FitClicked(object sender, RoutedEventArgs e)
    {
        if (PreviewImage.Source is not BitmapSource source || source.PixelWidth == 0 || source.PixelHeight == 0)
        {
            SetZoom(1);
            return;
        }
        var widthScale = Math.Max(0.25, (ImageScroller.ViewportWidth - 70) / source.PixelWidth);
        var heightScale = Math.Max(0.25, (ImageScroller.ViewportHeight - 70) / source.PixelHeight);
        SetZoom(Math.Min(widthScale, heightScale));
    }

    private void SetFramingFields(FrameBounds bounds)
    {
        LeftBox.Text = bounds.Left.ToString();
        TopBox.Text = bounds.Top.ToString();
        RightBox.Text = bounds.Right.ToString();
        BottomBox.Text = bounds.Bottom.ToString();
    }

    private string DescribeFraming(FrameBounds bounds) =>
        $"{bounds.Width} × {bounds.Height} px · strip {Current.Framing.StripWidth} × {Current.Framing.StripHeight}";

    private bool TryReadFraming(out FrameBounds bounds)
    {
        bounds = Current.Framing.Current;
        if (!int.TryParse(LeftBox.Text, out var left) ||
            !int.TryParse(TopBox.Text, out var top) ||
            !int.TryParse(RightBox.Text, out var right) ||
            !int.TryParse(BottomBox.Text, out var bottom))
        {
            FramingStatus.Text = "Enter whole-number coordinates.";
            return false;
        }

        bounds = new FrameBounds(left, top, right, bottom);
        var framing = Current.Framing;
        if (left < 0 || top < 0 || right >= framing.StripWidth || bottom >= framing.StripHeight)
        {
            FramingStatus.Text = $"Coordinates must remain inside 0–{framing.StripWidth - 1} × 0–{framing.StripHeight - 1}.";
            return false;
        }
        if (bounds.Width < 128 || bounds.Height < 128)
        {
            FramingStatus.Text = "The frame must be at least 128 × 128 pixels.";
            return false;
        }
        return true;
    }

    private async Task ApplyFramingAsync()
    {
        if (framingBusy || !TryReadFraming(out var bounds)) return;
        framingBusy = true;
        ApplyFramingButton.IsEnabled = false;
        ResetFramingButton.IsEnabled = false;
        FramingStatus.Text = "Applying framing…";
        try
        {
            await applyFraming(Current, bounds);
            UpdateFrame();
        }
        catch (Exception exception)
        {
            if (await recoverIfScannerFaulted(exception))
            {
                Close();
                return;
            }
            FramingStatus.Text = exception.Message;
            MessageBox.Show(exception.Message, "Could not update framing", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            framingBusy = false;
            ApplyFramingButton.IsEnabled = true;
            ResetFramingButton.IsEnabled = true;
        }
    }

    private void Nudge(int horizontal, int vertical)
    {
        if (!TryReadFraming(out var bounds)) return;
        var maximumRight = Current.Framing.StripWidth - 1;
        var maximumBottom = Current.Framing.StripHeight - 1;
        var dx = Math.Clamp(horizontal, -bounds.Left, maximumRight - bounds.Right);
        var dy = Math.Clamp(vertical, -bounds.Top, maximumBottom - bounds.Bottom);
        SetFramingFields(new FrameBounds(
            bounds.Left + dx, bounds.Top + dy, bounds.Right + dx, bounds.Bottom + dy));
        FramingStatus.Text = "Not applied";
    }

    private void NudgeLeftClicked(object sender, RoutedEventArgs e) => Nudge(-16, 0);
    private void NudgeRightClicked(object sender, RoutedEventArgs e) => Nudge(16, 0);
    private void NudgeUpClicked(object sender, RoutedEventArgs e) => Nudge(0, -16);
    private void NudgeDownClicked(object sender, RoutedEventArgs e) => Nudge(0, 16);
    private async void ApplyFramingClicked(object sender, RoutedEventArgs e) => await ApplyFramingAsync();
    private async void ResetFramingClicked(object sender, RoutedEventArgs e)
    {
        SetFramingFields(Current.Framing.Detected);
        await ApplyFramingAsync();
    }

    private void CloseClicked(object sender, RoutedEventArgs e) => Close();
}
