using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using System.Windows.Input;
using FireApplications.Helpers;
using FireApplications.Models;

namespace FireApplications.ViewModels
{
    public class MembersViewModel : ViewModelBase
    {
        private const string FileName = "firefighters.json";

        public string Title => "Zarządzanie ratownikami";

        // pełna lista
        public ObservableCollection<Firefighter> Firefighters { get; } = new();

        // filtrowana lista do UI
        public ObservableCollection<Firefighter> FilteredFirefighters { get; } = new();

        // wyszukiwanie
        private string _searchText = "";
        public string SearchText
        {
            get => _searchText;
            set
            {
                if (_searchText == value) return;
                _searchText = value;
                OnPropertyChanged(nameof(SearchText));
                FilterFirefighters();
            }
        }

        // formularz
        public string FirstName { get; set; } = "";
        public string LastName  { get; set; } = "";

        public ObservableCollection<RoleOption> AvailableRoles { get; } =
            new ObservableCollection<RoleOption>
            {
                new("KPP"), new("Kierowca"), new("Dowódca")
            };

        // edycja
        private Firefighter? _selectedForEdit;
        public Firefighter? SelectedForEdit
        {
            get => _selectedForEdit;
            set
            {
                _selectedForEdit = value;
                OnPropertyChanged(nameof(SelectedForEdit));
                OnPropertyChanged(nameof(IsEditing));
                OnPropertyChanged(nameof(FormHeader));
                
                (SaveEditCommand as RelayCommand)?.RaiseCanExecuteChanged();
                (CancelCommand   as RelayCommand)?.RaiseCanExecuteChanged();
            }
        }

        public bool IsEditing => SelectedForEdit != null;
        public string FormHeader => IsEditing ? "Edytuj ratownika" : "Dodaj ratownika";

        // komendy
        public ICommand AddCommand      { get; }
        public ICommand EditCommand     { get; }
        public ICommand SaveEditCommand { get; }
        public ICommand CancelCommand   { get; }
        public ICommand DeleteCommand   { get; }

        public MembersViewModel()
        {
            AddCommand      = new RelayCommand(_ => Add());
            EditCommand     = new RelayCommand(p => BeginEdit(p as Firefighter));
            SaveEditCommand = new RelayCommand(_ => SaveEdit(), _ => IsEditing);
            CancelCommand   = new RelayCommand(_ => CancelEdit(), _ => IsEditing);
            DeleteCommand   = new RelayCommand(p =>
            {
                if (p is Firefighter ff)
                {
                    Firefighters.Remove(ff);
                    Save();
                    FilterFirefighters();
                }
            });

            Load();
        }

        private void Load()
        {
            if (!File.Exists(FileName))
                File.WriteAllText(FileName, "[]", Encoding.UTF8);

            var json = File.ReadAllText(FileName, Encoding.UTF8);
            var list = JsonSerializer.Deserialize<ObservableCollection<Firefighter>>(json, new JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder       = JavaScriptEncoder.Create(UnicodeRanges.All)
            }) ?? new ObservableCollection<Firefighter>();

            Firefighters.Clear();
            foreach (var ff in list)
                Firefighters.Add(ff);

            FilterFirefighters();
        }

        private void FilterFirefighters()
        {
            var terms = SearchText
                .ToLower()
                .Split(' ', StringSplitOptions.RemoveEmptyEntries);

            FilteredFirefighters.Clear();
            foreach (var ff in Firefighters.Where(ff =>
            {
                var full = (ff.FirstName + " " + ff.LastName).ToLower();
                return terms.All(t => full.Contains(t));
            }))
            {
                FilteredFirefighters.Add(ff);
            }
        }

        private void Add()
        {
            if (IsEditing) return;

            var roles = new ObservableCollection<string>(
                AvailableRoles.Where(r => r.IsSelected).Select(r => r.Name)
            );

            Firefighters.Add(new Firefighter
            {
                FirstName = FirstName,
                LastName  = LastName,
                Roles      = roles
            });

            Save();
            ClearForm();
            FilterFirefighters();
        }

        private void BeginEdit(Firefighter? ff)
        {
            if (ff == null) return;

            SelectedForEdit = ff;
            FirstName       = ff.FirstName;
            LastName        = ff.LastName;

            foreach (var role in AvailableRoles)
                role.IsSelected = ff.Roles.Contains(role.Name);

            OnPropertyChanged(nameof(FirstName));
            OnPropertyChanged(nameof(LastName));
        }

        private void SaveEdit()
        {
            if (SelectedForEdit == null) return;

            SelectedForEdit.FirstName = FirstName;
            SelectedForEdit.LastName  = LastName;
            SelectedForEdit.Roles.Clear();
            foreach (var role in AvailableRoles.Where(r => r.IsSelected))
                SelectedForEdit.Roles.Add(role.Name);

            Save();
            SelectedForEdit = null;
            ClearForm();
            FilterFirefighters();
        }

        private void CancelEdit()
        {
            SelectedForEdit = null;
            ClearForm();
        }

        private void ClearForm()
        {
            FirstName = LastName = string.Empty;
            foreach (var role in AvailableRoles)
                role.IsSelected = false;
            OnPropertyChanged(nameof(FirstName));
            OnPropertyChanged(nameof(LastName));
        }

        private void Save()
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder       = JavaScriptEncoder.Create(UnicodeRanges.All)
            };
            File.WriteAllText(FileName,
                              JsonSerializer.Serialize(Firefighters, options),
                              Encoding.UTF8);
        }
    }
}
