using System.Collections.ObjectModel;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;

namespace PNGTA.Models;

public partial class LayerModel : ObservableObject
{
    [ObservableProperty] private string _name = "New Layer";
    [ObservableProperty] private string _imagePath = string.Empty;
    [ObservableProperty] private Bitmap? _imageBitmap;
    [ObservableProperty] private bool _isVisible = true;
    [ObservableProperty] private bool _isLocked = false;
    [ObservableProperty] private double _positionX = 0;
    [ObservableProperty] private double _positionY = 0;
    [ObservableProperty] private double _scaleX = 1;
    [ObservableProperty] private double _scaleY = 1;
    [ObservableProperty] private double _rotation = 0;

    public ObservableCollection<LayerModel> Children { get; } = new();
    public LayerModel? Parent { get; set; }
}