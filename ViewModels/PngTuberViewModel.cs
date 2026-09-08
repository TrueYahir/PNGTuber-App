using System;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using PNGTA.Services;

namespace PNGTA.ViewModels;

public partial class PngTuberViewModel : ViewModelBase, IDisposable
{
    private readonly IAudioService _audioService;
    private readonly DispatcherTimer _timer;

    [ObservableProperty]
    private Bitmap? _currentImage;

    private Bitmap? _idleBitmap;
    private Bitmap? _talkingBitmap;
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

    public void LoadImages(string idlePath, string talkingPath)
    {
        try
        {
            _idleBitmap = new Bitmap(idlePath);
            _talkingBitmap = new Bitmap(talkingPath);
            CurrentImage = _idleBitmap;
            
            _audioService.StartCapture();
            _timer.Start();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error cargando imágenes: {ex.Message}");
        }
    }

    private void UpdateState(object? sender, EventArgs e)
    {
        bool isTalkingNow = _audioService.IsTalking;
        
        if (isTalkingNow != _wasTalking)
        {
            CurrentImage = isTalkingNow ? _talkingBitmap : _idleBitmap;
            _wasTalking = isTalkingNow;
        }
    }

    public void Dispose()
    {
        _timer.Stop();
        _audioService.StopCapture();
    }
}