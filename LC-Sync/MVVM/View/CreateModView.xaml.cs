using System;
using System.CodeDom;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using LC_Sync.Core.LCSync;
using LC_Sync.Core.Util;

namespace LC_Sync.MVVM.View
{
    public partial class CreateModView : UserControl
    {
        public static CreateModView Instance { get; private set; }

        public CreateModView()
        {
            InitializeComponent();
            DataContext = this;
            Instance = this;

            LCSyncData.ClearMods();

            LogTextBlock.Text = Log.LogString;
            Log.PropertyChanged += Log_PropertyChanged;

            // Initialize the timer
            textChangeTimer = new System.Timers.Timer();
            textChangeTimer.Interval = 1000; // Set the delay in milliseconds (adjust as needed)
            textChangeTimer.AutoReset = false;
            textChangeTimer.Elapsed += TextChangeTimerElapsed;

            RemoveModButton.IsEnabled = false;
            PublishModButton.IsEnabled = false;

            ModTextBlock.Text = modListLog;
            ModSearchTextBlock.Text = "Available Mods:\n";
        }

        private string _modInput = "";
        public string ModInput
        {
            get { return _modInput; }
            set
            {
                if (_modInput != value)
                {
                    _modInput = value;
                    OnPropertyChanged(nameof(ModInput));
                }
            }
        }
        public string modListLog = "Added mods:\n";
        public List<ModInfo> modStorage = new List<ModInfo>();

        private DateTime lastUpdated = DateTime.Now;
        private System.Timers.Timer textChangeTimer;

        private void ModBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            enableSpinner();
            lastUpdated = DateTime.Now;

            textChangeTimer.Stop();
            textChangeTimer.Start();
        }

        private void TextChangeTimerElapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            // Check if no text changes occurred during the specified delay
            if ((DateTime.Now - lastUpdated).TotalMilliseconds >= textChangeTimer.Interval)
            {
                // Execute your custom logic here
                List<ModInfo> modsFound = new List<ModInfo>();

                foreach (ModInfo item in LCSyncData.TSPackageIndex)
                {
                    if (item.ModName.Contains(ModInput) || 
                        item.ModName.ToLower().Contains(ModInput) ||
                        item.ModNamespace.Contains(ModInput) || 
                        item.ModNamespace.ToLower().Contains(ModInput) ||
                        ModInput.Contains($"{item.ModName}/{item.ModNamespace}"))
                    {
                        modsFound.Add(item);
                    }
                }

                string modsFoundString = "";
                foreach (ModInfo item in modsFound)
                {
                    modsFoundString += "- " + item.ModName + "/" + item.ModNamespace + "\n";
                }

                Dispatcher.Invoke(() => ModSearchTextBlock.Text = "Available Mods:\n" + modsFoundString);
                disableSpinner();
            }
        }

        private async void AddMod_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(ModInput))
            {
                Log.ShowCustomMessageBox("ERROR", "Invalid input!");
                return;
            }

            enableSpinner();
            bool itemFound = false;
            foreach (ModInfo item in LCSyncData.TSPackageIndex)
            {
                string itemstring = item.ModName + "/" + item.ModNamespace;

                if (itemstring == ModInput || itemstring.ToLower() == ModInput)
                {
                    foreach (ModInfo storageMod in modStorage)
                    {
                        if (storageMod.ModName == item.ModName && storageMod.ModNamespace == item.ModNamespace)
                        {
                            disableSpinner();
                            Log.ShowCustomMessageBox("ERROR", "Mod already added!");
                            return;
                        }
                    }

                    modStorage.Add(item);

                    modListLog += "- " + itemstring + "\n";
                    ModTextBlock.Text = modListLog;
                    RemoveModButton.IsEnabled = true;
                    PublishModButton.IsEnabled = true;
                    itemFound = true;
                }
            }

            if (!itemFound)
            {
                disableSpinner();
                Log.ShowCustomMessageBox("ERROR", "Mod not found!");
                return;
            }

            disableSpinner();
        }

        private async void RLModRegister_Click(object sender, RoutedEventArgs e)
        {
            if (File.Exists(SteamHandler.LCInstallationPath + "\\package-index.json"))
            {
                File.Delete(SteamHandler.LCInstallationPath + "\\package-index.json");
                enableSpinner();
                await LCSyncData.InitPackageIndex();
                disableSpinner();
            } else
            {
                Log.ShowCustomMessageBox("ERROR", "Index not found!");
            }
        }

        private async void Publish_Click(object sender, RoutedEventArgs e)
        {
            enableSpinner();
            foreach (ModInfo item in modStorage)
            {
                TSMod newMod = new TSMod(item.ModNamespace, item.ModName);
                await newMod.InitializeAsync();

                LCSyncData.TSMods.Add(newMod);
            }

            foreach (TSMod item in LCSyncData.TSMods)
            {
                await FileHandler.InstallModAsync(item);
            }

            string key = await LCSyncData.UploadSrcbinModsAsync();
            Clipboard.SetText(key);
            FileHandler.setPreviousKey(key);

            disableSpinner();

            Log.ShowCustomMessageBox("Success", "Copied key to clipboard.");
        }

        private void RemoveMod_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(ModInput))
            {
                Log.ShowCustomMessageBox("ERROR", "Invalid input!");
                return;
            }

            enableSpinner();
            bool itemFound = false;
            for (int i = modStorage.Count - 1; i >= 0; i--)
            {
                ModInfo item = modStorage[i];
                string itemstring = item.ModName + "/" + item.ModNamespace;

                if (itemstring == ModInput || itemstring.ToLower() == ModInput)
                {
                    modStorage.Remove(item);

                    modListLog = modListLog.Replace("- " + itemstring + "\n", "");
                    ModTextBlock.Text = modListLog;

                    if (modStorage.Count == 0)
                    {
                        RemoveModButton.IsEnabled = false;
                        PublishModButton.IsEnabled = false;
                    }

                    itemFound = true;
                }
            }

            if (!itemFound)
            {
                disableSpinner();
                Log.ShowCustomMessageBox("ERROR", "Mod not found!");
                return;
            }

            disableSpinner();
        }

        private async void ModBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (LCSyncData.TSPackageIndex.Count == 0)
            {
                enableSpinner();
                await LCSyncData.InitPackageIndex();
                disableSpinner();
            }

            if (ModInput == "Enter Url/Name")
            {
                ModInput = string.Empty;
            }
        }

        public void enableSpinner()
        {
            Dispatcher.Invoke(() =>
            {
                Spinner.Spin = true;
                Spinner.Opacity = 1;
            });
        }

        private void disableSpinner()
        {
            Dispatcher.Invoke(() =>
            {
                Spinner.Spin = false;
                Spinner.Opacity = 0.25;
            });
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