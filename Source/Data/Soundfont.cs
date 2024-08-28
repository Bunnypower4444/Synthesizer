
namespace Synthesizer;

public class Soundfont
{
    public readonly struct Chunk(int size, long chunkOffset)
    {
        public readonly int Size = size;
        public readonly long ChunkOffset = chunkOffset;
    }

    public static class ChunkName
    {
        // LEVEL 0
        public const string Info                    = "INFO";
        public const string SampleData              = "sdta";
        public const string PresetData              = "pdta";
        
        // LEVEL 1
        //  INFO
        public const string SoundfontFormatVersion  = "ifil";
        public const string TargetEngine            = "isng";
        public const string SoundfontName           = "INAM";
        public const string ROMName                 = "irom";
        public const string ROMVersion              = "iver";
        public const string DateOfCreation          = "ICRD";
        public const string DesignersAndEngineers   = "IENG";
        public const string ProductFor              = "IPRD";
        public const string Copyright               = "ICOP";
        public const string Comments                = "ICMT";
        public const string Tools                   = "ISFT";

        //  sdta
        public const string Samples                 = "smpl";
        
        // pdta
        public const string PresetHeaders           = "phdr";
        public const string PresetIndices           = "pbag";
    }

    // version (ifil, iver): is it a number or string?

    




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
        
        Dictionary<string, Chunk> chunkLookup = [("RIFF", new(0, 12))];
        
        while (reader.BaseStream.Position < reader.BaseStream.Length)
        {
            string chunkID = reader.ReadFourCC();
            int chunkSize = reader.ReadInt32();
            long position = reader.BaseStream.Position;
            string chunkType = reader.ReadFourCC();
            
            Dictionary.Add(chunkType, new(chunkSize, position));
            
            // LIST chunks may contain subchunks
            if (chunkID == "LIST")
            {
                while (reader.BaseStream.Position < position + size)
                {
                    // subchunks do not have a chunk type, just their chunk id and size
                    string subchunkID = reader.ReadFourCC();
                    int chunkSize = reader.ReadInt32();
                    long subchunkPosition = reader.BaseStream.Position;
                    
                    Dictionary.Add(chunkType, position);
                    
                    // add padding to 2 byte boundary
                    if (reader.BaseStream.Position % 2 != 0)
                        reader.SkipBytes(1);
                }
                
                // add padding to 2 byte boundary
                if (reader.BaseStream.Position % 2 != 0)
                    reader.SkipBytes(1);
            }
        }

        return soundfont;
    }
}