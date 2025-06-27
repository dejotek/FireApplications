// Views/ApplicationsView.axaml.cs
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace FireApplications.Views
{
    public partial class ApplicationsView : UserControl
    {
        public ApplicationsView()
        {
            InitializeComponent();
        }

        private void InitializeComponent() => AvaloniaXamlLoader.Load(this);
    }
}