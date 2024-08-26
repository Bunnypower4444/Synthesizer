
namespace Synthesizer;

public class Soundfont
{
    public readonly struct Chunk(int size, long chunkOffset)
    {
        public readonly int Size = size;
        public readonly long ChunkOffset = chunkOffset;
    }

    public readonly Dictionary<string, Chunk> Chunks = [];

    public static Soundfont LoadFile(string path)
    {
        if (Path.GetExtension(path) == ".sf3" || Path.GetExtension(path) == ".sfz")
            throw new Exception($"Unsupported soundfont file type ({Path.GetExtension(path)})");

        using var stream = new FileStream(path, FileMode.Open, FileAccess.Read);
        using var reader = new BinaryReader(stream);
        // NOTE so that I don't forget later: ENCODING IS UTF-8, EACH CHAR IS 1 BYTE

        string riffID = reader.ReadFourCC();
        if (riffID != "RIFF")
            throw new Exception("Invalid soundfont file");

        // skip the file size
        reader.SkipBytes(8);

        // form type...?
        // Log.Info(reader.ReadFourCC());
        
        var soundfont = new Soundfont();
        
        while (reader.BaseStream.Position < reader.BaseStream.Length)
        {
            long chunkOffset = reader.BaseStream.Position;
            string chunkID = reader.ReadFourCC();
            int size = reader.ReadInt32();
            string chunkType = reader.ReadFourCC();
            Log.Info(chunkID + "-" + chunkType + ":" + chunkOffset);

            soundfont.Chunks.TryAdd(chunkType, new(size, chunkOffset));
            // the chunkType is included in the size, so subtract 4 characters
            reader.BaseStream.Position += size - 4;

            // add padding to 4-byte boundary
            reader.SkipBytes((4 - reader.BaseStream.Position % 4) % 4);

            /* if (chunkID == "LIST")
            {
                long subchunkOffset = reader.BaseStream.Position;
                string subchunkID = reader.ReadFourCC();
                Log.Info(subchunkID);
                int subchunkSize = reader.ReadInt32();
                soundfont.Chunks.TryAdd(subchunkID, new(subchunkSize, subchunkOffset));
                reader.BaseStream.Position += subchunkSize;
            } */

            // reader.BaseStream.Position = chunkOffset + 12 + size;
        }

        return soundfont;
    }
}