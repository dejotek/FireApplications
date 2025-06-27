using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace FireApplications.Views
{
    public partial class RequestsView : UserControl
    {
        public RequestsView()
        {
            InitializeComponent();
        }

        private void InitializeComponent() =>
            AvaloniaXamlLoader.Load(this);
    }
}