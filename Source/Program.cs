
namespace Synthesizer;

public class Program
{
    // If we are using a different Main, disable this one
    #if !(StreamSampleLoaderTests || GenModOverrideTests)

    public static void Main(string[] args)
    {
        Assets.Load(Run);
    }

    #endif

    public static void Run()
    {
        var soundfont = Assets.Soundfonts["MuseScore_General"];
        Console.WriteLine(soundfont);

        Assets.Dispose();
    }
}