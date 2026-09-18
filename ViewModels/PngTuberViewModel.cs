using System;
using System.Collections.Generic;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using PNGTA.Models;
using PNGTA.Services;

namespace PNGTA.ViewModels;

public partial class PngTuberViewModel : ViewModelBase, IDisposable
{
    private readonly IAudioService _audioService;
    private readonly DispatcherTimer _timer;

    [ObservableProperty]
    private Bitmap? _currentImage;

    private readonly Dictionary<string, Bitmap> _stateBitmaps = new();
    private string _currentEmotion = "idle";
    private bool _wasTalking = false;

    public PngTuberViewModel(IAudioService audioService)
    {
        _audioService = audioService;

        _timer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(16)
        };
        _timer.Tick += UpdateState;
    }

    public void LoadConfig(CharacterConfig config)
    {
        _stateBitmaps.Clear();
        _currentEmotion = "idle";

        foreach (var state in config.States)
        {
            if (state.Value.Frames.Count > 0 && !string.IsNullOrEmpty(state.Value.Frames[0].ImagePath))
            {
                try
                {
                    _stateBitmaps[state.Key] = new Bitmap(state.Value.Frames[0].ImagePath);
                }
                catch { }
            }
        }

        SetEmotion("idle");
        _audioService.StartCapture();
        _timer.Start();
    }

    public void SetEmotion(string emotionName)
    {
        emotionName = emotionName.ToLower();
        if (_stateBitmaps.ContainsKey(emotionName))
        {
            _currentEmotion = emotionName;
            ForceUpdateImage(_audioService.IsTalking);
        }
    }

    private void UpdateState(object? sender, EventArgs e)
    {
        bool isTalkingNow = _audioService.IsTalking;
        
        if (isTalkingNow != _wasTalking)
        {
            ForceUpdateImage(isTalkingNow);
            _wasTalking = isTalkingNow;
        }
    }

    private void ForceUpdateImage(bool isTalking)
    {
        if (isTalking && _currentEmotion == "idle" && _stateBitmaps.TryGetValue("talking", out var talkingBmp))
        {
            CurrentImage = talkingBmp;
            return;
        }

        if (_stateBitmaps.TryGetValue(_currentEmotion, out var emotionBmp))
        {
            CurrentImage = emotionBmp;
        }
        else if (_stateBitmaps.TryGetValue("idle", out var idleBmp))
        {
            CurrentImage = idleBmp;
        }
    }

    public void Dispose()
    {
        _timer.Stop();
    }
}