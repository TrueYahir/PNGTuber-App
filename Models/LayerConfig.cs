using System.Collections.Generic;

namespace PNGTA.Models;

public class LayerConfig
{
    public string Name { get; set; } = string.Empty;
    public string ImagePath { get; set; } = string.Empty;
    public bool IsVisible { get; set; } = true;
    public bool IsLocked { get; set; } = false;
    public double PositionX { get; set; } = 0;
    public double PositionY { get; set; } = 0;
    public double ScaleX { get; set; } = 1;
    public double ScaleY { get; set; } = 1;
    public double Rotation { get; set; } = 0;
    public List<LayerConfig> Children { get; set; } = new();
}