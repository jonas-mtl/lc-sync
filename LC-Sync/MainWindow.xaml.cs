using LC_Sync.Core;
using LC_Sync.Core.Util;
using LC_Sync.MVVM.View;
using System;
using System.Diagnostics;
using System.Windows;

namespace LC_Sync
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            
            VersionTextBlock.Text = $"(Version {InitCore.currentlyLoadedVersion.ToString().Remove(InitCore.currentlyLoadedVersion.ToString().Length - 2)})";
        }

        private void Window_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.LeftButton == System.Windows.Input.MouseButtonState.Pressed)
            {
                DragMove();
            }
        }

        private void Unmod_Click(object sender, RoutedEventArgs e)
        {

            CustomSelectBox prompt = new CustomSelectBox();
            prompt.Title = string.Empty;
            prompt.Text = "Are you sure?";
            prompt.Width = 300;
            prompt.ShowDialog();

            while(!prompt.closed)
            {
                continue;
            }

            if (prompt.returnValue)
            {
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();

                Log.Info("Unmodding...");
                FileHandler.removeMods();

                TimeSpan elapsed = stopwatch.Elapsed;
                Log.Info($"Done! ({Log.FormatElapsedTime(elapsed)})\n");
            }
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}