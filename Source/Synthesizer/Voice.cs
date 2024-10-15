
// Define StreamSampleLoaderTests in Synthesizer.csproj to run the tests
#if GenModOverrideTests
using System.Diagnostics;
using static Synthesizer.GeneratorType;
#endif


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

    public Voice(Soundfont soundfont, int presetNum, Sample sample, byte key, byte velocity,
        List<Generator> generatorsGlPreset,
        List<Generator> generatorsPreset,
        List<Generator> generatorsGlInst,
        List<Generator> generatorsInst,
        List<Modulator> modulatorsGlPreset,
        List<Modulator> modulatorsPreset,
        List<Modulator> modulatorsGlInst,
        List<Modulator> modulatorsInst)
    {
        Soundfont = soundfont;
        PresetNum = presetNum;
        Sample = sample;
        Key = key;
        Velocity = velocity;
        
        Modulators = CleanModulators(Defaults.Modulators, modulatorsGlPreset, modulatorsPreset, modulatorsGlInst, modulatorsInst);

        SynthParams = GetDefaultParameters();

        ApplyGenerators(SynthParams, generatorsGlPreset, generatorsPreset, generatorsGlInst, generatorsInst);
    }

    private static Dictionary<GeneratorType, SynthParam> GetDefaultParameters()
    {
        // basically copy over just the default value from the generator info
        return new
        (
            Defaults.Generators.Select
            (
                pair => KeyValuePair.Create<GeneratorType, SynthParam>(pair.Key, new((GenAmount)pair.Value.Default))
            )
        );
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

    private static void ApplyGenerators(
        Dictionary<GeneratorType, SynthParam> synthParams,
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

            synthParams[gigen.GenOper].BaseValue = gigen.GenAmount;
        }

        foreach (var igen in generatorsInst)
        {
            if (igen.GenOper.IsNotAllowed() || igen.GenOper.IsNonRealTime())
            {
                Log.Info($"Generator {igen.GenOper} is not allowed");
                continue;
            }

            synthParams[igen.GenOper].BaseValue = igen.GenAmount;
        }

        // Presets offset the value provided by the default/instrument generators
        // put the offsets from the global pre into a dictionary
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

        // apply offsets from local preset, remove corresponding global pre (override)
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

            synthParams[pgen.GenOper].BaseValue.AsUShort += pgen.GenAmount.AsUShort;
            presetGenOffset.Remove(pgen.GenOper);
        }

        // apply un-overriden global preset
        foreach (var gpgenoffset in presetGenOffset)
        {
            synthParams[gpgenoffset.Key].BaseValue.AsUShort += gpgenoffset.Value.AsUShort;
        }
    }

    public readonly Soundfont Soundfont;
    public readonly int PresetNum;
    public readonly Sample Sample;
    public readonly byte Key, Velocity;

    private readonly List<Modulator> Modulators;
    private readonly Dictionary<GeneratorType, SynthParam> SynthParams;

    public byte[] Update(float delta)
    {
        throw new NotImplementedException();
    }

    #if GenModOverrideTests

    public static void Main()
    {
        // GENERATORS
        static int Default(GeneratorType genType)
            => Defaults.Generators[genType].Default;

        var synthParams = GetDefaultParameters();
        ApplyGenerators(
            synthParams,
            [new(DelayModEnv, 8), new(ReleaseModEnv, 8), new(SustainModEnv, 8), new(DecayModEnv, 8)],
            [new(DelayModEnv, 4), new(ReleaseModEnv, 4), new(AttackModEnv, 4), new(HoldModEnv, 4)],
            [new(DelayModEnv, 2), new(ReleaseVolEnv, 2), new(SustainVolEnv, 2), new(HoldModEnv, 2)],
            [new(DelayModEnv, 1), new(ReleaseVolEnv, 1), new(AttackVolEnv, 1), new(DecayModEnv, 1)]
            );

        // loc inst overrides glob inst (1), loc pre overrides glob pre (+4)
        Debug.Assert(synthParams[DelayModEnv].SignedTotal == 5);
        // default + loc pre overrides glob pre (+4)
        Debug.Assert(synthParams[ReleaseModEnv].SignedTotal == Default(ReleaseModEnv) + 4);
        // default + glob pre (+8)
        Debug.Assert(synthParams[SustainModEnv].SignedTotal == Default(SustainModEnv) + 8);
        // loc inst (1) + glob pre (+8)
        Debug.Assert(synthParams[DecayModEnv].SignedTotal == 9);

        // default + loc pre (+4)
        Debug.Assert(synthParams[AttackModEnv].SignedTotal == Default(AttackModEnv) + 4);
        // glob inst (2) + loc pre (+4)
        Debug.Assert(synthParams[HoldModEnv].SignedTotal == 6);

        // loc inst (1) overrides glob inst
        Debug.Assert(synthParams[ReleaseVolEnv].SignedTotal == 1);
        // glob inst (2)
        Debug.Assert(synthParams[SustainVolEnv].SignedTotal == 2);
        // loc inst (1)
        Debug.Assert(synthParams[AttackVolEnv].SignedTotal == 1);

        // MODULATORS
        static Modulator Mod(GeneratorType dest, ushort src, ushort amtsrc, short amt)
        {
            return new Modulator()
            {
                DestOper = dest,
                SrcOper = new() { Value = src },
                AmtSrcOper = new() { Value = amtsrc },
                Amount = amt,
                TransfOper = Transform.Linear
            };
        }

        var cleanedMods = CleanModulators(
            [Mod(StartAddrsOffset, 0, 0, 16), Mod(EndAddrsOffset, 1, 1, 16), Mod(StartLoopAddrsOffset, 2, 2, 16), Mod(EndLoopAddrsOffset, 3, 3, 16)],
            [Mod(StartAddrsOffset, 0, 0, 8), Mod(EndAddrsOffset, 1, 2, 8), Mod(StartLoopAddrsOffset, 2, 3, 8), Mod(EndLoopAddrsOffset, 2, 3, 8)],
            [Mod(StartAddrsOffset, 0, 0, 4), Mod(EndAddrsOffset, 1, 1, 4), Mod(StartLoopAddrsOffset, 2, 3, 4), Mod(EndLoopAddrsOffset, 3, 2, 4)],
            [Mod(StartAddrsOffset, 0, 0, 2), Mod(EndAddrsOffset, 1, 2, 2), Mod(StartLoopAddrsOffset, 2, 2, 2), Mod(EndLoopAddrsOffset, 2, 3, 2)],
            [Mod(StartAddrsOffset, 0, 0, 1), Mod(EndAddrsOffset, 1, 1, 1), Mod(StartLoopAddrsOffset, 2, 3, 1), Mod(EndLoopAddrsOffset, 3, 2, 1)]
        );

        Debug.Assert(cleanedMods.Count == 14);
        // CleanModulators() returns the list in this order: def, loc inst, glob inst, loc pre, glob pre
        Debug.Assert(cleanedMods.SequenceEqual([
            Mod(EndLoopAddrsOffset, 3, 3, 16),
            Mod(StartAddrsOffset, 0, 0, 1), Mod(EndAddrsOffset, 1, 1, 1), Mod(StartLoopAddrsOffset, 2, 3, 1), Mod(EndLoopAddrsOffset, 3, 2, 1),
            Mod(EndAddrsOffset, 1, 2, 2), Mod(StartLoopAddrsOffset, 2, 2, 2), Mod(EndLoopAddrsOffset, 2, 3, 2),
            Mod(StartAddrsOffset, 0, 0, 4), Mod(EndAddrsOffset, 1, 1, 4), Mod(StartLoopAddrsOffset, 2, 3, 4), Mod(EndLoopAddrsOffset, 3, 2, 4),
            Mod(EndAddrsOffset, 1, 2, 8), Mod(EndLoopAddrsOffset, 2, 3, 8)
        ]), "Received " + string.Join(", ", cleanedMods.Select(mod => $"{mod.DestOper}, {mod.SrcOper.Value}, {mod.AmtSrcOper.Value}, {mod.Amount}")));

        Log.Info("All tests passed for GenModOverride :)");
    }

    #endif
}