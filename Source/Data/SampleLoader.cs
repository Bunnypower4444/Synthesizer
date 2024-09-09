
namespace Synthesizer;

public interface ISampleLoader : IDisposable
{
    public byte[] GetSampleData(Sample sample);
}

/// <summary>
/// Loads and caches samples from a stream.
/// </summary>
public class StreamSampleLoader : ISampleLoader
{
    public StreamSampleLoader(Stream stream, long sampleChunkStart, long sampleChunkSize)
    {
        Reader = new(stream);
    }

    public BinaryReader Reader;
    public long SampleChunkStart;
    public long SampleChunkSize;

    public byte[] GetSampleData(Sample sample)
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