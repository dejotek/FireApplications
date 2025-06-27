// Views/MembersView.axaml.cs
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace FireApplications.Views
{
    public partial class MembersView : UserControl
    {
        public MembersView()
        {
            InitializeComponent();
        }
        private void InitializeComponent() => AvaloniaXamlLoader.Load(this);
    }
}