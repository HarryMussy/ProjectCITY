using System;
using System.IO;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;

public class AudioManager : IDisposable
{
    private IWavePlayer outputDevice;
    private MixingSampleProvider mixer;
    public List<VolumeSampleProvider> trackVolumeProviders = new List<VolumeSampleProvider>();
    public float masterVolume;
    public float efxVolume;
    public float musicVolume;

    public AudioManager()
    {
        outputDevice = new WaveOutEvent();
        mixer = new MixingSampleProvider(WaveFormat.CreateIeeeFloatWaveFormat(44100, 2));
        mixer.ReadFully = true;
        outputDevice.Init(mixer);
        outputDevice.Play();
        //starting values for volume
        masterVolume = 0.0f;
        efxVolume = 0.0f;
        musicVolume = 0.0f;
    }

    public void PlayTrack(string filePath, bool loop = true)
    {
        //returns an error if the audio track is not found
        if (!File.Exists(filePath)) { throw new FileNotFoundException("Track file not found", filePath); }

        //find and format the audio file
        var audioFile = new AudioFileReader(filePath);
        var convertedAudio = new MediaFoundationResampler(audioFile, new WaveFormat(44100, 2))
        {
            ResamplerQuality = 60
        };

        ISampleProvider trackToPlay = convertedAudio.ToSampleProvider();
        if (loop)
        {
            trackToPlay = new LoopingSampleProvider(trackToPlay);
        }

        //play track
        var volumeProvider = new VolumeSampleProvider(trackToPlay);
        volumeProvider.Volume = masterVolume * musicVolume;
        trackVolumeProviders.Add(volumeProvider);
        mixer.AddMixerInput(volumeProvider);
    }

    public void PlayPlaceSound()
    {
        //find the audio file for the place sound and return an error if not found
        string projectRoot = AppContext.BaseDirectory;
        string filePath = Path.Combine(projectRoot, @$"gameAssets\audio\Effects\place.wav");
        if (!File.Exists(filePath)) { throw new FileNotFoundException("Effect file not found", filePath); }

        //format
        var effectFile = new AudioFileReader(filePath);
        var resampled = new MediaFoundationResampler(effectFile, new WaveFormat(44100, 2))
        {
            ResamplerQuality = 60
        };

        //play
        var effectSampleProvider = resampled.ToSampleProvider();
        var effectVolumeProvider = new VolumeSampleProvider(effectSampleProvider)
        {
            Volume = masterVolume * efxVolume
        };
        mixer.AddMixerInput(effectVolumeProvider);
    }

    public void SetMasterVolume(float v)
    {
        //changes the volume for all effects and tracks
        masterVolume = v;
        foreach (var t in trackVolumeProviders) { t.Volume = masterVolume * musicVolume; }
    }

    public void SetMusicVolume(float v)
    {
        //changes the music volume
        musicVolume = v;
        foreach (var t in trackVolumeProviders) { t.Volume = masterVolume * musicVolume; }
    }

    public void SetEffectsVolume(float v)
    {
        efxVolume = v;
        //effects volume is applied per effect in PlayPlaceSound
    }

    public void Dispose()
    {
        outputDevice?.Stop();
        outputDevice?.Dispose();
        outputDevice = null;
    }
}