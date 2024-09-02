
using System.Runtime.InteropServices;
using System.Text;

namespace Synthesizer;

public class Soundfont
{

    #region Enums

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
        public const string PresetZones           = "pbag";
        public const string PresetModulators        = "pmod";
        public const string PresetGenerators        = "pgen";
        public const string Instruments              = "inst";
        public const string InstrumentZones       = "ibag";
        public const string InstrumentModulators    = "imod";
        public const string InstrumentGenerators    = "igen";
        public const string SampleHeaders           = "shdr";
    }

    public enum GeneratorType : ushort
    {
        StartAddrsOffset = 0,
        EndAddrsOffset = 1,
        StartLoopAddrsOffset = 2,
        EndLoopAddrsOffset = 3,
        StartAddrsOffsetCoarseOffset = 4,
        ModLfoToPitch = 5,
        VibLfoToPitch = 6,
        ModEnvToPitch = 7,
        InitialFilterFc = 8,
        InitialFilterQ = 9,
        ModLfoToFilterFc = 10,
        ModEnvToFilterFc = 11,
        EndAddrsCoarseOffset = 12,
        ModLfoToVolume = 13,
        Unused1 = 14,
        ChorusEffectsSend = 15,
        ReverbEffectsSend = 16,
        Pan = 17,
        Unused2 = 18,
        Unused3 = 19,
        Unused4 = 20,
        DelayModLFO = 21,
        FreqModLFO = 22,
        DelayVibLFO = 23,
        FreqVibLFO = 24,
        DelayModEnv = 25,
        AttackModEnv = 26,
        HoldModEnv = 27,
        DecayModEnv = 28,
        SustainModEnv = 29,
        ReleaseModEnv = 30,
        KeynumToModEnvHold = 31,
        KeynumToModEnvDecay = 32,
        DelayVolEnv = 33,
        AttackVolEnv = 34,
        HoldVolEnv = 35,
        DecayVolEnv = 36,
        SustainVolEnv = 37,
        ReleaseVolEnv = 38,
        KeynumToVolEnvHold = 39,
        KeynumToVolEnvDecay = 40,
        Instrument = 41,
        Reserved1 = 42,
        KeyRange = 43,
        VelRange = 44,
        StartLoopAddrsCoarseOffset = 45,
        Keynum = 46,
        Velocity = 47,
        InitialAttenuation = 48,
        Reserved2 = 49,
        EndLoopAddrsCoarseOffset = 50,
        CoarseTune = 51,
        FineTune = 52,
        SampleID = 53,
        SampleModes = 54,
        Reserved3 = 55,
        ScaleTuning = 56,
        ExclusiveClass = 57,
        OverridingRootKey = 58,
        Unused5 = 59,
        EndOper = 60
    }

    public record struct ModulatorType
    {
        /// <summary>
        /// The raw binary value of the modulator type.
        /// Bits 0-6: Index
        /// Bit 7: MIDI Continuous Controller (CC) Flag
        /// Bit 8: Direction
        /// Bit 9: Polarity
        /// Bits 10-15: Type
        /// </summary>
        public ushort Value;

        public readonly byte SourceIndex => (byte)(Value & 0b01111111);
        public readonly bool ContinuousController => (Value & 0b10000000) > 0;
        public readonly bool Direction => (Value & 0b1_00000000) > 0;
        public readonly bool Polarity => (Value & 0b10_00000000) > 0;
        public readonly byte Type => (byte)(Value >> 10);
    }

    public enum Transform : ushort
    {

    }

    public enum SampleLink : ushort
    {
        MonoSample = 1, RightSample = 2, LeftSample = 4, LinkedSample = 8,
        RomMonoSample = 32769, RomRightSample = 32770, RomLeftSample = 32772, RomLinkedSample = 32776
    }

    #endregion

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
                    return Encoding.UTF8.GetString((byte*)it->RawName, 20);
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
    public struct Modulator
    {
        public ModulatorType ModSrcOper;
        public GeneratorType ModDestOper;
        public short ModAmount;
        public ModulatorType ModAmtSrcOper;
        public Transform ModTransOper;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Generator
    {
        public GeneratorType GenOper;
        public GenAmount GenAmount;
    }

    public struct GenAmount
    {
        // Most use the ushort format (but how do you tell what it actaully is???)
        public ushort UShortValue;
        public readonly short ShortValue => (short)UShortValue;
        public readonly (byte Low, byte High) RangeValue => ((byte)(UShortValue >> 8), (byte)(UShortValue & 0xFF));
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
                    return Encoding.UTF8.GetString((byte*)it->RawName, 20);
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
                    return Encoding.UTF8.GetString((byte*)i->RawName, 20);
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
    // public required List<PresetZone> PresetZones;
    // public required List<Modulator> PresetModulators;
    // public required List<Generator> PresetGenerators;
    // public required List<Instrument> Instuments;
    // public required List<InstrumentZone> InstrumentZones;
    // public required List<Modulator> InstrumentModulators;
    // public required List<Generator> InstrumentGenerators;
    // public required List<Sample> SampleHeaders;

    #endregion

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

        const string EOP = "EOP\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0";

        // Preset headers
        List<PresetHeader> presets = [];
        ReadFrom(ChunkName.PresetHeaders);
        var terminalRecord = default(PresetHeader) with { Name = EOP };
        while (reader.BaseStream.Position < chunkLookup[ChunkName.PresetHeaders].Size + chunkLookup[ChunkName.PresetHeaders].Position &&
            reader.ReadStruct<PresetHeader>() is {} header && !header.Equals(terminalRecord))
            presets.Add(header);
        
        var soundfont = new Soundfont()
        {
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
            PresetHeaders = presets
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