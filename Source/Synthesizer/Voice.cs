
namespace Synthesizer;

public class Voice
{
    public class SynthParam
    {
        public SynthParam(GenAmount baseValue)
        {
            BaseValue = baseValue;
        }

        public SynthParam(GenAmount baseValue, float modValue)
        {
            BaseValue = baseValue;
            ModValue = modValue;
        }

        public GenAmount BaseValue;
        public float ModValue;
        public float SignedTotal => BaseValue.AsShort + ModValue;
        public float UnsignedTotal => BaseValue.AsUShort + ModValue;
    }

    public Voice(Sample sample, byte key, byte velocity,
        List<Generator> generatorsGlPreset,
        List<Generator> generatorsPreset,
        List<Generator> generatorsGlInst,
        List<Generator> generatorsInst,
        List<Modulator> modulatorsGlPreset,
        List<Modulator> modulatorsPreset,
        List<Modulator> modulatorsGlInst,
        List<Modulator> modulatorsInst)
    {
        Sample = sample;
        Key = key;
        Velocity = velocity;
        
        Modulators = CleanModulators(Defaults.Modulators, modulatorsGlPreset, modulatorsPreset, modulatorsGlInst, modulatorsInst);

        // basically copy over just the default value from the generator info
        SynthParams = new
        (
            Defaults.Generators.Select
            (
                pair => KeyValuePair.Create<GeneratorType, SynthParam>(pair.Key, new((GenAmount)pair.Value.Default))
            )
        );

        ApplyGenerators(generatorsGlPreset, generatorsPreset, generatorsGlInst, generatorsInst);
    }

    private static List<Modulator> CleanModulators(
        Modulator[] modulatorsDefault,
        List<Modulator> modulatorsGlPreset,
        List<Modulator> modulatorsPreset,
        List<Modulator> modulatorsGlInst,
        List<Modulator> modulatorsInst)
    {
        List<Modulator> cleanedMods = [];

        // Local replaces global (both inst and preset), then put them at the end of the
        //  local modulators because why not
        int numLocal = modulatorsInst.Count;
        foreach (var glInst in modulatorsGlInst)
        {
            bool keep = true;
            for (int i = 0; i < numLocal; i++)
            {
                if (modulatorsInst[i].IdenticalTo(glInst))
                {
                    keep = false;
                    break;
                }
            }

            if (keep)
                modulatorsInst.Add(glInst);
        }

        numLocal = modulatorsPreset.Count;
        foreach (var glPreset in modulatorsGlPreset)
        {
            bool keep = true;
            for (int i = 0; i < numLocal; i++)
            {
                if (modulatorsPreset[i].IdenticalTo(glPreset))
                {
                    keep = false;
                    break;
                }
            }

            if (keep)
                modulatorsPreset.Add(glPreset);
        }

        // Instrument modulators supersede default modulators
        foreach (var defaultMod in modulatorsDefault)
        {
            bool keep = true;
            foreach (var instMod in modulatorsInst)
            {
                if (instMod.IdenticalTo(defaultMod))
                {
                    keep = false;
                    break;
                }
            }

            // put into the final list
            if (keep)
                cleanedMods.Add(defaultMod);
        }

        // put everything else in the final list
        cleanedMods.AddRange(modulatorsInst);
        cleanedMods.AddRange(modulatorsPreset);

        return cleanedMods;
    }

    private void ApplyGenerators(
        List<Generator> generatorsGlPreset,
        List<Generator> generatorsPreset,
        List<Generator> generatorsGlInst,
        List<Generator> generatorsInst)
    {
        // first do global inst, then local inst, so local can override global
        foreach (var gigen in generatorsGlInst)
        {
            if (gigen.GenOper.IsNotAllowed() || gigen.GenOper.IsNonRealTime())
            {
                Log.Info($"Generator {gigen.GenOper} is not allowed");
                continue;
            }

            SynthParams[gigen.GenOper].BaseValue = gigen.GenAmount;
        }

        foreach (var igen in generatorsInst)
        {
            if (igen.GenOper.IsNotAllowed() || igen.GenOper.IsNonRealTime())
            {
                Log.Info($"Generator {igen.GenOper} is not allowed");
                continue;
            }

            SynthParams[igen.GenOper].BaseValue = igen.GenAmount;
        }

        Dictionary<GeneratorType, GenAmount> presetGenOffset = [];
        foreach (var gpgen in generatorsGlPreset)
        {
            if (gpgen.GenOper.IsNotAllowed() || gpgen.GenOper.IsNonRealTime())
            {
                Log.Info($"Generator {gpgen.GenOper} is not allowed");
                continue;
            }

            if (gpgen.GenOper.IsInstrumentOnly())
            {
                Log.Info($"Generator {gpgen.GenOper} is not allowed in a preset");
                continue;
            }

            presetGenOffset[gpgen.GenOper] = gpgen.GenAmount;
        }

        foreach (var pgen in generatorsPreset)
        {
            if (pgen.GenOper.IsNotAllowed() || pgen.GenOper.IsNonRealTime())
            {
                Log.Info($"Generator {pgen.GenOper} is not allowed");
                continue;
            }

            if (pgen.GenOper.IsInstrumentOnly())
            {
                Log.Info($"Generator {pgen.GenOper} is not allowed in a preset");
                continue;
            }

            SynthParams[pgen.GenOper].BaseValue.AsUShort += pgen.GenAmount.AsUShort;
            presetGenOffset.Remove(pgen.GenOper);
        }

        foreach (var gpgenoffset in presetGenOffset)
        {
            SynthParams[gpgenoffset.Key].BaseValue.AsUShort += gpgenoffset.Value.AsUShort;
        }
    }

    public readonly Sample Sample;
    public readonly byte Key, Velocity;

    private readonly List<Modulator> Modulators;
    private readonly Dictionary<GeneratorType, SynthParam> SynthParams;

    public byte[] Update(float delta)
    {
        throw new NotImplementedException();
    }
}