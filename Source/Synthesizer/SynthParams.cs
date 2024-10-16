
namespace Synthesizer;

public class SynthParams
{
    public class Param
    {
        public Param(GenAmount baseValue)
        {
            BaseValue = baseValue;
        }

        public Param(GenAmount baseValue, float modValue)
        {
            BaseValue = baseValue;
            ModValue = modValue;
        }

        public GenAmount BaseValue;
        public float ModValue;
        public float SignedTotal => BaseValue.AsShort + ModValue;
        public float UnsignedTotal => BaseValue.AsUShort + ModValue;

        public Param Clone()
        {
            return new(BaseValue, ModValue);
        }
    }

    public SynthParams()
    {
        backingParams = new Param[(int)GeneratorType.EndOper];
    }

    public SynthParams(Param[] @params)
    {
        if (@params.Length != (int)GeneratorType.EndOper)
            throw new ArgumentException("Invalid number of synthesis parameters provided");
        
        backingParams = @params;
    }

    private readonly Param[] backingParams;

    public Param this[int index]
    {
        get => backingParams[index];
        set => backingParams[index] = value;
    }

    public Param this[GeneratorType type]
    {
        get => backingParams[(int)type];
        set => backingParams[(int)type] = value;
    }

    public SynthParams Clone()
    {
        return new(backingParams.Select(p => p.Clone()).ToArray());
    }
}