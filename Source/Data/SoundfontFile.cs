
using System.Runtime.InteropServices;
using System.Text;

namespace Synthesizer;

public class SoundfontFile
{
    public static class ChunkName
    {
        // LEVEL 0
        public const string Info                    = "INFO";
        public const string Samples              = "sdta";
        public const string Presets              = "pdta";
        
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
        public const string SampleData                = "smpl";
        
        //  pdta
        public const string PresetHeaders           = "phdr";
        public const string PresetZones             = "pbag";
        public const string PresetModulators        = "pmod";
        public const string PresetGenerators        = "pgen";
        public const string Instruments             = "inst";
        public const string InstrumentZones         = "ibag";
        public const string InstrumentModulators    = "imod";
        public const string InstrumentGenerators    = "igen";
        public const string SampleHeaders           = "shdr";
    }

     #region Subclasses/structs

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct PresetHeader
    {
        public fixed sbyte RawName[20]; 
        public ushort PresetNumber;
        public ushort BankNumber;
        public ushort ZoneIndex;
        public uint Library;
        public uint Genre;
        public uint Morphology;

        public string Name
        {
            get
            {
                fixed (PresetHeader* it = &this)
                {
                    return Encoding.UTF8.GetString((byte*)it->RawName, 20).TrimEnd('\0');
                }
            }
            set
            {
                var bytes = Encoding.UTF8.GetBytes(value);
                fixed (PresetHeader* it = &this)
                {
                    var span = new Span<sbyte>(it->RawName, 20);
                    span.Clear();
                    bytes.Select(by => (sbyte)by).ToArray()[..20].CopyTo(span);
                }
            }
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct PresetZone
    {
        public ushort GeneratorIndex;
        public ushort ModulatorIndex;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct Instrument
    {
        public fixed sbyte RawName[20];
        public ushort ZoneIndex;
        
        public string Name
        {
            get
            {
                fixed (Instrument* it = &this)
                {
                    return Encoding.UTF8.GetString((byte*)it->RawName, 20).TrimEnd('\0');
                }
            }
            set
            {
                var bytes = Encoding.UTF8.GetBytes(value);
                fixed (Instrument* it = &this)
                {
                    var span = new Span<sbyte>(it->RawName, 20);
                    span.Clear();
                    bytes.Select(by => (sbyte)by).ToArray()[..20].CopyTo(span);
                }
            }
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct InstrumentZone
    {
        public ushort GeneratorIndex;
        public ushort ModulatorIndex;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct Sample
    {
        public fixed sbyte RawName[20];
        public uint StartIndex;
        public uint EndIndex;
        public uint LoopStartIndex;
        public uint LoopEndIndex;
        public uint SampleRate;
        public byte OriginalPitch;
        public sbyte PitchCorrection;
        public ushort SampleLink;
        public SampleLink SampleType;

        public string Name
        {
            get
            {
                fixed (Sample* i = &this)
                {
                    return Encoding.UTF8.GetString((byte*)i->RawName, 20).TrimEnd('\0');
                }
            }
            set
            {
                var bytes = Encoding.UTF8.GetBytes(value);
                fixed (Sample* it = &this)
                {
                    var span = new Span<sbyte>(it->RawName, 20);
                    span.Clear();
                    bytes.Select(by => (sbyte)by).ToArray()[..20].CopyTo(span);
                }
            }
        }
    }

    #endregion

    #region Data

    // Info
    public required Version FormatVersion;
    public required string TargetEngine;
    public required string Name;
    public string? ROMName;
    public Version? ROMVersion;
    public string? DateOfCreation;
    public string? DesignersAndEngineers;
    public string? ProductFor;
    public string? Copyright;
    public string? Comments;
    public string? Tools;

    // Sample data
    // public string Samples                 = "smpl";
    
    // Preset data
    public required List<PresetHeader> PresetHeaders;
    public required List<PresetZone> PresetZones;
    public required List<Modulator> PresetModulators;
    public required List<Generator> PresetGenerators;
    public required List<Instrument> Instruments;
    public required List<InstrumentZone> InstrumentZones;
    public required List<Modulator> InstrumentModulators;
    public required List<Generator> InstrumentGenerators;
    public required List<Sample> SampleHeaders;

    #endregion

    public required string FilePath;
    public required uint SampleChunkStart;
    public required uint SampleChunkSize;

    public static SoundfontFile LoadFile(string path)
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
        
        BinaryReader ReadFrom(string chunkName)
        {
            reader.BaseStream.Position = chunkLookup[chunkName].Position;
            return reader;
        }

        BinaryReader? TryReadFrom(string chunkName)
        {
            if (chunkLookup.ContainsKey(chunkName))
                return ReadFrom(chunkName);
            return null;
        }

        /* const string EOP = "EOP\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0";

        // Preset headers
        List<PresetHeader> presets = [];
        ReadFrom(ChunkName.PresetHeaders);

        while (reader.BaseStream.Position < chunkLookup[ChunkName.PresetHeaders].Size + chunkLookup[ChunkName.PresetHeaders].Position &&
            reader.ReadStruct<PresetHeader>() is {} header && header.Name != EOP)
            presets.Add(header); */

        List<T> ReadListOf<T>(string chunk) where T : unmanaged
        {
            List<T> list = [];
            ReadFrom(chunk);

            while (reader.BaseStream.Position < chunkLookup[chunk].Size + chunkLookup[chunk].Position)
                list.Add(reader.ReadStruct<T>());
            
            return list;
        }

        var presets = ReadListOf<PresetHeader>(ChunkName.PresetHeaders);
        var presetZones = ReadListOf<PresetZone>(ChunkName.PresetZones);
        var presetModulators = ReadListOf<Modulator>(ChunkName.PresetModulators);
        var presetGenerators = ReadListOf<Generator>(ChunkName.PresetGenerators);
        var instruments = ReadListOf<Instrument>(ChunkName.Instruments);
        var instrumentZones = ReadListOf<InstrumentZone>(ChunkName.InstrumentZones);
        var instrumentModulators = ReadListOf<Modulator>(ChunkName.InstrumentModulators);
        var instrumentGenerators = ReadListOf<Generator>(ChunkName.InstrumentGenerators);
        var sampleHeaders = ReadListOf<Sample>(ChunkName.SampleHeaders);
        
        var soundfont = new SoundfontFile()
        {
            FilePath = path,
            SampleChunkStart = (uint)chunkLookup[ChunkName.SampleData].Position,
            SampleChunkSize = (uint)chunkLookup[ChunkName.SampleData].Size,
            FormatVersion = ReadFrom(ChunkName.SoundfontFormatVersion).ReadVersion(),
            TargetEngine = ReadFrom(ChunkName.TargetEngine).ReadString(chunkLookup[ChunkName.TargetEngine].Size),
            Name = TryReadFrom(ChunkName.SoundfontName)?.ReadString(chunkLookup[ChunkName.SoundfontName].Size) ?? "EMU8000",
            ROMName = TryReadFrom(ChunkName.ROMName)?.ReadString(chunkLookup[ChunkName.ROMName].Size),
            ROMVersion = TryReadFrom(ChunkName.ROMName)?.ReadVersion(),
            DateOfCreation = TryReadFrom(ChunkName.DateOfCreation)?.ReadString(chunkLookup[ChunkName.DateOfCreation].Size),
            DesignersAndEngineers = TryReadFrom(ChunkName.DesignersAndEngineers)?.ReadString(chunkLookup[ChunkName.DesignersAndEngineers].Size),
            ProductFor = TryReadFrom(ChunkName.ProductFor)?.ReadString(chunkLookup[ChunkName.ProductFor].Size),
            Copyright = TryReadFrom(ChunkName.Copyright)?.ReadString(chunkLookup[ChunkName.Copyright].Size),
            Comments = TryReadFrom(ChunkName.Comments)?.ReadString(chunkLookup[ChunkName.Comments].Size),
            Tools = TryReadFrom(ChunkName.Tools)?.ReadString(chunkLookup[ChunkName.Tools].Size),
            PresetHeaders = presets,
            PresetZones = presetZones,
            PresetModulators = presetModulators,
            PresetGenerators = presetGenerators,
            Instruments = instruments,
            InstrumentZones = instrumentZones,
            InstrumentModulators = instrumentModulators,
            InstrumentGenerators = instrumentGenerators,
            SampleHeaders = sampleHeaders
        };
         
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

        return soundfont;
    }

    public override string ToString()
    {
        static string FormatOptional(string label, string? value)
            => string.IsNullOrEmpty(value) ? "" : label + ": " + value + "\n";

        return
            $"Soundfont Version: {FormatVersion}\n" +
            $"Target Engine: {TargetEngine}\n" +
            $"Name: {Name} \n" +
            FormatOptional("ROM Name", ROMName) +
            FormatOptional("ROM Version", ROMVersion?.ToString()) +
            FormatOptional("Date of Creation", DateOfCreation) +
            FormatOptional("Sound Designers and Engineers", DesignersAndEngineers) +
            FormatOptional("Product For", ProductFor) +
            FormatOptional("Copyright Message", Copyright) +
            FormatOptional("Comments", Comments) +
            FormatOptional("Tools Used", Tools)
            ;
    }
}