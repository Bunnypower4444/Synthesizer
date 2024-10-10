
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

    public void NoteOn(int presetIndex, byte key, byte velocity)
    {
        throw new NotImplementedException();        
    }

    public void NoteOff(int presetIndex, byte key, byte velocity)
    {
        throw new NotImplementedException();        
    }

    public byte[] Update(float delta)
    {
        throw new NotImplementedException();
    }
}