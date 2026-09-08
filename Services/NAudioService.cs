using System;
using System.Collections.Generic;
using NAudio.Wave;

namespace PNGTA.Services;

public class NAudioService : IAudioService
{
    private WaveIn? _waveIn; // Corregido a WaveIn
    private DateTime _lastTimeTalking = DateTime.MinValue;
    private readonly TimeSpan _debounceHoldTime = TimeSpan.FromMilliseconds(200); 

    public float CurrentVolume { get; private set; }
    public bool IsTalking { get; private set; }
    public float Threshold { get; set; } = 0.10f;

    public IEnumerable<string> GetAvailableDevices()
    {
        int waveInDevices = WaveIn.DeviceCount;
        for (int i = 0; i < waveInDevices; i++)
        {
            WaveInCapabilities deviceInfo = WaveIn.GetCapabilities(i);
            yield return deviceInfo.ProductName;
        }
    }

    public void StartCapture(int deviceNumber = 0)
    {
        StopCapture();

        _waveIn = new WaveIn // Corregido a WaveIn
        {
            DeviceNumber = deviceNumber,
            WaveFormat = new WaveFormat(44100, 1) 
        };

        _waveIn.DataAvailable += OnDataAvailable;
        _waveIn.StartRecording();
    }

    public void StopCapture()
    {
        if (_waveIn != null)
        {
            _waveIn.DataAvailable -= OnDataAvailable;
            _waveIn.StopRecording();
            _waveIn.Dispose();
            _waveIn = null;
        }
    }

    private void OnDataAvailable(object? sender, WaveInEventArgs e)
    {
        float max = 0;
        for (int index = 0; index < e.BytesRecorded; index += 2)
        {
            short sample = (short)((e.Buffer[index + 1] << 8) | e.Buffer[index]);
            var sample32 = sample / 32768f; 
            if (sample32 < 0) sample32 = -sample32;
            if (sample32 > max) max = sample32;
        }

        CurrentVolume = max;

        if (CurrentVolume >= Threshold)
        {
            _lastTimeTalking = DateTime.Now;
            IsTalking = true;
        }
        else
        {
            if ((DateTime.Now - _lastTimeTalking) > _debounceHoldTime)
            {
                IsTalking = false;
            }
        }
    }

    public void Dispose()
    {
        StopCapture();
    }
}