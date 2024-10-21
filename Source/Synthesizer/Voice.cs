
// Define StreamSampleLoaderTests in Synthesizer.csproj to run the tests
#if GenModOverrideTests
using System.Diagnostics;
using static Synthesizer.GeneratorType;
#endif


namespace Synthesizer;

public class Voice
{
    public enum PlayingStatus
    { On, Released, Off }

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

        if (!Soundfont.SampleLoader.SampleIsCached(Sample))
            Soundfont.SampleLoader.PreloadSample(Sample);
        
        Modulators = Modulator.CleanModulators(Defaults.Modulators, modulatorsGlPreset, modulatorsPreset, modulatorsGlInst, modulatorsInst);

        SynthParams = Defaults.GetDefaultParameters();

        Generator.ApplyGenerators(SynthParams, generatorsGlPreset, generatorsPreset, generatorsGlInst, generatorsInst);
    }

    public readonly Soundfont Soundfont;
    public readonly int PresetNum;
    public readonly Sample Sample;
    public readonly byte Key, Velocity;

    private readonly List<Modulator> Modulators;
    private readonly SynthParams SynthParams;

    public float Time;
    public PlayingStatus Status = PlayingStatus.On;

    public byte[]? Update(float delta)
    {
        if (Status == PlayingStatus.Off)
            return null;

        var startIndex = (uint)(Time * Sample.SampleRate + Sample.StartIndex);
        Time += delta;
        var endIndex = (uint)(Time * Sample.SampleRate + Sample.StartIndex);

        return Soundfont.SampleLoader.GetSampleData(Sample with { StartIndex = startIndex, EndIndex = endIndex });
    }

    public void Release()
    {
        Status = PlayingStatus.Released;
    }

    public void Stop()
    {
        Status = PlayingStatus.Off;
    }

    #if GenModOverrideTests

    public static void Main()
    {
        // GENERATORS
        static int Default(GeneratorType genType)
            => Defaults.Generators[genType].Default;

        var synthParams = Defaults.GetDefaultParameters();
        Generator.ApplyGenerators(
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

        var cleanedMods = Modulator.CleanModulators(
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