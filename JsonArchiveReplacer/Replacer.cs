using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;

namespace JsonArchiveReplacer
{
    public class Replacer
    {
        private readonly string _zipsFolder;
        private readonly string _tempFolder;
        private readonly Dictionary<string, string> _replacements;

        public event EventHandler<string> Output;
        public event EventHandler<int> Progress;

        public Replacer(string zipsFolder, string tempFolder, Dictionary<string, string> replacements)
        {
            _zipsFolder = zipsFolder;
            _tempFolder = tempFolder;
            _replacements = replacements;
        }

        public void Run()
        {
            Output?.Invoke(this, "Checking 'Zips' folder");

            if (Directory.Exists(_zipsFolder))
            {
                string[] fileEntries = Directory.GetFiles(_zipsFolder, "*.zip");

                Progress?.Invoke(this, 0);
                for (int i = 0; i < fileEntries.Length; i++)
                {
                    ClearTempFolder();
                    HandleZipFile(fileEntries[i]);
                    Progress?.Invoke(this, (int)((float)i / fileEntries.Length * 100));
                }
                Progress?.Invoke(this, 100);
                Output?.Invoke(this, $"{fileEntries.Length} zip files handled");                
            }
            Directory.Delete(_tempFolder, true);

            Output?.Invoke(this, "Finished");
        }

        private void HandleZipFile(string zipFilePath)
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
                            string destinationPath = Path.GetFullPath(Path.Combine(_tempFolder, entry.FullName));

                            entry.ExtractToFile(destinationPath);
                            
                            haveFiles |= true;
                        }
                    }
                    if (haveFiles)
                    {
                        string[] files = Directory.GetFiles(_tempFolder, "*.json");
                        foreach (var file in files)
                        {
                            ReplaceFieldsInFile(file);
                            var fileName = file.Replace($"{_tempFolder}\\", "");
                            var entry = archive.GetEntry(fileName);
                            entry.Delete();
                            archive.CreateEntryFromFile(file, fileName);
                        }
                    }
                }
            }
        }

        private void ReplaceFieldsInFile(string fileName)
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

        private void ClearTempFolder()
        {
            if (!Directory.Exists(_tempFolder))
            {
                Directory.CreateDirectory(_tempFolder);
            }
            else
            {
                Directory.Delete(_tempFolder, true);
                Directory.CreateDirectory(_tempFolder);
            }
        }
    }
}
