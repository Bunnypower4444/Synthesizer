
namespace Synthesizer;

public class Voice
{
    public Voice(Sample sample, byte key, byte velocity,
        List<Generator> generators, List<Modulator> modulators)
    {
        Sample = sample;
        Key = key;
        Velocity = velocity;
        
        // TODO: synth params and filter overridden modulators
        Modulators = modulators;
    }

    public Sample Sample;
    public byte Key, Velocity;
    private List<Modulator> Modulators;

    public byte[] Update(float delta)
    {
        throw new NotImplementedException();
    }
}