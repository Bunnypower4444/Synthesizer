
namespace Synthesizer;

public class Soundfont
{
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
        //      rest of INFO is optional
        public const string ROMName                 = "irom";
        public const string ROMVersion              = "iver";
        public const string DateOfCreation          = "ICRD";
        public const string DesignersAndEngineers   = "IENG";
        public const string ProductFor              = "IPRD";
        public const string Copyright               = "ICOP";
        public const string Comments                = "ICMT";
        public const string Tools                   = "ISFT";

        //  sdta
        //      optional
        public const string Samples                 = "smpl";
        
        //  pdta
        public const string PresetHeaders           = "phdr";
        public const string PresetIndices           = "pbag";
        public const string PresetModulators           = "pmod";
        public const string PresetGenerators           = "pgen";
        public const string Instuments           = "inst";
        public const string InstrumentIndices           = "ibag";
        public const string InstrumentModulators           = "imod";
        public const string InstrumentGenerators           = "igen";
        public const string SampleHeaders           = "shdr";
    }

    // Info
    public required float FormatVersion;
    public required string TargetEngine;
    public required string Name;
    public string? ROMName;
    public float? ROMVersion;
    public string? DateOfCreation;
    public string? DesignersAndEngineers;
    public string? ProductFor;
    public string? Copyright;
    public string? Comments;
    public string? Tools;

    // Sample data
    // public string Samples                 = "smpl";
    
    // Preset data
    // public required string PresetHeaders
    // public required string PresetIndices
    // public required string PresetModulators;
    // public required string PresetGenerators;
    // public required string Instuments;
    // public required string InstrumentIndices;
    // public required string InstrumentModulators;
    // public required string InstrumentGenerators;
    // public required string SampleHeaders;




    public static void LoadFile(string path)
    {
        if (Path.GetExtension(path) == ".sf3" || Path.GetExtension(path) == ".sfz")
            throw new Exception($"Unsupported soundfont file type ({Path.GetExtension(path)})");

        using var stream = new FileStream(path, FileMode.Open, FileAccess.Read);
        using var reader = new BinaryReader(stream);
        // NOTE so that I don't forget later: ENCODING IS UTF-8, EACH CHAR IS 1 BYTE

        string riffID = reader.ReadFourCC();
        if (riffID != "RIFF")
            throw new Exception("Invalid soundfont file");

        int fileSize = reader.ReadInt32();

        // check if file size + 8 matches actual size
        // if not, file is invalid
        if (fileSize + 8 != new FileInfo(path).Length)
            throw new Exception("Soundfont file is corrupted: file size does not match");

        // form type...? soundfont file doesn't have i think
        // nevermind, I forgot file size is 4 bytes not 8
        // check if form type is a soundfont
        string formType = reader.ReadFourCC();
        if (formType != "sfbk")
            throw new Exception("Invalid soundfont file");
        
        var chunkLookup = new Dictionary<string, (int Size, long Position)>();

        while (reader.BaseStream.Position < reader.BaseStream.Length)
        {
            string chunkID = reader.ReadFourCC();
            int chunkSize = reader.ReadInt32();
            long position = reader.BaseStream.Position;
            string chunkType = reader.ReadFourCC();
            
            chunkLookup.Add(chunkType, (chunkSize, position));
            
            // LIST chunks may contain subchunks
            if (chunkID == "LIST")
            {
                while (reader.BaseStream.Position < position + chunkSize)
                {
                    // subchunks do not have a chunk type, just their chunk id and size
                    string subchunkID = reader.ReadFourCC();
                    int subchunkSize = reader.ReadInt32();
                    long subchunkPosition = reader.BaseStream.Position;
                    
                    chunkLookup.Add(subchunkID, (subchunkSize, subchunkPosition));
                    
                    // skip to next chunk
                    reader.SkipBytes(subchunkSize);

                    // add padding to 2 byte boundary
                    if (reader.BaseStream.Position % 2 != 0)
                        reader.SkipBytes(1);
                }
            }

            // go to next chunk
            reader.BaseStream.Position = position + chunkSize;

            // add padding to 2 byte boundary
            if (reader.BaseStream.Position % 2 != 0)
                reader.SkipBytes(1);
        }

        foreach (var keyValue in chunkLookup)
        {
            Log.Info(keyValue.Key + ": " + keyValue.Value);
        }
        
        /* while (reader.BaseStream.Position < reader.BaseStream.Length)
        {
            string chunkID = reader.ReadFourCC();
            int chunkSize = reader.ReadInt32();
            long position = reader.BaseStream.Position;
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
            } *

            // reader.BaseStream.Position = chunkOffset + 12 + size;
        }*/

        // return soundfont;
    }
}