
namespace Synthesizer;

public class Program
{
    public static void Main(string[] args)
    {
        Assets.Load(Run);
    }

    public static void Run()
    {
        var soundfont = Assets.Soundfonts["MuseScore_General"];
        Console.WriteLine(soundfont);
    }
}