using NjmgLib;
namespace NjmgTool;

internal static class Program
{
    private static int Main(string[] args)
    {
        try
        {
            var debugEnabled = false;
            var placeholderTextEnabled = false;
            var loadedScript = new Script();

            if (args.Length == 0) PrintHelpText();

            for (var argi = 0; argi < args.Length; argi++)
            {
                var command = args[argi];
                if (command == "--extractScript")
                {
                    var romPath = args[++argi];
                    var dst = args[++argi];
                    NjmgUtility.ExtractScriptFromRom(
                        romPath, dst, debugEnabled: debugEnabled, placeholderTextEnabled: placeholderTextEnabled);
                }
                else if (command == "--loadScript")
                {
                    var src = args[++argi];
                    loadedScript = NjmgUtility.LoadScript(src);
                }
                else if (command == "--injectScript")
                {
                    var src = args[++argi];
                    var dst = args[++argi];
                    NjmgUtility.InjectScript(loadedScript, src, dst);
                }
                else if (command == "--logRomInfo")
                {
                    var romPath = args[++argi];
                    NjmgUtility.LogRomInfo(romPath);
                }
                else if (command == "--set")
                {
                    var name = args[++argi];
                    if (name == "debugEnabled")
                        debugEnabled = int.Parse(args[++argi]) != 0;
                    else if (name == "placeholderTextEnabled")
                        placeholderTextEnabled = int.Parse(args[++argi]) != 0;
                    else
                        throw new ArgumentException($"Unrecognized variable '{name}'.");
                }
                else if (command == "--help")
                {
                    PrintHelpText();
                }
                else
                {
                    throw new ArgumentException($"Unrecognized command '{command}'.");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            return 1;
        }
        return 0;
    }

    private static void PrintHelpText()
    {
        Console.WriteLine(
            "Nekojara Monogatari Tool\n" +
            "Usage:\n" +
            "    NjmgTool --command1 arg1 arg2 ... --command2 arg1 arg2 ...\n" +
            "Commands:\n" +
            "    --extractScript inputRomPath outputScriptPath\n" +
            "        Creates a new script file from a ROM file\n" +
            "    --loadScript path\n" +
            "        Loads an existing script file\n" +
            "    --injectScript inputRomPath outputRomPath\n" +
            "        Injects the currently loaded script file into a ROM file\n" +
            "        'inputRomPath' is the original unmodified ROM\n" +
            "        'outputRomPath' is where to save the resulting modified ROM\n" +
            "    --logRomInfo inputRomPath\n" +
            "        Displays information about a ROM\n" +
            "    --set option value\n" +
            "        Sets an option to the given value\n" +
            "        Available options:\n" +
            "            debugEnabled (boolean)\n" +
            "                Whether to include extra debug information when extracting a script\n" +
            "            placeholderTextEnabled (boolean)\n" +
            "                When extracting a script, whether to replace most text with placeholders (ex. M102)\n" +
            "                The original text will be still be included, but commented out\n" +
            "    --help\n" +
            "        Display this message\n"
        );
    }
}