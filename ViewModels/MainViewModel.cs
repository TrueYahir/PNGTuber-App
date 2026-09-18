using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PNGTA.Models;
using PNGTA.Services;

namespace PNGTA.ViewModels;

public partial class MainViewModel : ViewModelBase, IDisposable
{
    private readonly IAudioService _audioService;
    private readonly AvatarProjectService _projectService;
    private readonly DispatcherTimer _timer;

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

    public ObservableCollection<string> AudioDevices { get; } = new();
    public ObservableCollection<string> AvailableStates { get; } = new() 
    { 
        "Idle", "Talking", "Sad", "Crying", "Mad", "Angry", "Laugh", "Thinking" 
    };
    public ObservableCollection<LayerModel> Layers { get; } = new();

    private bool _wasTalking;

    public MainViewModel(IAudioService audioService, AvatarProjectService projectService)
    {
        _audioService = audioService;
        _projectService = projectService;

        LoadAudioDevices();

        _timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(16) };
        _timer.Tick += UpdatePreview;
        _timer.Start();
        
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
        var config = new CharacterConfig { AudioThreshold = Threshold };
        
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
            Rotation = model.Rotation
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

    public void Dispose()
    {
        _timer.Stop();
    }
    
    public void AddNewLayer()
    {
        Layers.Add(new LayerModel { Name = "New Layer" });
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
}