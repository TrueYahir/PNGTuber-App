using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace PNGTA.Models;

public enum PlaybackMode
{
    OneShot,
    Loop,
    PingPong
}

public partial class AnimationFrame : ObservableObject
{
    [ObservableProperty] private string _layerName = string.Empty;
    [ObservableProperty] private string _imagePath = string.Empty;
    [ObservableProperty] private double _positionX;
    [ObservableProperty] private double _positionY;
    [ObservableProperty] private double _scaleX = 1.0;
    [ObservableProperty] private double _scaleY = 1.0;
    [ObservableProperty] private double _rotation;
    [ObservableProperty] private int _durationMs = 100;
}

public partial class AnimationTrack : ObservableObject
{
    [ObservableProperty] private string _name = "New Animation";
    [ObservableProperty] private PlaybackMode _playbackMode = PlaybackMode.Loop;
    [ObservableProperty] private int _priority = 0;
    public ObservableCollection<AnimationFrame> Frames { get; } = new();
    
    public AnimationTrack Clone()
    {
        var clone = new AnimationTrack
        {
            Name = Name + " Copy",
            PlaybackMode = PlaybackMode,
            Priority = Priority
        };
        foreach(var frame in Frames)
        {
            clone.Frames.Add(new AnimationFrame 
            {
                LayerName = frame.LayerName,
                ImagePath = frame.ImagePath,
                PositionX = frame.PositionX,
                PositionY = frame.PositionY,
                ScaleX = frame.ScaleX,
                ScaleY = frame.ScaleY,
                Rotation = frame.Rotation,
                DurationMs = frame.DurationMs
            });
        }
        return clone;
    }
}