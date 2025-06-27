using System.Windows.Input;
using FireApplications.Helpers;
using FireApplications.ViewModels;

namespace FireApplications.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private ViewModelBase? _currentView;
        public ViewModelBase? CurrentView
        {
            get => _currentView;
            set { _currentView = value; OnPropertyChanged(); }
        }

        public ICommand ShowApplicationsCommand { get; }
        public ICommand ShowRequestsCommand  { get; }
        public ICommand ShowMembersCommand   { get; }
        public ICommand ShowSettingsCommand  { get; }
        public ICommand ShowWelcomeCommand  { get; }

        public MainWindowViewModel()
        {
            ShowApplicationsCommand = new RelayCommand(_ => CurrentView = new ApplicationsViewModel());
            ShowRequestsCommand = new RelayCommand(_ => CurrentView = new RequestsViewModel());
            ShowMembersCommand  = new RelayCommand(_ => CurrentView = new MembersViewModel());
            ShowSettingsCommand = new RelayCommand(_ => CurrentView = new SettingsViewModel());
            ShowWelcomeCommand  = new RelayCommand(_ => CurrentView = CreateWelcomeVm());

            // start with welcome / empty view
            CurrentView = CreateWelcomeVm();
        }
        
        private WelcomeViewModel CreateWelcomeVm()
            => new WelcomeViewModel(ShowRequestsCommand);
    }
}