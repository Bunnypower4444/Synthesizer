
namespace Synthesizer;

public class Soundfont
{
    public Soundfont(SoundfontFile file)
    {
        FormatVersion = file.FormatVersion;
        TargetEngine = file.TargetEngine;
        Name = file.Name;
        ROMName = file.ROMName;
        ROMVersion = file.ROMVersion;
        DateOfCreation = file.DateOfCreation;
        DesignersAndEngineers = file.DesignersAndEngineers;
        ProductFor = file.ProductFor;
        Copyright = file.Copyright;
        Comments = file.Comments;
        Tools = file.Tools;
    }

    public Version FormatVersion;
    public string TargetEngine;
    public string Name;
    public string? ROMName;
    public Version? ROMVersion;
    public string? DateOfCreation;
    public string? DesignersAndEngineers;
    public string? ProductFor;
    public string? Copyright;
    public string? Comments;
    public string? Tools;

    public required Preset[] PresetHeaders;

    // Add fields
    
    /* public override string ToString()
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
    } */
}

public struct Preset
{
    public string Name; 
    public ushort PresetNumber;
    public ushort BankNumber;
    public Generator[] GlobalGenerators;
    public Modulator[] GlobalModulators;
    public PresetZone[] PresetZones;
    public uint Library;
    public uint Genre;
    public uint Morphology;
}

public struct PresetZone
{
    public Generator[] Generators;
    public Modulator[] Modulators;
    public Instrument Instrument;
}

public struct Modulator
{
    public ModulatorType SrcOper;
    public GeneratorType DestOper;
    public short Amount;
    public ModulatorType AmtSrcOper;
    public Transform TransOper;
}

public struct Generator
{
    public GeneratorType GenOper;
    public ushort GenAmount;
}

public struct Instrument
{
    public string Name;
    public Generator[] GlobalGenerators;
    public Modulator[] GlobalModulators;
    public InstrumentZone[] InstrumentZones;
}

public struct InstrumentZone
{
    public Generator[] Generators;
    public Modulator[] Modulators;
    public Sample Sample;
}

public struct Sample
{
    public string Name;
    public uint StartIndex;
    public uint EndIndex;
    public uint LoopStartIndex;
    public uint LoopEndIndex;
    public uint SampleRate;
    public byte OriginalPitch;
    public sbyte PitchCorrection;
    public ushort SampleLink;
    public SampleLink SampleType;
}