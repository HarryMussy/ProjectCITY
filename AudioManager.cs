using System;
using System.IO;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;

public class AudioManager : IDisposable
{
    private IWavePlayer outputDevice;
    private MixingSampleProvider mixer;
    public List<VolumeSampleProvider> trackVolumeProviders = new List<VolumeSampleProvider>();
    public float volume;

    public AudioManager()
    {
        // Use WaveOutEvent for playback device
        outputDevice = new WaveOutEvent();

        // Mixer to combine multiple audio sources
        mixer = new MixingSampleProvider(WaveFormat.CreateIeeeFloatWaveFormat(44100, 2));
        mixer.ReadFully = true;

        outputDevice.Init(mixer);
        outputDevice.Play();
        volume = 0.0f;
    }

    public void PlayTrack(string filePath, bool loop = true)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException("Track file not found", filePath);

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

        // Wrap track with volume control
        var volumeProvider = new VolumeSampleProvider(trackToPlay);
        volumeProvider.Volume = volume;

        trackVolumeProviders.Add(volumeProvider);
        mixer.AddMixerInput(volumeProvider);
    }



    public void PlayPlaceSound()
    {
        string projectRoot = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\.."));
        string filePath = Path.Combine(projectRoot, @$"gameAssets\audio\Effects\place.wav");
        if (!File.Exists(filePath))
            throw new FileNotFoundException("Effect file not found", filePath);

        var effectFile = new AudioFileReader(filePath);

        // Resample to 44.1kHz stereo using MediaFoundationResampler
        var resampled = new MediaFoundationResampler(effectFile, new WaveFormat(44100, 2))
        {
            ResamplerQuality = 60 // Optional: from 1 (low) to 60 (high)
        };

        ISampleProvider effectSampleProvider = resampled.ToSampleProvider();

        mixer.AddMixerInput(effectSampleProvider);
    }

    public void Dispose()
    {
        outputDevice?.Stop();
        outputDevice?.Dispose();
        outputDevice = null;
    }
}