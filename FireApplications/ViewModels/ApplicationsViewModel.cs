// ViewModels/ApplicationsViewModel.cs
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using FireApplications.Models;

namespace FireApplications.ViewModels
{
    public class ApplicationsViewModel : ViewModelBase
    {
        private const string FileName = "applications.json";

        public string Title => "Wygenerowane wnioski";

        public ObservableCollection<EquivalentApplication> Applications { get; } 
            = new ObservableCollection<EquivalentApplication>();

        public ApplicationsViewModel()
        {
            Load();
        }

        private void Load()
        {
            if (!File.Exists(FileName))
                File.WriteAllText(FileName, "[]", Encoding.UTF8);

            var json = File.ReadAllText(FileName, Encoding.UTF8);
            var list = JsonSerializer.Deserialize<ObservableCollection<EquivalentApplication>>(json, new JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder       = JavaScriptEncoder.Create(UnicodeRanges.All)
            }) ?? new ObservableCollection<EquivalentApplication>();

            Applications.Clear();
            foreach (var a in list.OrderByDescending(x => x.GeneratedAt))
                Applications.Add(a);
        }

        public void Add(EquivalentApplication app)
        {
            Applications.Insert(0, app);
            Save();
        }

        private void Save()
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder       = JavaScriptEncoder.Create(UnicodeRanges.All)
            };
            File.WriteAllText(FileName,
                JsonSerializer.Serialize(Applications, options),
                Encoding.UTF8);
        }
    }
}