using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media.Imaging;
using Pakon.Scanner;

namespace Pakon.Client;

public sealed class FrameItem : INotifyPropertyChanged
{
    private bool isIncluded = true;
    private bool isSelected;
    private int rotation;
    private double exposure;
    private double contrast;
    private double redBalance;
    private double greenBalance;
    private double blueBalance;
    private BitmapImage? preview;

    public required int Index { get; init; }
    public required int StripIndex { get; init; }
    public required int FrameNumber { get; init; }
    public required string FrameName { get; init; }
    public required string SourcePath { get; set; }
    public required FrameFraming Framing { get; set; }
    public bool HasUsableDxName =>
        !string.IsNullOrWhiteSpace(FrameName) &&
        !string.Equals(FrameName.Trim(), "DX_ERROR", StringComparison.OrdinalIgnoreCase);
    public string DisplayName => HasUsableDxName ? FrameName : $"Frame {FrameNumber:00}";
    public bool IsIncluded { get => isIncluded; set => Set(ref isIncluded, value); }
    public bool IsSelected { get => isSelected; set => Set(ref isSelected, value); }
    public int Rotation { get => rotation; set => Set(ref rotation, ((value % 360) + 360) % 360); }
    public double Exposure { get => exposure; set => Set(ref exposure, value); }
    public double Contrast { get => contrast; set => Set(ref contrast, value); }
    public double RedBalance { get => redBalance; set => Set(ref redBalance, value); }
    public double GreenBalance { get => greenBalance; set => Set(ref greenBalance, value); }
    public double BlueBalance { get => blueBalance; set => Set(ref blueBalance, value); }
    public BitmapImage? Preview { get => preview; set => Set(ref preview, value); }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void Set<T>(ref T field, T value, [CallerMemberName] string? name = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return;
        field = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
