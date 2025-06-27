// ViewModels/WelcomeViewModel.cs
using System.Windows.Input;
using FireApplications.Helpers;

namespace FireApplications.ViewModels
{
    public class WelcomeViewModel : ViewModelBase
    {
        public string Message => "OSP Kamienica";
        public ICommand ShowRequestsCommand { get; }

        public WelcomeViewModel(ICommand showRequestsCommand)
        {
            ShowRequestsCommand = showRequestsCommand;
        }
    }
}