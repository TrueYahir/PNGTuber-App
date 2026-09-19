using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using PNGTA.Models;
using PNGTA.Services;

namespace PNGTA.ViewModels;

public partial class MainViewModel : ViewModelBase, IDisposable
{
    private readonly IAudioService _audioService;
    private readonly AvatarProjectService _projectService;
    private readonly DispatcherTimer _timer;
    private readonly AnimationManager _animationManager;
    private readonly DispatcherTimer _blinkTimer = new();
    private readonly Random _random = new();

    [ObservableProperty] private float _threshold = 0.1f;
    [ObservableProperty] private string? _selectedAudioDevice;
    [ObservableProperty] private string _selectedPreviewState = "Idle";
    [ObservableProperty] private Bitmap? _previewImage;

    [ObservableProperty] private string _idleImagePath = string.Empty;
    [ObservableProperty] private string _talkingImagePath = string.Empty;
    [ObservableProperty] private string _sadImagePath = string.Empty;
    [ObservableProperty] private string _cryingImagePath = string.Empty;
    [ObservableProperty] private string _madImagePath = string.Empty;
    [ObservableProperty] private string _angryImagePath = string.Empty;
    [ObservableProperty] private string _laughImagePath = string.Empty;
    [ObservableProperty] private string _thinkingImagePath = string.Empty;

    [ObservableProperty] private Bitmap? _idleBitmap;
    [ObservableProperty] private Bitmap? _talkingBitmap;
    [ObservableProperty] private Bitmap? _sadBitmap;
    [ObservableProperty] private Bitmap? _cryingBitmap;
    [ObservableProperty] private Bitmap? _madBitmap;
    [ObservableProperty] private Bitmap? _angryBitmap;
    [ObservableProperty] private Bitmap? _laughBitmap;
    [ObservableProperty] private Bitmap? _thinkingBitmap;
    
    [ObservableProperty] private LayerModel? _selectedLayer;
    [ObservableProperty] private AnimationTrack? _selectedAnimation;
    [ObservableProperty] private AnimationFrame? _selectedFrame;
    [ObservableProperty] private BlinkConfig _blinkSettings = new();

    public ObservableCollection<string> AudioDevices { get; } = new();
    public ObservableCollection<string> AvailableStates { get; } = new() 
    { 
        "Idle", "Talking", "Sad", "Crying", "Mad", "Angry", "Laugh", "Thinking" 
    };
    public ObservableCollection<LayerModel> Layers { get; } = new();
    public ObservableCollection<AnimationTrack> Animations { get; } = new();

    private bool _wasTalking;
    private bool _isBlinking;

    public MainViewModel(IAudioService audioService, AvatarProjectService projectService)
    {
        _audioService = audioService;
        _projectService = projectService;

        LoadAudioDevices();

        _animationManager = new AnimationManager();
        _animationManager.SetLayers(Layers);

        _timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(16) };
        _timer.Tick += UpdatePreview;
        _timer.Start();
        
        InitializeBlinkSystem();
        
        _audioService.StartCapture();
    }

    private void LoadAudioDevices()
    {
        var devices = _audioService.GetAvailableDevices().ToList();
        foreach (var device in devices)
        {
            AudioDevices.Add(device);
        }
        SelectedAudioDevice = AudioDevices.FirstOrDefault();
    }

    partial void OnThresholdChanged(float value)
    {
        _audioService.Threshold = value;
    }

    partial void OnSelectedPreviewStateChanged(string value)
    {
        ForcePreviewUpdate();
    }

    partial void OnBlinkSettingsChanged(BlinkConfig value)
    {
        if (value != null)
        {
            value.PropertyChanged += (s, e) => UpdateBlinkTimer();
            UpdateBlinkTimer();
        }
    }

    public void InitializeBlinkSystem()
    {
        _blinkTimer.Tick += OnBlinkTimerTick;
        BlinkSettings.PropertyChanged += (s, e) => UpdateBlinkTimer();
        UpdateBlinkTimer();
    }

    private void UpdateBlinkTimer()
    {
        _blinkTimer.Stop();
        if (BlinkSettings.IsEnabled && !string.IsNullOrEmpty(BlinkSettings.TargetAnimationName))
        {
            int min = Math.Min(BlinkSettings.MinIntervalMs, BlinkSettings.MaxIntervalMs);
            int max = Math.Max(BlinkSettings.MinIntervalMs, BlinkSettings.MaxIntervalMs);
            int nextInterval = _random.Next(min, max + 1);
            _blinkTimer.Interval = TimeSpan.FromMilliseconds(nextInterval);
            _blinkTimer.Start();
        }
    }

    private async void OnBlinkTimerTick(object? sender, EventArgs e)
    {
        _blinkTimer.Stop();
        if (!BlinkSettings.IsEnabled || _isBlinking) return;

        var blinkAnim = Animations.FirstOrDefault(a => a.Name.Equals(BlinkSettings.TargetAnimationName, StringComparison.OrdinalIgnoreCase));
        if (blinkAnim != null)
        {
            _isBlinking = true;

            if (blinkAnim.Frames.Count > 0)
            {
                int durationPerFrame = BlinkSettings.DurationMs / blinkAnim.Frames.Count;
                foreach (var frame in blinkAnim.Frames)
                {
                    frame.DurationMs = durationPerFrame;
                }
            }

            for (int i = 0; i < BlinkSettings.ConsecutiveBlinks; i++)
            {
                if (!BlinkSettings.IsEnabled) break;

                _animationManager.Stop(blinkAnim.Name);
                _animationManager.Play(blinkAnim);

                await Task.Delay(BlinkSettings.DurationMs);

                if (i < BlinkSettings.ConsecutiveBlinks - 1)
                {
                    await Task.Delay(BlinkSettings.DurationMs / 2);
                }
            }

            _animationManager.Stop(blinkAnim.Name);
            _isBlinking = false;
        }

        UpdateBlinkTimer();
    }

    public void UpdateImagePath(string state, string path)
    {
        try
        {
            var bitmap = new Bitmap(path);
            switch (state)
            {
                case "Idle": IdleImagePath = path; IdleBitmap = bitmap; break;
                case "Talking": TalkingImagePath = path; TalkingBitmap = bitmap; break;
                case "Sad": SadImagePath = path; SadBitmap = bitmap; break;
                case "Crying": CryingImagePath = path; CryingBitmap = bitmap; break;
                case "Mad": MadImagePath = path; MadBitmap = bitmap; break;
                case "Angry": AngryImagePath = path; AngryBitmap = bitmap; break;
                case "Laugh": LaughImagePath = path; LaughBitmap = bitmap; break;
                case "Thinking": ThinkingImagePath = path; ThinkingBitmap = bitmap; break;
            }
            ForcePreviewUpdate();
        }
        catch { }
    }

    private void ForcePreviewUpdate()
    {
        bool isTalkingNow = _audioService.IsTalking;
        UpdatePreviewImage(isTalkingNow);
    }

    private void UpdatePreview(object? sender, EventArgs e)
    {
        bool isTalkingNow = _audioService.IsTalking;
        if (isTalkingNow != _wasTalking)
        {
            UpdatePreviewImage(isTalkingNow);

            var idleTrack = Animations.FirstOrDefault(a => a.Name.Equals("Idle", StringComparison.OrdinalIgnoreCase));
            var talkingTrack = Animations.FirstOrDefault(a => a.Name.Equals("Talking", StringComparison.OrdinalIgnoreCase));

            if (isTalkingNow)
            {
                if (idleTrack != null) _animationManager.Stop(idleTrack.Name);
                if (talkingTrack != null) _animationManager.Play(talkingTrack);
            }
            else
            {
                if (talkingTrack != null) _animationManager.Stop(talkingTrack.Name);
                if (idleTrack != null) _animationManager.Play(idleTrack);
            }

            _wasTalking = isTalkingNow;
        }
    }

    private void UpdatePreviewImage(bool isTalking)
    {
        if (isTalking && SelectedPreviewState == "Idle" && TalkingBitmap != null)
        {
            PreviewImage = TalkingBitmap;
            return;
        }

        PreviewImage = SelectedPreviewState switch
        {
            "Idle" => IdleBitmap,
            "Talking" => TalkingBitmap,
            "Sad" => SadBitmap,
            "Crying" => CryingBitmap,
            "Mad" => MadBitmap,
            "Angry" => AngryBitmap,
            "Laugh" => LaughBitmap,
            "Thinking" => ThinkingBitmap,
            _ => IdleBitmap
        };
    }

    public CharacterConfig BuildCurrentConfig()
    {
        var config = new CharacterConfig 
        { 
            AudioThreshold = Threshold,
            BlinkSettings = BlinkSettings 
        };
        
        void AddState(string name, string path)
        {
            if (!string.IsNullOrEmpty(path))
            {
                config.States[name.ToLower()] = new CharacterState 
                { 
                    Name = name.ToLower(), 
                    Frames = { new SpriteFrame { ImagePath = path } } 
                };
            }
        }

        AddState("Idle", IdleImagePath);
        AddState("Talking", TalkingImagePath);
        AddState("Sad", SadImagePath);
        AddState("Crying", CryingImagePath);
        AddState("Mad", MadImagePath);
        AddState("Angry", AngryImagePath);
        AddState("Laugh", LaughImagePath);
        AddState("Thinking", ThinkingImagePath);

        config.Layers = new List<LayerConfig>();
        foreach (var layer in Layers)
        {
            config.Layers.Add(MapLayerToConfig(layer));
        }

        return config;
    }

    public void LoadConfig(CharacterConfig config)
    {
        Threshold = config.AudioThreshold;
        _audioService.Threshold = Threshold;
        BlinkSettings = config.BlinkSettings ?? new BlinkConfig();

        string GetPath(string state) => 
            config.States.TryGetValue(state, out var s) && s.Frames.Count > 0 ? s.Frames[0].ImagePath : string.Empty;

        UpdateImagePath("Idle", GetPath("idle"));
        UpdateImagePath("Talking", GetPath("talking"));
        UpdateImagePath("Sad", GetPath("sad"));
        UpdateImagePath("Crying", GetPath("crying"));
        UpdateImagePath("Mad", GetPath("mad"));
        UpdateImagePath("Angry", GetPath("angry"));
        UpdateImagePath("Laugh", GetPath("laugh"));
        UpdateImagePath("Thinking", GetPath("thinking"));

        Layers.Clear();
        if (config.Layers != null)
        {
            foreach (var layerConfig in config.Layers)
            {
                Layers.Add(MapConfigToLayer(layerConfig, null));
            }
        }
    }

    private LayerConfig MapLayerToConfig(LayerModel model)
    {
        var config = new LayerConfig
        {
            Name = model.Name,
            ImagePath = model.ImagePath,
            IsVisible = model.IsVisible,
            IsLocked = model.IsLocked,
            PositionX = model.PositionX,
            PositionY = model.PositionY,
            ScaleX = model.ScaleX,
            ScaleY = model.ScaleY,
            Rotation = model.Rotation,
            Opacity = model.Opacity,
            ZIndex = model.ZIndex
        };

        foreach (var child in model.Children)
        {
            config.Children.Add(MapLayerToConfig(child));
        }

        return config;
    }

    private LayerModel MapConfigToLayer(LayerConfig config, LayerModel? parent)
    {
        var model = new LayerModel
        {
            Name = config.Name,
            ImagePath = config.ImagePath,
            IsVisible = config.IsVisible,
            IsLocked = config.IsLocked,
            PositionX = config.PositionX,
            PositionY = config.PositionY,
            ScaleX = config.ScaleX,
            ScaleY = config.ScaleY,
            Rotation = config.Rotation,
            Opacity = config.Opacity,
            ZIndex = config.ZIndex,
            Parent = parent
        };

        if (!string.IsNullOrEmpty(model.ImagePath))
        {
            try 
            { 
                model.ImageBitmap = new Bitmap(model.ImagePath); 
            } 
            catch { }
        }

        if (config.Children != null)
        {
            foreach (var childConfig in config.Children)
            {
                model.Children.Add(MapConfigToLayer(childConfig, model));
            }
        }

        return model;
    }

    public void AddNewLayer()
    {
        Layers.Add(new LayerModel { Name = "New Layer", ZIndex = Layers.Count });
    }

    public void RemoveSelectedLayer()
    {
        if (SelectedLayer != null)
        {
            RemoveLayerRecursive(Layers, SelectedLayer);
            SelectedLayer = null;
        }
    }

    private bool RemoveLayerRecursive(ObservableCollection<LayerModel> list, LayerModel target)
    {
        if (list.Contains(target))
        {
            list.Remove(target);
            return true;
        }
        foreach (var layer in list)
        {
            if (RemoveLayerRecursive(layer.Children, target))
            {
                return true;
            }
        }
        return false;
    }

    public void DuplicateSelectedLayer()
    {
        if (SelectedLayer != null)
        {
            var clone = SelectedLayer.Clone();
            if (SelectedLayer.Parent != null)
            {
                SelectedLayer.Parent.Children.Add(clone);
            }
            else
            {
                Layers.Add(clone);
            }
            SelectedLayer = clone;
        }
    }

    public void AddNewAnimation()
    {
        Animations.Add(new AnimationTrack { Name = "New Animation" });
    }

    public void RemoveSelectedAnimation()
    {
        if (SelectedAnimation != null)
        {
            _animationManager.Stop(SelectedAnimation.Name);
            Animations.Remove(SelectedAnimation);
            SelectedAnimation = null;
        }
    }

    public void DuplicateSelectedAnimation()
    {
        if (SelectedAnimation != null)
        {
            var clone = SelectedAnimation.Clone();
            Animations.Add(clone);
            SelectedAnimation = clone;
        }
    }

    public void AddFrameToSelectedAnimation()
    {
        if (SelectedAnimation != null)
        {
            var layerName = SelectedLayer?.Name ?? string.Empty;
            SelectedAnimation.Frames.Add(new AnimationFrame { LayerName = layerName });
        }
    }

    public void RemoveSelectedFrame()
    {
        if (SelectedAnimation != null && SelectedFrame != null)
        {
            SelectedAnimation.Frames.Remove(SelectedFrame);
            SelectedFrame = null;
        }
    }

    public void PlaySelectedAnimation()
    {
        if (SelectedAnimation != null)
        {
            _animationManager.Play(SelectedAnimation);
        }
    }

    public void PauseSelectedAnimation()
    {
        if (SelectedAnimation != null)
        {
            _animationManager.Pause(SelectedAnimation.Name);
        }
    }

    public void StopSelectedAnimation()
    {
        if (SelectedAnimation != null)
        {
            _animationManager.Stop(SelectedAnimation.Name);
        }
    }

    public void RestartSelectedAnimation()
    {
        if (SelectedAnimation != null)
        {
            _animationManager.Stop(SelectedAnimation.Name);
            _animationManager.Play(SelectedAnimation);
        }
    }

    public void Dispose()
    {
        _timer.Stop();
        _blinkTimer.Stop();
        _animationManager.StopAll();
    }
}