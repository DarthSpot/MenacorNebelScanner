using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
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

        private void ClickCloseApp(object? sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void ClickOpenFile(object? sender, RoutedEventArgs e)
        {
            var ofd = new OpenFileDialog();
            ofd.Title = "Öffne Menacor Datei";
            ofd.Filters.Add(new FileDialogFilter()
                {Extensions = new List<string>() {"*.menacor"}, Name = "Menacor Datei"});

            var result = ofd.ShowAsync(this);
            var path = result.Result.SingleOrDefault();
            
        }
    }
}