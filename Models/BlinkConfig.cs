using CommunityToolkit.Mvvm.ComponentModel;

namespace PNGTA.Models;

public partial class BlinkConfig : ObservableObject
{
    [ObservableProperty] private bool _isEnabled;
    [ObservableProperty] private int _minIntervalMs = 3000;
    [ObservableProperty] private int _maxIntervalMs = 7000;
    [ObservableProperty] private int _durationMs = 150;
    [ObservableProperty] private int _consecutiveBlinks = 1;
    [ObservableProperty] private string _targetAnimationName = string.Empty;
}