using System.Collections.Generic;

namespace PNGTA.Models;

public class SpriteFrame
{
    public string ImagePath {get; set;} = string.Empty;
    public int DurationMS {get; set;} = 100;
}

public class CharacterState
{
    public string Name {get; set;} = string.Empty;
    public List<SpriteFrame> Frames {get; set;} = new();
}

public class CharacterConfig
{
    public string Name {get;set;} = "New Character";
    public Dictionary<string, CharacterState> States {get; set;} = new();
    public float AudioThreshold {get; set;} = 0.15f;
    public List<LayerConfig> Layers { get; set; } = new();
}