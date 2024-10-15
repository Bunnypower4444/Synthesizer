
using System.Runtime.InteropServices;

namespace Synthesizer;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct Modulator
{
    public ModulatorType SrcOper;
    public GeneratorType DestOper;
    public short Amount;
    public ModulatorType AmtSrcOper;
    public Transform TransfOper;

    public override readonly string ToString()
    {
        return $"Source: ({SrcOper}), Amount Source: ({AmtSrcOper}), Dest: {DestOper}, Transf: {TransfOper}, Amt: {Amount}";
    }

    public readonly bool IdenticalTo(Modulator other)
        => SrcOper == other.SrcOper && DestOper == other.DestOper &&
        AmtSrcOper == other.AmtSrcOper && TransfOper == other.TransfOper;

    public static List<Modulator> CleanModulators(
        Modulator[] modulatorsDefault,
        List<Modulator> modulatorsGlPreset,
        List<Modulator> modulatorsPreset,
        List<Modulator> modulatorsGlInst,
        List<Modulator> modulatorsInst)
    {
        List<Modulator> cleanedMods = [];

        // Local replaces global (both inst and preset), then put them at the end of the
        //  local modulators because why not
        int numLocal = modulatorsInst.Count;
        foreach (var glInst in modulatorsGlInst)
        {
            bool keep = true;
            for (int i = 0; i < numLocal; i++)
            {
                if (modulatorsInst[i].IdenticalTo(glInst))
                {
                    keep = false;
                    break;
                }
            }

            if (keep)
                modulatorsInst.Add(glInst);
        }

        numLocal = modulatorsPreset.Count;
        foreach (var glPreset in modulatorsGlPreset)
        {
            bool keep = true;
            for (int i = 0; i < numLocal; i++)
            {
                if (modulatorsPreset[i].IdenticalTo(glPreset))
                {
                    keep = false;
                    break;
                }
            }

            if (keep)
                modulatorsPreset.Add(glPreset);
        }

        // Instrument modulators supersede default modulators
        foreach (var defaultMod in modulatorsDefault)
        {
            bool keep = true;
            foreach (var instMod in modulatorsInst)
            {
                if (instMod.IdenticalTo(defaultMod))
                {
                    keep = false;
                    break;
                }
            }

            // put into the final list
            if (keep)
                cleanedMods.Add(defaultMod);
        }

        // put everything else in the final list
        cleanedMods.AddRange(modulatorsInst);
        cleanedMods.AddRange(modulatorsPreset);

        return cleanedMods;
    }
}