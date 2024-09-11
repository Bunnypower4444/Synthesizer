
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
    private readonly SortedDictionary<long, (int Size, byte[] Data)> cache = [];

    public byte[] GetSampleData(Sample sample)
    {
        // TODO
        throw new NotImplementedException();
    }

    public bool SampleIsCached(Sample sample)
    {
        var keys = cache.Keys.ToArray();
        var index = Array.BinarySearch(keys, sample.StartIndex);

        // If sample start index is in between loaded,
        // Figure out the index of the element that would come BEFORE it (so minus one after inverting)
        if (index < 0)
            index = ~index - 1;

        if (index < 0)
            return false;
        else
            return keys[index] + cache[keys[index]].Size >= sample.EndIndex;
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
        Reader?.Dispose();
    }
}