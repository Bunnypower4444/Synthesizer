
namespace Synthesizer;

public class Synthesizer
{
    private static readonly Modulator[] DefaultModulators =
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
            // shift this down by 500 * 0.1% (thanks synthfont)
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

    public Soundfont? Soundfont;
}