// Models/CharactersModels.cs
using System.Collections.Generic;

namespace PNGTA.Models;

public class SpriteFrame
{
    public string ImagePath { get; set; } = string.Empty;
    public int DurationMS { get; set; } = 100;
}

public class CharacterState
{
    public string Name { get; set; } = string.Empty;
    public List<SpriteFrame> Frames { get; set; } = new();
}

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
    public double Opacity { get; set; } = 1.0;
    public int ZIndex { get; set; } = 0;
    public List<LayerConfig> Children { get; set; } = new();
}

public class CharacterConfig
{
    public string Name { get; set; } = "New Character";
    public Dictionary<string, CharacterState> States { get; set; } = new();
    public float AudioThreshold { get; set; } = 0.15f;
    public List<LayerConfig> Layers { get; set; } = new();
    public BlinkConfig BlinkSettings { get; set; } = new();
}