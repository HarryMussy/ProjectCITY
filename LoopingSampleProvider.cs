using NAudio.Wave;
using NAudio.Wave.SampleProviders;

public class LoopingSampleProvider : ISampleProvider
{
    private readonly ISampleProvider source;

    public LoopingSampleProvider(ISampleProvider source)
    {
        this.source = source;
        WaveFormat = source.WaveFormat;
    }

    public WaveFormat WaveFormat { get; private set; }

    public int Read(float[] buffer, int offset, int count)
    {
        int totalSamplesRead = 0;

        while (totalSamplesRead < count)
        {
            int samplesRead = source.Read(buffer, offset + totalSamplesRead, count - totalSamplesRead);
            if (samplesRead == 0)
            {
                //rewind source when it reaches the end
                if (source is AudioFileReader afr)
                {
                    afr.Position = 0;
                }
                else
                {
                    break;
                }
            }
            totalSamplesRead += samplesRead;
        }
        return totalSamplesRead;
    }
}