using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Documents;
using JsonArchiveReplacer;

namespace JsonArchiveReplacerDesktop
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const string OperationalDataFilePath = @".\cache.json";

        private class OperationalData
        {
            public string ZipFolder { get; set; }
            public IEnumerable<Replacement> Replacements { get; set; }
        }

        private class Replacement
        {
            public string ToFind { get; set; }
            public string ToReplace { get; set; }
        }        

        private readonly ObservableCollection<Replacement> _replacements = new ObservableCollection<Replacement>();

        public MainWindow()
        {
            InitializeComponent();

            ReadCacheFile();
            _replacements.Add(new Replacement());
            ReplacementsBox.ItemsSource = _replacements;
        }

        private void SelectFolder(object sender, RoutedEventArgs e)
        {
            using (var dialog = new System.Windows.Forms.FolderBrowserDialog())
            {
                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    FolderNameTB.Text = dialog.SelectedPath;
                }
            }
        }

        private void AddNewLine(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                _replacements.Add(new Replacement());
            }
        }

        private void RemoveItem(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var textBox = sender as System.Windows.Controls.TextBox;
            if (textBox != null)
            {
                var replacement = textBox.DataContext as Replacement;
                if (replacement != null && _replacements.Count > 1)
                {
                    _replacements.Remove(replacement);
                }                
            }
        }

        private void Run(object sender, RoutedEventArgs e)
        {
            RunBtn.IsEnabled = false;
            try
            {                
                var replacer = new Replacer(FolderNameTB.Text, @".\Temp", _replacements.Where(x => !string.IsNullOrWhiteSpace(x.ToFind)).ToDictionary(r => r.ToFind, r => r.ToReplace));
                replacer.Output += HandleOutput;
                replacer.Progress += HandleProgress;

                replacer.Run();
            }
            catch (Exception ex)
            {
                OutputBox.Document.Blocks.Add(new Paragraph(new Run(ex.Message)));
                OutputBox.Document.Blocks.Add(new Paragraph(new Run(ex.StackTrace)));
            }
            RunBtn.IsEnabled = true;
        }

        private void HandleOutput(object sender, string e)
        {
            OutputBox.AppendText($"{e}\r");
        }

        private void HandleProgress(object sender, int e)
        {
            Progress.Value = e;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            var operationalData = new OperationalData()
            {
                ZipFolder = FolderNameTB.Text,
                Replacements = _replacements.Where(x => !string.IsNullOrWhiteSpace(x.ToFind))                
            };
            var tempFile = System.Text.Json.JsonSerializer.Serialize(operationalData);
            File.WriteAllText(OperationalDataFilePath, tempFile);
        }
    
        private void ReadCacheFile()
        {
            if (File.Exists(OperationalDataFilePath))
            {
                var tempFile = File.ReadAllText(OperationalDataFilePath);
                if (!string.IsNullOrWhiteSpace(OperationalDataFilePath))
                {
                    var operationalData = System.Text.Json.JsonSerializer.Deserialize<OperationalData>(tempFile);

                    FolderNameTB.Text = operationalData.ZipFolder;

                    foreach (var replacement in operationalData.Replacements)
                    {
                        _replacements.Add(replacement);
                    }
                }
            }
        }
    }
}
