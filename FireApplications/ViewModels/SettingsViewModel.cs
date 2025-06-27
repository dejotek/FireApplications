// ViewModels/SettingsViewModel.cs
using System;
using System.IO;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using System.Windows.Input;
using FireApplications.Helpers;

namespace FireApplications.ViewModels
{
    public class SettingsViewModel : ViewModelBase
    {
        private const string FileName = "settings.json";

        // ← tutaj dodajemy Title
        public string Title => "Ustawienia";

        private string _unitName = "";
        public string UnitName
        {
            get => _unitName;
            set { _unitName = value; OnPropertyChanged(); }
        }

        public ICommand SaveSettingsCommand { get; }

        public SettingsViewModel()
        {
            LoadSettings();
            SaveSettingsCommand = new RelayCommand(_ => SaveSettings());
        }

        private void LoadSettings()
        {
            if (!File.Exists(FileName))
            {
                UnitName = "Moja Jednostka";
                SaveSettings();
                return;
            }

            var json = File.ReadAllText(FileName, Encoding.UTF8);
            try
            {
                var doc = JsonDocument.Parse(json);
                if (doc.RootElement.TryGetProperty("UnitName", out var prop))
                    UnitName = prop.GetString() ?? UnitName;
            }
            catch { }
        }

        private void SaveSettings()
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder       = JavaScriptEncoder.Create(UnicodeRanges.All)
            };
            var obj = new { UnitName = this.UnitName };
            var json = JsonSerializer.Serialize(obj, options);
            File.WriteAllText(FileName, json, Encoding.UTF8);
        }
    }
}