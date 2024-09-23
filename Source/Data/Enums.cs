
namespace Synthesizer;

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

public enum ModulatorSourceType : byte
{
    None = 0,
    NoteOnVelocity = 2,
    NoteOnKeyNumber = 3,
    PolyPressure = 10,
    ChannelPressure = 13,
    PitchWheel = 14,
    PitchWheelSensitivity = 16,
    Link = 127
}

public enum ModulatorContinuityType : byte
{
    Linear, Concave, Convex, Switch
}

public record struct ModulatorType
{
    /// <summary>
    /// The raw binary value of the modulator type.
    /// <list type="bullet">
    ///     <item>
    ///         <term>Bits 0-6</term>
    ///         <description>Source Index</description>
    ///     </item>
    ///     <item>
    ///         <term>Bits 7</term>
    ///         <description>MIDI Continuous Controller (CC) Flag</description>
    ///     </item>
    ///     <item>
    ///         <term>Bits 8</term>
    ///         <description>Direction</description>
    ///     </item>
    ///     <item>
    ///         <term>Bits 9</term>
    ///         <description>Polarity</description>
    ///     </item>
    ///     <item>
    ///         <term>Bits 10-15</term>
    ///         <description>Type - Continuity of the the controller</description>
    ///     </item>
    /// </list>
    /// </summary>
    public ushort Value;

    public ModulatorSourceType SourceIndex
    {
        readonly get => (ModulatorSourceType)(Value & 0b01111111);
        set => Value = (Value & ~0b01111111) | (ushort)value
    }
    public bool ContinuousController
    {
        readonly get => (Value & 0b10000000) > 0;
        set => Value = (Value & ~0b10000000) | ((ushort)value << 7)
    }
    public bool Direction
    {
        readonly get => (Value & 0b1_0000000) > 0;
        set => Value = (Value & ~0b1_0000000) | ((ushort)value << 8)
    }
    public bool Polarity
    {
        readonly get => (Value & 0b10_0000000) > 0;
        set => Value = (Value & ~0b10_0000000) | ((ushort)value << 9)
    }
    public ModulatorSourceType SourceIndex
    {
        readonly get => (ModulatorContinuityType)(Value >> 10);
        set => Value = (Value & ~0b11111100_00000000) | ((ushort)value << 10)
    }

    public override readonly string ToString()
    {
        return $"Index: {SourceIndex}, CC: {ContinuousController}, Dir: {Direction}, Pol: {Polarity}, Type: {Type}";
    }
}

public enum Transform : ushort
{
    Linear = 0, Absolute = 2
}

public enum SampleLink : ushort
{
    MonoSample = 1, RightSample = 2, LeftSample = 4, LinkedSample = 8,
    RomMonoSample = 32769, RomRightSample = 32770, RomLeftSample = 32772, RomLinkedSample = 32776
}