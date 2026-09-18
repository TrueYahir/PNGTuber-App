namespace PNGTA.Models;
public class AppSettings
{
    public string LastAudioDevice {get;set;} = string.Empty;
    public float LastThreshold {get;set;} = 0.1f;
    public string LastProfilePath {get;set;} = string.Empty;
}