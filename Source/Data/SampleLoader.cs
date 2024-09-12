
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
    public StreamSampleLoader(Stream stream, uint sampleChunkStart, uint sampleChunkSize)
    {
        Reader = new(stream);
        SampleChunkStart = sampleChunkStart;
        SampleChunkSize = sampleChunkSize;
    }

    public BinaryReader Reader;
    public uint SampleChunkStart;
    public uint SampleChunkSize;
    private readonly SortedDictionary<uint, (uint Size, byte[] Data)> cache = [];

    public byte[] GetSampleData(Sample sample)
    {
        if (SampleIsCached(sample, out var positionBeforeIndex))
        {
            var positionBefore = cache.Keys.ToArray()[positionBeforeIndex];
            return cache[positionBefore].Data[(int)(sample.StartIndex - positionBefore)..(int)(sample.EndIndex - positionBefore)];
        }
        
        PreloadSample(sample);
        
        var keys = cache.Keys.ToArray();
        var index = Array.BinarySearch(keys, sample.StartIndex);
        // If sample start index is in between loaded,
        // Figure out the index of the element that would come BEFORE it (so minus one after inverting)
        if (index < 0)
            index = ~index - 1;
        return cache[keys[index]].Data[(int)(sample.StartIndex - keys[index])..(int)(sample.EndIndex - keys[index])];
    }

    public bool SampleIsCached(Sample sample)
        => SampleIsCached(sample, out _);

    private bool SampleIsCached(Sample sample, out int positionBeforeIndex)
    {
        var keys = cache.Keys.ToArray();
        var index = Array.BinarySearch(keys, sample.StartIndex);

        // If sample start index is in between loaded,
        // Figure out the index of the element that would come BEFORE it (so minus one after inverting)
        if (index < 0)
            index = ~index - 1;

        positionBeforeIndex = index >= 0 ? index : -1;

        if (index < 0)
            return false;
        else
            return keys[index] + cache[keys[index]].Size >= sample.EndIndex;
    }

    public void PreloadSample(Sample sample)
    {
        if (SampleIsCached(sample, out var positionBeforeIndex))
            return;

        var keys = cache.Keys.ToArray();
        var positionBefore = positionBeforeIndex >= 0 ? keys[positionBeforeIndex] : 0;
        
        // Check how much data we actually need to get if some of it is already loaded
        uint loadStartIndex = sample.StartIndex, loadEndIndex = sample.EndIndex;

        if (positionBefore <= sample.StartIndex && cache[positionBefore].Size + positionBefore > sample.StartIndex)
            loadStartIndex = cache[positionBefore].Size + positionBefore;
        
        if (positionBeforeIndex + 1 < keys.Length
            && keys[positionBeforeIndex + 1] < sample.EndIndex
            && cache[keys[positionBeforeIndex + 1]].Size >= sample.EndIndex - keys[positionBeforeIndex + 1])
            loadEndIndex = keys[positionBeforeIndex + 1];

        // Create the data array, and put existing data at the ends (if applicable)
        uint arrayStartIndex = loadStartIndex != sample.StartIndex
            ? positionBefore : sample.StartIndex;
        uint arrayEndIndex = loadEndIndex != sample.EndIndex
            ? keys[positionBeforeIndex + 1] + cache[keys[positionBeforeIndex + 1]].Size : sample.EndIndex;
        
        byte[] data = new byte[arrayEndIndex - arrayStartIndex];

        if (loadStartIndex != sample.StartIndex)
        {
            cache[positionBefore].Data.CopyTo(data, 0);
            // Don't have to remove the key because the new data will start from the same spot
        }

        if (loadEndIndex != sample.EndIndex)
        {
            cache[keys[positionBeforeIndex + 1]].Data.CopyTo(data, loadEndIndex - arrayStartIndex);
            cache.Remove(keys[positionBeforeIndex + 1]);
        }

        // Read the data
        Reader.BaseStream.Position = SampleChunkStart + arrayStartIndex;
        Reader.Read(data, (int)(loadStartIndex - arrayStartIndex), (int)(loadEndIndex - loadStartIndex));

        cache[arrayStartIndex] = ((uint)data.Length, data);
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