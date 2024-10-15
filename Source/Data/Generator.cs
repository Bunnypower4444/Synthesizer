
using System.Runtime.InteropServices;

namespace Synthesizer;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct Generator
{
    public Generator(GeneratorType type, GenAmount amount)
    {
        GenOper = type; GenAmount = amount;
    }

    public Generator(GeneratorType type, ushort unsigned)
    {
        GenOper = type; GenAmount = new() { AsUShort = unsigned };
    }

    public Generator(GeneratorType type, short signed)
    {
        GenOper = type; GenAmount = new() { AsShort = signed };
    }

    public Generator(GeneratorType type, Range range)
    {
        GenOper = type; GenAmount = new() { AsRange = range };
    }

    public GeneratorType GenOper;
    public GenAmount GenAmount;

    public override readonly string ToString()
    {
        return GenOper.ToString();
    }

    public static void ApplyGenerators(
        Dictionary<GeneratorType, SynthParam> synthParams,
        List<Generator> generatorsGlPreset,
        List<Generator> generatorsPreset,
        List<Generator> generatorsGlInst,
        List<Generator> generatorsInst)
    {
        // first do global inst, then local inst, so local can override global
        foreach (var gigen in generatorsGlInst)
        {
            if (gigen.GenOper.IsNotAllowed() || gigen.GenOper.IsNonRealTime())
            {
                Log.Info($"Generator {gigen.GenOper} is not allowed");
                continue;
            }

            synthParams[gigen.GenOper].BaseValue = gigen.GenAmount;
        }

        foreach (var igen in generatorsInst)
        {
            if (igen.GenOper.IsNotAllowed() || igen.GenOper.IsNonRealTime())
            {
                Log.Info($"Generator {igen.GenOper} is not allowed");
                continue;
            }

            synthParams[igen.GenOper].BaseValue = igen.GenAmount;
        }

        // Presets offset the value provided by the default/instrument generators
        // put the offsets from the global pre into a dictionary
        Dictionary<GeneratorType, GenAmount> presetGenOffset = [];
        foreach (var gpgen in generatorsGlPreset)
        {
            if (gpgen.GenOper.IsNotAllowed() || gpgen.GenOper.IsNonRealTime())
            {
                Log.Info($"Generator {gpgen.GenOper} is not allowed");
                continue;
            }

            if (gpgen.GenOper.IsInstrumentOnly())
            {
                Log.Info($"Generator {gpgen.GenOper} is not allowed in a preset");
                continue;
            }

            presetGenOffset[gpgen.GenOper] = gpgen.GenAmount;
        }

        // apply offsets from local preset, remove corresponding global pre (override)
        foreach (var pgen in generatorsPreset)
        {
            if (pgen.GenOper.IsNotAllowed() || pgen.GenOper.IsNonRealTime())
            {
                Log.Info($"Generator {pgen.GenOper} is not allowed");
                continue;
            }

            if (pgen.GenOper.IsInstrumentOnly())
            {
                Log.Info($"Generator {pgen.GenOper} is not allowed in a preset");
                continue;
            }

            synthParams[pgen.GenOper].BaseValue.AsUShort += pgen.GenAmount.AsUShort;
            presetGenOffset.Remove(pgen.GenOper);
        }

        // apply un-overriden global preset
        foreach (var gpgenoffset in presetGenOffset)
        {
            synthParams[gpgenoffset.Key].BaseValue.AsUShort += gpgenoffset.Value.AsUShort;
        }
    }
}

[StructLayout(LayoutKind.Explicit, Pack = 1)]
public struct GenAmount
{
    [FieldOffset(0)]
    public ushort AsUShort;
    [FieldOffset(0)]
    public short AsShort;
    [FieldOffset(0)]
    public Range AsRange;

    public static explicit operator GenAmount(int value)
    {
        return new() { AsUShort = (ushort)value };
    }
}