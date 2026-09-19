// Models/LayerModel.cs
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
    [ObservableProperty] private double _opacity = 1.0;
    [ObservableProperty] private int _zIndex = 0;

    public ObservableCollection<LayerModel> Children { get; } = new();
    public LayerModel? Parent { get; set; }

    public LayerModel Clone()
    {
        var clone = new LayerModel
        {
            Name = Name + " Copy",
            ImagePath = ImagePath,
            ImageBitmap = ImageBitmap,
            IsVisible = IsVisible,
            IsLocked = IsLocked,
            PositionX = PositionX,
            PositionY = PositionY,
            ScaleX = ScaleX,
            ScaleY = ScaleY,
            Rotation = Rotation,
            Opacity = Opacity,
            ZIndex = ZIndex
        };

        foreach (var child in Children)
        {
            var childClone = child.Clone();
            childClone.Parent = clone;
            clone.Children.Add(childClone);
        }

        return clone;
    }
}