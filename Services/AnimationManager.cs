using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia.Threading;
using Avalonia.Media.Imaging;
using PNGTA.Models;

namespace PNGTA.Services;

public class AnimationState
{
    public required AnimationTrack Track { get; set; }
    public int CurrentFrameIndex { get; set; }
    public int ElapsedMs { get; set; }
    public bool IsForward { get; set; }
    public bool IsPaused { get; set; }
}

public class AnimationManager
{
    private readonly DispatcherTimer _timer;
    private readonly Dictionary<string, AnimationState> _activeAnimations = new();
    private IEnumerable<LayerModel> _rootLayers = Array.Empty<LayerModel>();

    public AnimationManager()
    {
        _timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(16) };
        _timer.Tick += OnTick;
    }

    public void SetLayers(IEnumerable<LayerModel> layers)
    {
        _rootLayers = layers;
    }

    public void Play(AnimationTrack track)
    {
        if (track.Frames.Count == 0) return;
        
        if (_activeAnimations.TryGetValue(track.Name, out var existing) && existing.Track == track)
        {
            existing.IsPaused = false;
            return;
        }

        _activeAnimations[track.Name] = new AnimationState
        {
            Track = track,
            CurrentFrameIndex = 0,
            ElapsedMs = 0,
            IsForward = true,
            IsPaused = false
        };
        _timer.Start();
    }

    public void Stop(string animationName)
    {
        _activeAnimations.Remove(animationName);
        if (_activeAnimations.Count == 0) _timer.Stop();
    }

    public void Pause(string animationName)
    {
        if (_activeAnimations.TryGetValue(animationName, out var state))
            state.IsPaused = true;
    }

    public void StopAll()
    {
        _activeAnimations.Clear();
        _timer.Stop();
    }

    private void OnTick(object? sender, EventArgs e)
    {
        var dt = 16;
        var completed = new List<string>();

        var sortedAnimations = _activeAnimations.Values
            .Where(a => !a.IsPaused)
            .OrderBy(a => a.Track.Priority)
            .ToList();

        foreach (var state in sortedAnimations)
        {
            state.ElapsedMs += dt;
            var currentFrame = state.Track.Frames[state.CurrentFrameIndex];

            if (state.ElapsedMs >= currentFrame.DurationMs)
            {
                state.ElapsedMs -= currentFrame.DurationMs;
                
                if (state.IsForward)
                {
                    state.CurrentFrameIndex++;
                    if (state.CurrentFrameIndex >= state.Track.Frames.Count)
                    {
                        if (state.Track.PlaybackMode == PlaybackMode.OneShot)
                        {
                            completed.Add(state.Track.Name);
                            state.CurrentFrameIndex = state.Track.Frames.Count - 1;
                        }
                        else if (state.Track.PlaybackMode == PlaybackMode.Loop)
                        {
                            state.CurrentFrameIndex = 0;
                        }
                        else if (state.Track.PlaybackMode == PlaybackMode.PingPong)
                        {
                            state.IsForward = false;
                            state.CurrentFrameIndex = state.Track.Frames.Count - 2;
                            if (state.CurrentFrameIndex < 0) state.CurrentFrameIndex = 0;
                        }
                    }
                }
                else
                {
                    state.CurrentFrameIndex--;
                    if (state.CurrentFrameIndex < 0)
                    {
                        state.IsForward = true;
                        state.CurrentFrameIndex = 1;
                        if (state.CurrentFrameIndex >= state.Track.Frames.Count) state.CurrentFrameIndex = 0;
                    }
                }
            }

            ApplyFrame(state.Track.Frames[state.CurrentFrameIndex]);
        }

        foreach (var name in completed)
        {
            _activeAnimations.Remove(name);
        }

        if (_activeAnimations.Count == 0)
        {
            _timer.Stop();
        }
    }

    private void ApplyFrame(AnimationFrame frame)
    {
        var target = FindLayer(_rootLayers, frame.LayerName);
        if (target != null)
        {
            if (!string.IsNullOrEmpty(frame.ImagePath) && target.ImagePath != frame.ImagePath)
            {
                target.ImagePath = frame.ImagePath;
                try { target.ImageBitmap = new Bitmap(frame.ImagePath); } catch { }
            }
            target.PositionX = frame.PositionX;
            target.PositionY = frame.PositionY;
            target.ScaleX = frame.ScaleX;
            target.ScaleY = frame.ScaleY;
            target.Rotation = frame.Rotation;
        }
    }

    private LayerModel? FindLayer(IEnumerable<LayerModel> layers, string name)
    {
        foreach (var layer in layers)
        {
            if (layer.Name == name) return layer;
            var child = FindLayer(layer.Children, name);
            if (child != null) return child;
        }
        return null;
    }
}