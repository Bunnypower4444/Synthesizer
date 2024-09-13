
// Enable this to run StreamSampleLoader tests
#define StreamSampleLoaderTests
#if StreamSampleLoaderTests
#define CustomMain
#endif

using System.Diagnostics;

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
    public void ClearCache();
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
        if (sample.EndIndex > SampleChunkSize)
            sample.EndIndex = SampleChunkSize;

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

        if (sample.EndIndex > SampleChunkSize)
            sample.EndIndex = SampleChunkSize;

        var keys = cache.Keys.ToArray();
        var positionBefore = positionBeforeIndex >= 0 ? keys[positionBeforeIndex] : 0;
        
        // Check how much data we actually need to get if some of it is already loaded
        uint loadStartIndex = sample.StartIndex, loadEndIndex = sample.EndIndex;

        if (positionBefore <= sample.StartIndex && cache[positionBefore].Size + positionBefore >= sample.StartIndex)
            loadStartIndex = cache[positionBefore].Size + positionBefore;
        
        if (positionBeforeIndex + 1 < keys.Length
            && keys[positionBeforeIndex + 1] <= sample.EndIndex
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

    public void ClearCache()
        => cache.Clear();

    ~StreamSampleLoader()
    {
        Dispose();
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        Reader.Dispose();
    }

    #if StreamSampleLoaderTests

    /*
     * Errors
     Unhandled exception. System.Collections.Generic.KeyNotFoundException: The given key '0' was not present in the dictionary.
        at System.Collections.Generic.SortedDictionary`2.get_Item(TKey key)
        at Synthesizer.StreamSampleLoader.PreloadSample(Sample sample) in /Users/yinqian/Desktop/Users/Baaaa/Coding/Java_C#/Synthesizer/Source/Data/SampleLoader.cs:line 101
        at Synthesizer.StreamSampleLoader.Main() in /Users/yinqian/Desktop/Users/Baaaa/Coding/Java_C#/Synthesizer/Source/Data/SampleLoader.cs:line 175
     */
    
    public static void Main()
    {
        // Use a MemoryStream to provide fake sample data
        byte[] data = [
            255, 255, 255, 255, 255, 255,
            0  , 1  , 2  , 3  , 4  , 5  , 6  , 7  , 8  , 9  ,
            10 , 11 , 12 , 13 , 14 , 15 , 16 , 17 , 18 , 19 ,
            20 , 21 , 22 , 23 , 24 , 25 , 26 , 27 , 28 , 29 ,
            254, 254, 254, 254, 254, 254, 254, 254
        ];

        using var stream = new MemoryStream(
            data,
            false
        );

        using var loader = new StreamSampleLoader(stream, 6, 30);

        static Sample SampleFrom(uint start, uint end)
            => new() { StartIndex = start, EndIndex = end };

        Debug.Assert(false == loader.SampleIsCached(SampleFrom(10, 15)));

        loader.PreloadSample(SampleFrom(10, 20));
        Debug.Assert(loader.cache.ContainsKey(10));
        Debug.Assert(loader.cache[10].Size == 10);
        Debug.Assert(loader.cache[10].Data.SequenceEqual(new byte[] {10, 11, 12, 13, 14, 15, 16, 17, 18, 19}));
        Debug.Assert(true == loader.SampleIsCached(SampleFrom(10, 20)));
        Debug.Assert(true == loader.SampleIsCached(SampleFrom(15, 18)));
        Debug.Assert(false == loader.SampleIsCached(SampleFrom(8, 15)));
        Debug.Assert(false == loader.SampleIsCached(SampleFrom(12, 22)));
        Debug.Assert(loader.GetSampleData(SampleFrom(15, 20)).SequenceEqual(new byte[] {15, 16, 17, 18, 19}));
        Debug.Assert(1 == loader.cache.Count);

        Debug.Assert(loader.GetSampleData(SampleFrom(20, 25)).SequenceEqual(new byte[] {20, 21, 22, 23, 24}));
        Debug.Assert(1 == loader.cache.Count);
        Debug.Assert(loader.cache[10].Data.SequenceEqual(new byte[] {10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24}));
        
        Debug.Assert(loader.GetSampleData(SampleFrom(2, 5)).SequenceEqual(new byte[] {2, 3, 4}));
        Debug.Assert(2 == loader.cache.Count);
        
        Debug.Assert(loader.GetSampleData(SampleFrom(5, 12)).SequenceEqual(new byte[] {5, 6, 7, 8, 9, 10, 11}));
        Debug.Assert(1 == loader.cache.Count);
        Debug.Assert(loader.cache[5].Data.SequenceEqual(new byte[] {5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24}));

        Log.Info("All tests passed for StreamSampleLoader :)");
    }

    #endif
}