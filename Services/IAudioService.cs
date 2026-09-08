using System;
using System.Collections.Generic;

namespace PNGTA.Services;

public interface IAudioService : IDisposable
{
    float CurrentVolume { get; }
    bool IsTalking { get; }
    float Threshold { get; set; }
    
    IEnumerable<string> GetAvailableDevices();
    void StartCapture(int deviceNumber = 0);
    void StopCapture();
}