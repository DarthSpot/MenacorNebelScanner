using Avalonia;
using Avalonia.Markup.Xaml;

namespace MenacorNebel.App.Views
{
    public class MainWindow : FluentWindow
    {
        public MainWindow()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}