
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

    public static Version ReadVersion(this BinaryReader reader)
    {
        var major = reader.ReadUInt16();
        var minor = reader.ReadUInt16();
        return new(major, minor);
    }

    public static string ReadString(this BinaryReader reader, int maxLength)
    {
        var chars = new char[maxLength];
        for (int i = 0; i < maxLength; i++)
        {
            char c = reader.ReadChar();
            if (c == 0)
                return chars[..i].AsSpan().ToString();
            else chars[i] = c;
        }

        return chars.AsSpan().ToString();
    }

    public static unsafe T ReadStruct<T>(this BinaryReader reader) where T : unmanaged
    {
        T it = new();
        Span<byte> buffer = new(&it, sizeof(T));
        reader.Read(buffer);
        return it;
    }
}