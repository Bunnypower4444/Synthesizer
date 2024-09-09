
namespace Synthesizer;

public interface ISampleLoader : IDisposable
{
    public byte[] GetSampleData(Sample sample);
    public bool SampleIsCached(Sample sample);
    public void PreloadSample(Sample sample);
    public void PreloadInstrument(Instrument instrument)
    {
        foreach (var zone in instrument.InstrumentZones)
        {
            PreloadSample(zone.Sample);
        }
    }
}

/// <summary>
/// Loads and caches samples from a stream.
/// </summary>
public class StreamSampleLoader : ISampleLoader
{
    public StreamSampleLoader(Stream stream, long sampleChunkStart, int sampleChunkSize)
    {
        Reader = new(stream);
        SampleChunkStart = sampleChunkStart;
        SampleChunkSize = sampleChunkSize;
    }

    public BinaryReader Reader;
    public long SampleChunkStart;
    public int SampleChunkSize;
    public Dictionary<long, (int Size, byte[] Data)> Cache = [];

    public byte[] GetSampleData(Sample sample)
    {
        // TODO
        throw new NotImplementedException();
    }

    public bool SampleIsCached(Sample sample)
    {
        // TODO
        throw new NotImplementedException();
    }

    public void PreloadSample(Sample sample)
    {
        // TODO
        throw new NotImplementedException();
    }

    ~StreamSampleLoader()
    {
        Dispose();
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        Reader.Dispose();
    }
}