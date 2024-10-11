
using static Synthesizer.GeneratorType;
using DefaultGenInfo = (int Min, int Max, int Default);

namespace Synthesizer;

internal static class Defaults
{
    public static readonly Modulator[] Modulators =
    [
        // All of the default modulators can be found in the Soundfont 2.x specification
        //  at section 8.4.x
        // 8.4.1 MIDI Note-On Velocity to Initial Attenuation
        //  Source Enumeration = 0x0502 (type=1, P=0, D=1, CC=0, index = 2)
        //  Destination Enumeration = Initial Attenuation
        //  Amount = 960
        //  Amount Source Enumeration = 0x0 (No controller)
        //  Transform Enumeration = 0 (Linear)
        new()
        {
            SrcOper = new()
            {
                SourceIndex = ModulatorSourceType.NoteOnVelocity,
                ContinuousController = false,
                Direction = true,
                Polarity = false,
                ContinuityType = ModulatorContinuityType.Concave
            },
            DestOper = GeneratorType.InitialAttenuation,
            Amount = 960,
            AmtSrcOper = new() { Value = 0x0 },
            TransfOper = Transform.Linear
        },

        // 8.4.2 MIDI Note-On Velocity to Filter Cutoff
        //  Source Enumeration = 0x0102 (type=0, P=0, D=1, CC=0, index = 2)
        //  Destination Enumeration = Initial Filter Cutoff
        //  Amount = -2400 Cents
        //  Amount Source Enumeration = 0x0 (No controller)
        //  Transform Enumeration = 0 (Linear)
        new()
        {
            SrcOper = new()
            {
                SourceIndex = ModulatorSourceType.NoteOnVelocity,
                ContinuousController = false,
                Direction = true,
                Polarity = false,
                ContinuityType = ModulatorContinuityType.Linear
            },
            DestOper = GeneratorType.InitialFilterFc,
            Amount = -2400,
            AmtSrcOper = new() { Value = 0x0 },
            TransfOper = Transform.Linear
        },

        // 8.4.3 MIDI Channel Pressure to Vibrato LFO Pitch Depth
        //  Source Enumeration = 0x000D (type=0, P=0, D=0, CC=0, index = 13)
        //  Destination Enumeration = Vibrato LFO to Pitch
        //  Amount = 50 cents/max excursion
        //  Amount Source Enumeration = 0x0 (No controller)
        //  Transform Enumeration = 0 (Linear)
        new()
        {
            SrcOper = new()
            {
                SourceIndex = ModulatorSourceType.ChannelPressure,
                ContinuousController = false,
                Direction = false,
                Polarity = false,
                ContinuityType = ModulatorContinuityType.Linear
            },
            DestOper = GeneratorType.VibLfoToPitch,
            Amount = 50,
            AmtSrcOper = new() { Value = 0x0 },
            TransfOper = Transform.Linear
        },

        // 8.4.4 MIDI Continuous Controller 1 (Modulation Wheel) to Vibrato LFO Pitch Depth
        //  Source Enumeration = 0x0081 (type=0, P=0, D=0, CC=1, index = 1)
        //  Destination Enumeration = Vibrato LFO to Pitch
        //  Amount = 50
        //  Amount Source Enumeration = 0x0 (No controller)
        //  Transform Enumeration = 0 (Linear)
        new()
        {
            SrcOper = new()
            {
                MidiSourceIndex = 1,
                ContinuousController = true,
                Direction = false,
                Polarity = false,
                ContinuityType = ModulatorContinuityType.Linear
            },
            DestOper = GeneratorType.VibLfoToPitch,
            Amount = 50,
            AmtSrcOper = new() { Value = 0x0 },
            TransfOper = Transform.Linear
        }, 

        // 8.4.5 MIDI Continuous Controller 7 (Main Volume) to Initial Attenuation
        //  Source Enumeration = 0x0587 (type=1, P=0, D=1, CC=1, index = 7)
        //      (NOTE: in the specs, it says 0x0582, which is incorrect)
        //  Destination Enumeration = Initial Attenuation
        //  Amount = 960
        //  Amount Source Enumeration = 0x0 (No controller)
        //  Transform Enumeration = 0 (Linear)
        new()
        {
            SrcOper = new()
            {
                MidiSourceIndex = 7,
                ContinuousController = true,
                Direction = true,
                Polarity = false,
                ContinuityType = ModulatorContinuityType.Concave
            },
            DestOper = GeneratorType.InitialAttenuation,
            Amount = 960,
            AmtSrcOper = new() { Value = 0x0 },
            TransfOper = Transform.Linear
        },

        // 8.4.6 MIDI Continuous Controller 10 (Pan) to Pan Position
        //  Source Enumeration = 0x028A (type=0, P=1, D=0, CC=1, index = 10)
        //  Destination Enumeration = Initial Attenuation
        //  Amount = 1000 tenths of a percent
        //  Amount Source Enumeration = 0x0 (No controller)
        //  Transform Enumeration = 0 (Linear)
        new()
        {
            SrcOper = new()
            {
                MidiSourceIndex = 10,
                ContinuousController = true,
                Direction = false,
                Polarity = true,
                ContinuityType = ModulatorContinuityType.Linear
            },
            DestOper = GeneratorType.Pan,
            // Since the absolute 0 of Pan is the center (500 * 0.1%),
            // shift this down by 500 * 0.1% (thanks fluidsynth)
            Amount = 500,
            AmtSrcOper = new() { Value = 0x0 },
            TransfOper = Transform.Linear
        },

        // 8.4.7 MIDI Continuous Controller 11 (Expression Controller) to Initial Attenuation
        //  Source Enumeration = 0x058B (type=1, P=0, D=1, CC=1, index = 11)
        //  Destination Enumeration = Initial Attenuation
        //  Amount = 960
        //  Amount Source Enumeration = 0x0 (No controller)
        //  Transform Enumeration = 0 (Linear)
        new()
        {
            SrcOper = new()
            {
                MidiSourceIndex = 11,
                ContinuousController = true,
                Direction = true,
                Polarity = false,
                ContinuityType = ModulatorContinuityType.Concave
            },
            DestOper = GeneratorType.InitialAttenuation,
            Amount = 960,
            AmtSrcOper = new() { Value = 0x0 },
            TransfOper = Transform.Linear
        },

        // 8.4.8 MIDI Continuous Controller 91 (Reverb) to Reverb Effects Send
        //  Source Enumeration = 0x00DB (type=0, P=0, D=0, CC=1, index = 91)
        //  Destination Enumeration = Reverb Effects Send
        //  Amount = 200 tenths of a percent
        //  Amount Source Enumeration = 0x0 (No controller)
        //  Transform Enumeration = 0 (Linear)
        new()
        {
            SrcOper = new()
            {
                MidiSourceIndex = 91,
                ContinuousController = true,
                Direction = false,
                Polarity = false,
                ContinuityType = ModulatorContinuityType.Linear
            },
            DestOper = GeneratorType.ReverbEffectsSend,
            Amount = 200,
            AmtSrcOper = new() { Value = 0x0 },
            TransfOper = Transform.Linear
        },

        // 8.4.9 MIDI Continuous Controller 93 (Chorus) to Chorus Effects Send
        //  Source Enumeration = 0x00DD (type=0, P=0, D=0, CC=1, index = 93)
        //  Destination Enumeration = Chorus Effects Send (Effects Send 2)
        //  Amount = 200 tenths of a percent
        //  Amount Source Enumeration = 0x0 (No controller)
        //  Transform Enumeration = 0 (Linear)
        new()
        {
            SrcOper = new()
            {
                MidiSourceIndex = 93,
                ContinuousController = true,
                Direction = false,
                Polarity = false,
                ContinuityType = ModulatorContinuityType.Linear
            },
            DestOper = GeneratorType.ChorusEffectsSend,
            Amount = 200,
            AmtSrcOper = new() { Value = 0x0 },
            TransfOper = Transform.Linear
        },

        // 8.4.10 MIDI Pitch Wheel to Initial Pitch Controlled by MIDI Pitch Wheel Sensitivity
        //  Source Enumeration = 0x020E (type=0, P=1, D=0, CC=0, index = 14)
        //  Destination Enumeration = Initial Pitch
        //  Amount = 12700 Cents
        //  Amount Source Enumeration = 0x0010 (type=0, D=0, P=0, C=0, index=16)
        //  Transform Enumeration = 0 (Linear) 
        new()
        {
            SrcOper = new()
            {
                SourceIndex = ModulatorSourceType.PitchWheel,
                ContinuousController = false,
                Direction = false,
                Polarity = true,
                ContinuityType = ModulatorContinuityType.Linear
            },
            DestOper = GeneratorType.FineTune,
            Amount = 12700,
            AmtSrcOper = new()
            {
                SourceIndex = ModulatorSourceType.PitchWheelSensitivity,
                ContinuousController = false,
                Direction = false,
                Polarity = false,
                ContinuityType = ModulatorContinuityType.Linear
            },
            TransfOper = Transform.Linear
        },
    ];

    public static readonly Dictionary<GeneratorType, DefaultGenInfo> Generators = new()
    {
        //                                  MIN    MAX     DEF
        [StartAddrsOffset]             = (     0,     0,      0),  // Ranges for these depend
        [EndAddrsOffset]               = (     0,     0,      0),  // on sample length, handle
        [StartLoopAddrsOffset]         = (     0,     0,      0),  // these separately
        [EndLoopAddrsOffset]           = (     0,     0,      0),
        [StartAddrsOffsetCoarseOffset] = (     0,     0,      0),
        [ModLfoToPitch]                = (-12000, 12000,      0),
        [VibLfoToPitch]                = (-12000, 12000,      0),
        [ModEnvToPitch]                = (-12000, 12000,      0),
        [InitialFilterFc]              = (  1500, 13500,  13500),
        [InitialFilterQ]               = (     0,   960,      0),
        [ModLfoToFilterFc]             = (-12000, 12000,      0),
        [ModEnvToFilterFc]             = (-12000, 12000,      0),
        [EndAddrsCoarseOffset]         = (     0,     0,      0),
        [ModLfoToVolume]               = (  -960,   960,      0),
        [ChorusEffectsSend]            = (     0,  1000,      0),
        [ReverbEffectsSend]            = (     0,  1000,      0),
        [Pan]                          = (  -500,   500,      0),
        [DelayModLFO]                  = (-12000,  5000, -12000),
        [FreqModLFO]                   = (-16000,  4500,      0),
        [DelayVibLFO]                  = (-12000,  5000, -12000),
        [FreqVibLFO]                   = (-16000,  4500,      0),
        [DelayModEnv]                  = (-12000,  5000, -12000),
        [AttackModEnv]                 = (-12000,  8000, -12000),
        [HoldModEnv]                   = (-12000,  5000, -12000),
        [DecayModEnv]                  = (-12000,  8000, -12000),
        [SustainModEnv]                = (     0,  1000,      0),
        [ReleaseModEnv]                = (-12000,  8000, -12000),
        [KeynumToModEnvHold]           = ( -1200,  1200,      0),
        [KeynumToModEnvDecay]          = ( -1200,  1200,      0),
        [DelayVolEnv]                  = (-12000,  5000, -12000),
        [AttackVolEnv]                 = (-12000,  8000, -12000),
        [HoldVolEnv]                   = (-12000,  5000, -12000),
        [DecayVolEnv]                  = (-12000,  8000, -12000),
        [SustainVolEnv]                = (     0,  1440,      0),
        [ReleaseVolEnv]                = (-12000,  8000, -12000),
        [KeynumToVolEnvHold]           = ( -1200,  1200,      0),
        [KeynumToVolEnvDecay]          = ( -1200,  1200,      0),
        [KeyRange]                     = (     0,   127, 0xFF00),
        [VelRange]                     = (     0,   127, 0xFF00),
        [StartLoopAddrsCoarseOffset]   = (     0,     0,      0),
        [Keynum]                       = (     0,   127,     -1),
        [Velocity]                     = (     0,   127,     -1),
        [InitialAttenuation]           = (     0,  1440,      0),
        [EndLoopAddrsCoarseOffset]     = (     0,     0,      0),
        [CoarseTune]                   = (     0,   120,      0),
        [FineTune]                     = (     0,    99,      0),
        [SampleModes]                  = (     0,     3,      0),  // Bit flags
        [ScaleTuning]                  = (     0,  1200,    100),
        [ExclusiveClass]               = (     1,   127,      0),
        [OverridingRootKey]            = (     0,   127,     -1)
    };
}