using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;

namespace JsonArchiveReplacer
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
            Console.WriteLine("Checking 'Zips' folder");            
            if (Directory.Exists(ZipsFolder))
            {
                string[] fileEntries = Directory.GetFiles(ZipsFolder, "*.zip");                
                foreach (string fileName in fileEntries)
                {
                    ClearTempFolder();
                    HandleZipFile(fileName);                    
                }
                Console.WriteLine($"{fileEntries.Length} zip files handled");
            }
            Directory.Delete(TempFolder, true);
            Console.WriteLine("Finished");
            Console.ReadLine();
        }

        private static void HandleZipFile(string zipFilePath)
        {
            using (FileStream zipToOpen = new FileStream(zipFilePath, FileMode.Open))
            {
                using (ZipArchive archive = new ZipArchive(zipToOpen, ZipArchiveMode.Update))
                {
                    var haveFiles = false;
                    foreach (ZipArchiveEntry entry in archive.Entries)
                    {
                        if (entry.FullName.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
                        {
                            // Gets the full path to ensure that relative segments are removed.
                            string destinationPath = Path.GetFullPath(Path.Combine(TempFolder, entry.FullName));

                            entry.ExtractToFile(destinationPath);
                            haveFiles |= true;
                        }
                    }
                    if (haveFiles)
                    {
                        string[] files = Directory.GetFiles(TempFolder, "*.json");
                        foreach (var file in files)
                        {
                            ReplaceFieldsInFile(file);
                            var fileName = file.Replace($"{TempFolder}\\", "");
                            var entry = archive.GetEntry(fileName);
                            entry.Delete();
                            archive.CreateEntryFromFile(file, fileName);
                        }
                    }
                }
            }
        }

        private static void ReplaceFieldsInFile(string fileName)
        {
            var file = File.ReadAllText(fileName);
            foreach (var repl in _replacements)
            {
                if (file.Contains(repl.Key))
                {
                    file = file.Replace(repl.Key, repl.Value);
                }
            }            
            File.WriteAllText(fileName, file);
        }
    
        private static void ClearTempFolder()
        {
            if (!Directory.Exists(TempFolder))
            {
                Directory.CreateDirectory(TempFolder);
            }
            else
            {
                Directory.Delete(TempFolder, true);
                Directory.CreateDirectory(TempFolder);
            }
        }
    }
}
