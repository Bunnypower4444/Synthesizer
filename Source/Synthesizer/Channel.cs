
namespace Synthesizer;

public class Channel
{
    public Channel(Soundfont soundfont, string name)
    {
        Soundfont = soundfont;
        Name = name;
    }

    public Channel(Soundfont soundfont, int channelNum)
    {
        Soundfont = soundfont;
        Name = channelNum.ToString();
    }

    public readonly Soundfont Soundfont;
    public string Name;
    public float ChannelPressure = 1;
    /// <summary>
    /// The amount the Pitch Wheel is offset by, from -1 to 1
    /// </summary>
    public float PitchWheel = 0;
    /// <summary>
    /// How sensitive the Pitch Wheel is, in cents
    /// </summary>
    public float PitchWheelSensitivity = 400;
    public ushort Bank = 0;

    private readonly List<Voice> voices = [];

    /// <summary>
    /// For a given preset, note, and velocity, plays, in
    /// each instrument that plays in the respective ranges,
    /// the samples for the respective ranges.
    /// </summary>
    /// <param name="presetNumber"></param>
    /// <param name="key"></param>
    /// <param name="velocity"></param>
    public void NoteOn(int presetNumber, byte key, byte velocity)
    {
        static bool CheckRanges(byte value,
            Range? local, Range? global)
        {
            return local != null ?
                local.Value.ValueInRange(value)
                : (global?.ValueInRange(value) ?? true);
        }

        var preset = Soundfont.Banks[Bank].GetPreset(presetNumber);
        var glPresetZone = preset.GlobalZone;

        foreach (var pzone in preset.PresetZones)
        {
            // Check against key and vel ranges
            if (CheckRanges(key, pzone.KeyRange, glPresetZone?.KeyRange) &&
                CheckRanges(velocity, pzone.VelRange, glPresetZone?.VelRange))
            {
                SearchSamples(pzone);
            }
        }

        void SearchSamples(PresetZone pzone)
        {
            var instrument = pzone.Instrument;
            var glInstZone = instrument.GlobalZone;

            foreach (var izone in instrument.InstrumentZones)
            {
                // Check ranges
                if (CheckRanges(key, izone.KeyRange, glInstZone?.KeyRange) &&
                    CheckRanges(velocity, izone.VelRange, glInstZone?.VelRange))
                {
                    voices.Add(new Voice(
                        Soundfont,
                        presetNumber,
                        izone.Sample,
                        key, velocity,
                        glPresetZone?.Generators ?? [],
                        pzone.Generators,
                        glInstZone?.Generators ?? [],
                        izone.Generators,
                        glPresetZone?.Modulators ?? [],
                        pzone.Modulators ?? [],
                        glInstZone?.Modulators ?? [],
                        izone.Modulators ?? []
                    ));
                }
            }
        }
    }

    public void NoteOff(int presetNumber, byte key, byte velocity)
    {
        foreach (var voice in voices)
        {
            if (voice.PresetNum == presetNumber && voice.Key == key)
            {
                
            }
        }
    }

    public byte[] Update(float delta)
    {
        throw new NotImplementedException();
    }
}