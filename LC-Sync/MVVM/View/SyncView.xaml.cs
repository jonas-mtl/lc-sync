using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using LC_Sync.Core.LCSync;
using LC_Sync.Core.Util;

namespace LC_Sync.MVVM.View
{
    public partial class SyncView : UserControl
    {
        public SyncView()
        {
            InitializeComponent();
            DataContext = this;

            LCSyncData.ClearMods();

            LogTextBlock.Text = Log.LogString;
            Log.PropertyChanged += Log_PropertyChanged;


            UsePrevKeyButton.Visibility = Visibility.Collapsed;
            if (!string.IsNullOrEmpty(LCSyncData.StoredSrcBinKey))
            {
                UsePrevKeyButton.Visibility = Visibility.Visible;
                UsePrevKeyButton.Content = $"Use key ({LCSyncData.StoredSrcBinKey})";
            }
        }

        private string _keyText = "";
        public string KeyText
        {
            get { return _keyText; }
            set
            {
                if (_keyText != value)
                {
                    _keyText = value;
                    OnPropertyChanged(nameof(KeyText));
                }
            }
        }

        private void SyncButton_Click(object sender, RoutedEventArgs e)
        {
            // Get the text from the TextBox and call the syncMods function
            string key = KeyText;
            syncMods(key);
        }

        private void SyncButtonPrevKey_Click(object sender, RoutedEventArgs e)
        {
            syncMods(LCSyncData.StoredSrcBinKey);
        }

        private void KeyTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            // Clear the default text when the TextBox gets focus
            if (KeyText == "Enter key")
            {
                KeyText = string.Empty;
            }
        }

        // Your syncMods function
        private async void syncMods(string key)
        {
            if(string.IsNullOrEmpty(key))
            {
                Log.ShowCustomMessageBox("ERROR", "Please enter a key!");
                return;
            }

            enableSpinner();

            LCSyncData.SrcbinKey = key;
            if (!await LCSyncData.UpdateSrcbinModsAsync())
            {
                disableSpinner();
                Log.ShowCustomMessageBox("ERROR", "Invalid Key!");
                return;
            }

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            Log.Info("Syncing...");

            foreach (TSMod mod in LCSyncData.TSMods)
            {
                await FileHandler.InstallModAsync(mod);
            }

            TimeSpan elapsed = stopwatch.Elapsed;
            FileHandler.setPreviousKey(key);
            if (!string.IsNullOrEmpty(LCSyncData.StoredSrcBinKey))
            {
                UsePrevKeyButton.Visibility = Visibility.Visible;
                UsePrevKeyButton.Content = $"Use key ({LCSyncData.StoredSrcBinKey})";
            }

            disableSpinner();
            Log.Info($"DONE! ({Log.FormatElapsedTime(elapsed)})\n");
        }

        private void enableSpinner()
        {
            Spinner.Spin = true;
            Spinner.Opacity = 1;
        }

        private void disableSpinner()
        {
            Spinner.Spin = false;
            Spinner.Opacity = 0.25;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void Log_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Log.LogString))
            {
                LogTextBlock.Text = Log.LogString;
                LogScrollViewer.ScrollToEnd();
            }
        }
    }
}