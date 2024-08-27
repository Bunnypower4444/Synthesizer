
namespace Synthesizer;

public static class BinaryReaderExt
{
    public static void SkipBytes(this BinaryReader reader, long bytes)
    {
        reader.BaseStream.Position += bytes;
    }

    public static string ReadFourCC(this BinaryReader reader)
    {
        return reader.ReadChars(4).AsSpan().ToString();
    }
}