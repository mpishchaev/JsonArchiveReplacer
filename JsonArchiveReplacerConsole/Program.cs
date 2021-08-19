using System;
using System.Collections.Generic;
using JsonArchiveReplacer;

namespace JsonArchiveReplacerConsole
{
    class Program
    {
        private static string ZipsFolder = $@".\Zips";
        private static string TempFolder = $@".\Temp";

        private static Dictionary<string, string> _replacements = new Dictionary<string, string>()
        {
            { "\"iconURL\":\"\"", "\"iconURL\": null" },
            { "\"iconURL\": \"\"", "\"iconURL\": null" },
            { "\"coverURL\":\"\"", "\"coverURL\": null" },
            { "\"coverURL\": \"\"", "\"coverURL\": null" },
        };

        static void Main(string[] args)
        {
            var replacer = new Replacer(ZipsFolder, TempFolder, _replacements);
            replacer.Output += Output;            

            replacer.Run();
            Console.ReadLine();
        }

        private static void Output(object sender, string e)
        {
            Console.WriteLine(e);
        }

    }
}
