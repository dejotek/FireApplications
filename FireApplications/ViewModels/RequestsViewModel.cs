using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using FireApplications.Helpers;
using FireApplications.Models;
using PdfSharpCore;
using PdfSharpCore.Pdf;
using RazorLight;
using RazorLight.Razor;
using HtmlRendererCore.PdfSharp; 
using AvaloniaUI.PrintToPDF;

namespace FireApplications.ViewModels
{
    public class RequestsViewModel : ViewModelBase
    {
        private const string FileName = "firefighters.json";

        public string Title => "Generowanie wniosków";

        // Formularz
        public string Address { get; set; } = "";
        public string Duration { get; set; } = "";
        
        private DateTimeOffset? _eventDate = DateTimeOffset.Now;
        public DateTimeOffset? EventDate
        {
            get => _eventDate;
            set
            {
                if (_eventDate != value)
                {
                    _eventDate = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _searchText = "";
        public string SearchText
        {
            get => _searchText;
            set { _searchText = value; OnPropertyChanged(); FilterMembers(); }
        }

        // Kolekcje
        public ObservableCollection<Firefighter> AllMembers      { get; } = new();
        public ObservableCollection<Firefighter> FilteredMembers { get; } = new();
        public ObservableCollection<Firefighter> SelectedMembers { get; } = new();

        // Komendy
        public ICommand AddMemberCommand      { get; }
        public ICommand RemoveMemberCommand   { get; }
        public ICommand GenerateReportCommand { get; }

        // nowa wersja
        private bool   _showNotification;
        public bool ShowNotification
        {
            get => _showNotification;
            set
            {
                _showNotification = value;
                OnPropertyChanged(nameof(ShowNotification));
            }
        }

        private string _notificationMessage = "";
        public string NotificationMessage
        {
            get => _notificationMessage;
            set
            {
                _notificationMessage = value;
                OnPropertyChanged(nameof(NotificationMessage));
            }
        }


        public RequestsViewModel()
        {
            AddMemberCommand      = new RelayCommand(p => AddMember(p as Firefighter));
            RemoveMemberCommand   = new RelayCommand(p => RemoveMember(p as Firefighter));
            GenerateReportCommand = new RelayCommand(async _ => await GenerateReportAsync());

            LoadMembers();
            FilterMembers();
        }

        private void LoadMembers()
        {
            if (!File.Exists(FileName)) return;
            var json = File.ReadAllText(FileName, Encoding.UTF8);
            var list = JsonSerializer.Deserialize<ObservableCollection<Firefighter>>(json, new JsonSerializerOptions {
                WriteIndented = true,
                Encoder = JavaScriptEncoder.Create(UnicodeRanges.All)
            }) ?? new ObservableCollection<Firefighter>();

            AllMembers.Clear();
            foreach (var ff in list) AllMembers.Add(ff);
        }

        private void FilterMembers()
        {
            var terms = (SearchText ?? "")
                        .ToLower()
                        .Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var matches = AllMembers.Where(ff =>
            {
                var full = (ff.FirstName + " " + ff.LastName).ToLower();
                return terms.All(t => full.Contains(t));
            });

            FilteredMembers.Clear();
            foreach (var ff in matches) FilteredMembers.Add(ff);
        }

        private void AddMember(Firefighter? ff)
        {
            if (ff == null || SelectedMembers.Contains(ff)) return;
            SelectedMembers.Add(ff);
        }

        private void RemoveMember(Firefighter? ff)
        {
            if (ff == null) return;
            SelectedMembers.Remove(ff);
        }
        
        public async Task GenerateReportAsync()
{
    try
    {
        // 1) Przygotuj model
        var model = new EquivalentApplication
        {
            Address     = Address,
            Duration    = Duration,
            Members     = SelectedMembers
                            .Select(f => $"{f.LastName} {f.FirstName}")
                            .ToList(),
            GeneratedAt = DateTime.Now
        };

        // 2) Wczytaj CSS i inline’uj
        var baseDir = AppContext.BaseDirectory;
        var cssPath = Path.Combine(baseDir, "Templates", "styles.css");
        var css     = File.Exists(cssPath)
                        ? File.ReadAllText(cssPath, Encoding.UTF8)
                        : "";

        // 3) Skonfiguruj RazorLight z folderu Templates
        var templatesDir = Path.Combine(baseDir, "Templates");
        var engine = new RazorLightEngineBuilder()
            .UseFileSystemProject(templatesDir)
            .UseMemoryCachingProvider()
            .Build();

        // 4) Renderuj tylko ciało (<body>…) szablonu
        string bodyHtml = await engine.CompileRenderAsync("doc.cshtml", model);

        // 5) Zbuduj pełny HTML z inline CSS
        string fullHtml =
            "<!DOCTYPE html><html><head><meta charset=\"utf-8\">" +
            "<style>" + css + "</style>" +
            "</head><body>" + bodyHtml + "</body></html>";

        // 6) Dump debug.html
        File.WriteAllText(Path.Combine(baseDir, "debug.html"), fullHtml, Encoding.UTF8);

        // 7) Generuj PDF
        var config = new PdfGenerateConfig
        {
            PageSize     = PageSize.A4,
            MarginTop    = 40,
            MarginBottom = 40,
            MarginLeft   = 40,
            MarginRight  = 40
        };
        PdfDocument pdf = PdfGenerator.GeneratePdf(fullHtml, config);

        // 8) Zapisz na pulpicie
        var desktop  = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
        var fileName = Path.Combine(desktop, $"wniosek_{DateTime.Now:yyyyMMdd_HHmmss}.pdf");
        pdf.Save(fileName);

        // 9) Toast sukcesu
        NotificationMessage = "Wygenerowano wniosek";
        ShowNotification     = true;
        await Task.Delay(3000);
        ShowNotification     = false;
    }
    catch (Exception ex)
    {
        // W razie błędu pokaż dokładny komunikat
        NotificationMessage = "Błąd: " + ex.Message;
        ShowNotification     = true;
        await Task.Delay(3000);
        ShowNotification     = false;
    }
}


        


    }
}
