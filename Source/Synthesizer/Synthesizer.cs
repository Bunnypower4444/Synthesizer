
namespace Synthesizer;

public class Synthesizer
{
    private static readonly Modulator[] DefaultModulators =
    [
        // All of the default modulators can be found in the Soundfont 2.x specification
        //  at section 8.4.x
        // MIDI Note-On Velocity to Initial Attenuation
        //  Source Enumeration = 0x0502 (type=1, P=0, D=1, CC=0, index = 2)
        //  Destination Enumeration = Initial Attenuation
        //  Amount = 960
        //  Amount Source Enumeration = 0x0 (No controller)
        //  Transform Enumeration = 0 (Linear)
        new()
        {
            SrcOper = new() { Value = 0x0502 },
            DestOper = GeneratorType.InitialAttenuation,
            Amount = 960,
            AmtSrcOper = new() { Value = 0x0 },
            TransfOper = Transform.Linear
        },

        // MIDI Note-On Velocity to Filter Cutoff
        //  Source Enumeration = 0x0102 (type=0, P=0, D=1, CC=0, index = 2)
        //  Destination Enumeration = Initial Filter Cutoff
        //  Amount = -2400 Cents
        //  Amount Source Enumeration = 0x0 (No controller)
        //  Transform Enumeration = 0 (Linear)
        new()
        {
            SrcOper = new() { Value = 0x0102 },
            DestOper = GeneratorType.InitialFilterFc,
            Amount = -2400,
            AmtSrcOper = new() { Value = 0x0 },
            TransfOper = Transform.Linear
        },

        // MIDI Channel Pressure to Vibrato LFO Pitch Depth
        //  Source Enumeration = 0x000D (type=0, P=0, D=0, CC=0, index = 13)
        //  Destination Enumeration = Vibrato LFO to Pitch
        //  Amount = 50 cents/max excursion
        //  Amount Source Enumeration = 0x0 (No controller)
        //  Transform Enumeration = 0 (Linear)
        new()
        {
            SrcOper = new() { Value = 0x000D },
            DestOper = GeneratorType.VibLfoToPitch,
            Amount = 50,
            AmtSrcOper = new() { Value = 0x0 },
            TransfOper = Transform.Linear
        },

        // MIDI Continuous Controller 1 (Modulation Wheel) to Vibrato LFO Pitch Depth
        //  Source Enumeration = 0x0081 (type=0, P=0, D=0, CC=1, index = 1)
        //  Destination Enumeration = Vibrato LFO to Pitch
        //  Amount = 50
        //  Amount Source Enumeration = 0x0 (No controller)
        //  Transform Enumeration = 0 (Linear)
        new()
        {
            SrcOper = new() { Value = 0x0081 },
            DestOper = GeneratorType.VibLfoToPitch,
            Amount = 50,
            AmtSrcOper = new() { Value = 0x0 },
            TransfOper = Transform.Linear
        }, 

        // MIDI Continuous Controller 7 (Main Volume) to Initial Attenuation
        //  Source Enumeration = 0x0582 (type=1, P=0, D=1, CC=1, index = 7)
        //  Destination Enumeration = Initial Attenuation
        //  Amount = 960
        //  Amount Source Enumeration = 0x0 (No controller)
        //  Transform Enumeration = 0 (Linear)
        new()
        {
            // fix the hex, or use the properties to set: 0x0582 is wrong
            SrcOper = new() { Value = 0x0582 },
            DestOper = GeneratorType.InitialAttenuation,
            Amount = 960,
            AmtSrcOper = new() { Value = 0x0 },
            TransfOper = Transform.Linear
        },

        // MIDI Continuous Controller 10 (Pan) to Pan Position
        //  Source Enumeration = 0x028A (type=0, P=1, D=0, CC=1, index = 10)
        //  Destination Enumeration = Initial Attenuation
        //  Amount = 1000 tenths of a percent
        //  Amount Source Enumeration = 0x0 (No controller)
        //  Transform Enumeration = 0 (Linear)
        new()
        {
            SrcOper = new() { Value = 0x28A },
            DestOper = GeneratorType.Pan,
            Amount = -2400,
            AmtSrcOper = new() { Value = 0x0 },
            TransfOper = Transform.Linear
        },

        // 8.4.7 MIDI Continuous Controller 11 to Initial Attenuation
        //  Source Enumeration = 0x058B (type=1, P=0, D=1, CC=1, index = 11)
        //  Destination Enumeration = Initial Attenuation
        //  Amount = 960
        //  Amount Source Enumeration = 0x0 (No controller)
        //  Transform Enumeration = 0 (Linear) 

        // 8.4.8 MIDI Continuous Controller 91 to Reverb Effects Send
        //  Source Enumeration = 0x00DB (type=0, P=0, D=0, CC=1, index = 91)
        //  Destination Enumeration = Reverb Effects Send
        //  Amount = 200 tenths of a percent
        //  Amount Source Enumeration = 0x0 (No controller)
        //  Transform Enumeration = 0 (Linear)

        // 8.4.9 MIDI Continuous Controller 93 to Chorus Effects Send
        //  Source Enumeration = 0x00DD (type=0, P=0, D=0, CC=1, index = 93)
        //  Destination Enumeration = Chorus Effects Send (Effects Send 2)
        //  Amount = 200 tenths of a percent
        //  Amount Source Enumeration = 0x0 (No controller)
        //  Transform Enumeration = 0 (Linear)

        // 8.4.10 MIDI Pitch Wheel to Initial Pitch Controlled by MIDI Pitch Wheel Sensitivity
        //  Source Enumeration = 0x020E (type=0, P=1, D=0, CC=0, index = 14)
        //  Destination Enumeration = Initial Pitch
        //  Amount = 12700 Cents
        //  Amount Source Enumeration = 0x0010 (type=0, D=0, P=0, C=0, index=16)
        //  Transform Enumeration = 0 (Linear) 
    ];

    public Soundfont? Soundfont;
}