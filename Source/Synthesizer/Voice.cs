
namespace Synthesizer;

public class Voice
{
    public Voice(Sample sample, byte key, byte velocity,
        List<Generator> generators, List<Modulator> modulators)
    {
        Sample = sample;
        Key = key;
        Velocity = velocity;
        
        // TODO: filter overridden modulators
        Modulators = modulators;

        // basically copy over just the default value from the generator info
        SynthParams = new
        (
            Defaults.Generators.Select
            (
                pair => KeyValuePair.Create(pair.Key, (GenAmount)pair.Value.Default)
            )
        );
    }

    public readonly Sample Sample;
    public readonly byte Key, Velocity;

    private readonly List<Modulator> Modulators;
    private readonly Dictionary<GeneratorType, GenAmount> SynthParams;

    public byte[] Update(float delta)
    {
        throw new NotImplementedException();
    }
}