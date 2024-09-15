
using System.Runtime.InteropServices;

namespace Synthesizer;

public class Soundfont : IDisposable
{
    public Soundfont(SoundfontFile file)
    {
        // Fill in all the metadata
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

        // Create the sample loader
        SampleLoader = new StreamSampleLoader(
            new FileStream(file.FilePath, FileMode.Open, FileAccess.Read),
            file.SampleChunkStart, file.SampleChunkSize
        );

        Banks = [];
        
        // Parse the presets
        for (int iPreset = 0; iPreset < file.PresetHeaders.Count - 1; iPreset++)
        {
            var sfPreset = file.PresetHeaders[iPreset];
            var sfPresetNext = file.PresetHeaders[iPreset + 1];

            var preset = new Preset()
            {
                Name = sfPreset.Name,
                PresetNumber = sfPreset.PresetNumber,
                PresetZones = [],
                Library = sfPreset.Library,
                Genre = sfPreset.Genre,
                Morphology = sfPreset.Morphology,
            };
            
            // Parse each zone (generators + modulators + instruments)
            // The # of zones is the difference in the zone indices between the next preset and this one
            for (int iPZone = sfPreset.ZoneIndex;
                iPZone < sfPresetNext.ZoneIndex;
                iPZone++)
            {
                var sfPZone = file.PresetZones[iPZone];
                var sfPZoneNext = file.PresetZones[iPZone + 1];
                
                // Get the generators and modulators from the file
                // Once again, use the difference in indices between zones to find out # of gens/mods
                List<Generator> pzoneGenerators =
                    file.PresetGenerators[sfPZone.GeneratorIndex..sfPZoneNext.GeneratorIndex];
                List<Modulator> pzoneModulators =
                    file.PresetModulators[sfPZone.ModulatorIndex..sfPZoneNext.ModulatorIndex];
                
                // If the last generator is Instrument, use its value as the index to the instruments list
                if (pzoneGenerators.Count > 0 && pzoneGenerators[^1].GenOper == GeneratorType.Instrument)
                {
                    var pzone = new PresetZone
                    {
                        // Last generator is Instrument, so ignore it
                        Generators = pzoneGenerators[..^1],
                        Modulators = pzoneModulators,
                        // Parse the instrument
                        Instrument = ParseInstrument(pzoneGenerators[^1].GenAmount.UShortValue, file)
                    };

                    // Add it to the zones
                    preset.PresetZones.Add(pzone);
                }
                // If the last generator isn't Instrument, but this is the first zone, this zone is a global zone
                else if (iPZone == sfPreset.ZoneIndex)
                {
                    // If the global zone is the only zone, throw an error--we need zones for the instruments
                    if (sfPreset.ZoneIndex + 1 == sfPresetNext.ZoneIndex)
                        throw new Exception("Preset must have at least one zone with an Instrument generator last");
                   
                    // Set the global gens/mods
                    preset.GlobalGenerators = pzoneGenerators;
                    preset.GlobalModulators = pzoneModulators;
                }
                else
                    throw new Exception("Preset zone must have at least one generator");
            }

            if (!Banks.ContainsKey(sfPreset.BankNumber))
                Banks.Add(sfPreset.BankNumber, new() { BankNumber = sfPreset.BankNumber, Presets = [] });
            
            Banks[sfPreset.BankNumber].Presets.Add(preset);
        }
    }

    private static Instrument ParseInstrument(ushort instrumentIndex, SoundfontFile file)
    {
        var sfInstrument = file.Instruments[instrumentIndex];
        var sfInstrumentNext = file.Instruments[instrumentIndex + 1];
        var instrument = new Instrument()
        {
            Name = sfInstrument.Name,
            InstrumentZones = []
        };

        // Parse the instrument zones (generators + modulators + samples)
        // The # of zones is the difference in the zone indices between the next instrument and this one
        for (int iIZone = sfInstrument.ZoneIndex;
            iIZone < sfInstrumentNext.ZoneIndex;
            iIZone++)
        {
            var sfIZone = file.InstrumentZones[iIZone];
            var sfIZoneNext = file.InstrumentZones[iIZone + 1];
            
            // Get the generators and modulators from the file
            // Once again, use the difference in indices between zones to find out # of gens/mods
            List<Generator> izoneGenerators =
                file.InstrumentGenerators[sfIZone.GeneratorIndex..sfIZoneNext.GeneratorIndex];
            List<Modulator> izoneModulators =
                file.InstrumentModulators[sfIZone.ModulatorIndex..sfIZoneNext.ModulatorIndex];
            
            // If the last generator is SampleID, use its value as the index to the sample header list
            if (izoneGenerators.Count > 0 && izoneGenerators[^1].GenOper == GeneratorType.SampleID)
            {
                var izone = new InstrumentZone()
                {
                    // Last generator is SampleID, so ignore it
                    Generators = izoneGenerators[..^1],
                    Modulators = izoneModulators  
                };

                var sfSample = file.SampleHeaders[izoneGenerators[^1].GenAmount.UShortValue];
                
                // Get the info from the file
                izone.Sample = new()
                {
                    Name = sfSample.Name,
                    StartIndex = sfSample.StartIndex,
                    EndIndex = sfSample.EndIndex,
                    LoopStartIndex = sfSample.LoopStartIndex,
                    LoopEndIndex = sfSample.LoopEndIndex,
                    SampleRate = sfSample.SampleRate,
                    OriginalPitch = sfSample.OriginalPitch,
                    PitchCorrection = sfSample.PitchCorrection,
                    SampleLink = sfSample.SampleLink,
                    SampleType = sfSample.SampleType
                };
                
                // Add it to the zones
                instrument.InstrumentZones.Add(izone);
            }
            // If the last generator isn't SampleID, but this is the first zone, this zone is a global zone
            else if (iIZone == sfInstrument.ZoneIndex)
            {
                // If the global zone is the only zone, throw an error--we need zones for the samples
                if (sfInstrument.ZoneIndex + 1 == sfInstrumentNext.ZoneIndex)
                    throw new Exception("Instrument must have at least one zone with a SampleID generator last");

                // Set global gens/mods
                instrument.GlobalGenerators = izoneGenerators;
                instrument.GlobalModulators = izoneModulators;
            }
            else
                throw new Exception("Instrument zone must have at least one generator");
        }

        return instrument;
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

    public SortedDictionary<int, Bank> Banks;

    public ISampleLoader SampleLoader;

    ~Soundfont() => Dispose();

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        SampleLoader.Dispose();
    }
    
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

public struct Bank
{
    public ushort BankNumber;
    public SortedSet<Preset> Presets;

    public override readonly string ToString()
    {
        return $"Bank #{BankNumber}, {Presets.Count} Presets";
    }
}

public struct Preset : IComparable<Preset>
{
    public string Name; 
    public ushort PresetNumber;
    public List<Generator>? GlobalGenerators;
    public List<Modulator>? GlobalModulators;
    public List<PresetZone> PresetZones;
    public uint Library;
    public uint Genre;
    public uint Morphology;

    public readonly int CompareTo(Preset other)
    {
        return PresetNumber - other.PresetNumber;
    }

    public override readonly string ToString()
    {
        return $"{Name}, Preset #{PresetNumber}, {PresetZones.Count} Instruments";
    }
}

public struct PresetZone
{
    public List<Generator> Generators;
    public List<Modulator>? Modulators;
    public Instrument Instrument;

    public override readonly string ToString()
    {
        return Instrument.Name;
    }
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct Modulator
{
    public ModulatorType SrcOper;
    public GeneratorType DestOper;
    public short Amount;
    public ModulatorType AmtSrcOper;
    public Transform TransfOper;

    public override readonly string ToString()
    {
        return $"Source: ({SrcOper}), Amount Source: ({AmtSrcOper}), Dest: {DestOper}, Transf: {TransfOper}, Amt: {Amount}";
    }
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct Generator
{
    public GeneratorType GenOper;
    public GenAmount GenAmount;

    public override readonly string ToString()
    {
        return GenOper.ToString();
    }
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct GenAmount
{
    public ushort UShortValue;
    public readonly short ShortValue => (short)UShortValue;
    public readonly (byte Low, byte High) RangeValue => ((byte)(UShortValue >> 8), (byte)(UShortValue & 0xFF));
}

public struct Instrument
{
    public string Name;
    public List<Generator>? GlobalGenerators;
    public List<Modulator>? GlobalModulators;
    public List<InstrumentZone> InstrumentZones;

    public override readonly string ToString()
    {
        return $"{Name}, {InstrumentZones.Count} Samples";
    }
}

public struct InstrumentZone
{
    public List<Generator> Generators;
    public List<Modulator>? Modulators;
    public Sample Sample;

    public override readonly string ToString()
    {
        return Sample.Name;
    }
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

    public override readonly string ToString()
    {
        return Name;
    }
}