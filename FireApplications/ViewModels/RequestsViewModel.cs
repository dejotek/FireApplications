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
using FireApplications.Helpers;
using FireApplications.Models;
using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf;

namespace FireApplications.ViewModels
{
    public class RequestsViewModel : ViewModelBase
    {
        private const string FileName = "firefighters.json";

        public string Title => "Generowanie wniosków";

        // Formularz
        public string Address     { get; set; } = "";
        public string Duration    { get; set; } = "";

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

        private async Task GenerateReportAsync()
        {
            try
            {
                // 1) Tworzenie PDF
                var desktop  = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
                var fileName = Path.Combine(desktop, $"wniosek_{DateTime.Now:yyyyMMdd_HHmmss}.pdf");

                var doc = new PdfDocument();
                var page = doc.AddPage();
                var gfx = XGraphics.FromPdfPage(page);
                var font = new XFont("Verdana", 12, XFontStyle.Regular);

                double y = 40;
                void Draw(string s)
                {
                    gfx.DrawString(s, font, XBrushes.Black, new XPoint(40, y));
                    y += 25;
                    if (y > page.Height - 40)
                    {
                        page = doc.AddPage();
                        gfx  = XGraphics.FromPdfPage(page);
                        y    = 40;
                    }
                }

                Draw($"Adres zdarzenia: {Address}");
                Draw($"Czas trwania: {Duration}");
                Draw("Wybrani członkowie:");
                foreach (var ff in SelectedMembers)
                    Draw($"• {ff.FirstName} {ff.LastName}");

                doc.Save(fileName);

                // 2) Toast sukcesu
                NotificationMessage = "Wygenerowano wniosek";
                ShowNotification     = true;
                
                var app = new EquivalentApplication
                {
                    Address       = Address,
                    Duration      = Duration,
                    Members       = SelectedMembers
                        .Select(f => $"{f.FirstName} {f.LastName}")
                        .ToList(),
                    GeneratedAt   = DateTime.Now
                };

                // zakładamy, że masz singleton lub możesz utworzyć VM:
                var appsVm = new ApplicationsViewModel();
                appsVm.Add(app);

                // 3) Czekaj 3 sekundy
                await Task.Delay(3000);
            }
            catch
            {
                NotificationMessage = "Błąd przy generowaniu";
                ShowNotification     = true;
                await Task.Delay(3000);
            }
            finally
            {
                ShowNotification = false;
            }
        }
    }
}
