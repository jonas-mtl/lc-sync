using System;
using System.CodeDom;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using LC_Sync.Core;
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
            textChangeTimer.Interval = 500;
            textChangeTimer.AutoReset = false;
            textChangeTimer.Elapsed += TextChangeTimerElapsed;

            PublishModButton.IsEnabled = false;
            ModTextBlock.Text = modListLog + InitCore.currentlyLoadedModlist;
            modListLog = modListLog + InitCore.currentlyLoadedModlist;
            modStorage = InitCore.currentlyLoadedMods;

            if (modStorage.Count != 0) PublishModButton.IsEnabled = true;

            while (InitCore.updatingPackageIndex)
            {
                enableSpinner();
                ModBox.IsReadOnly = true;
                ModBox.Text = "Mod index is updating...";
            }

            ModBox.Text = "";
            ModBox.IsReadOnly = false;

            disableSpinner();
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
                List<ModInfo> modsFound = new List<ModInfo>();
                Application.Current.Dispatcher.Invoke(() =>
                {
                    ModListStackPanel.Children.Clear();
                });

                StringCompare sc = new StringCompare(65,75,20,30);
                foreach (ModInfo item in LCSyncData.TSPackageIndex)
                {
                    if (sc.IsEqual(ModInput, item.ModName))
                    {
                        createModController(item);
                    }
                }

                string modsFoundString = "";
                foreach (ModInfo item in modsFound)
                {
                    modsFoundString += "- " + item.ModName + "/" + item.ModNamespace + "\n";
                }

                disableSpinner();
            }
        }

        private void AddMod_Click(object sender, RoutedEventArgs e)
        {
            enableSpinner();

            string modListString = "";

            if (sender is FrameworkElement button)
            {
                DependencyObject parent = VisualTreeHelper.GetParent(button);

                if (parent is StackPanel stackPanel)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(stackPanel, 0) as DependencyObject;

                    if (child is TextBox textbox)
                    {

                        modListString = textbox.Text;
                    }
                }
            }

            string modListData = modListString.Substring(2);
            string[] modListParsed = modListData.Split(new string[] { " by " }, StringSplitOptions.None);

            ModInfo currentMod = new ModInfo() { ModName = modListParsed[0] , ModNamespace = modListParsed[1] };

            bool itemFound = false;
            foreach (ModInfo item in LCSyncData.TSPackageIndex)
            {
                if (currentMod.ModNamespace == item.ModNamespace && currentMod.ModName == item.ModName)
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

                    modListLog += modListString + "\n";
                    ModTextBlock.Text = modListLog;
                    itemFound = true;
                }
            }

            if (!itemFound)
            {
                disableSpinner();
                Log.ShowCustomMessageBox("ERROR", "Mod not found!");
                return;
            }

            if (modStorage.Count > 0)
            {
                PublishModButton.IsEnabled = true;
            }

            disableSpinner();
        }

        private async void RLModRegister_Click(object sender, RoutedEventArgs e)
        {
            if (File.Exists(SteamHandler.LCInstallationPath + "\\package-index.json"))
            {
                File.Delete(SteamHandler.LCInstallationPath + "\\package-index.json");
            }

            enableSpinner();
            await LCSyncData.InitPackageIndex();
            disableSpinner();
        }

        private void ModHelp_Click (object sender, RoutedEventArgs e)
        {
            Log.ShowHelp();
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
            enableSpinner();

            string modListString = "";

            if (sender is FrameworkElement button)
            {
                DependencyObject parent = VisualTreeHelper.GetParent(button);

                if (parent is StackPanel stackPanel)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(stackPanel, 0) as DependencyObject;

                    if (child is TextBox textbox)
                    {

                        modListString = textbox.Text;
                    }
                }
            }

            string modListData = modListString.Substring(2);
            string[] modListParsed = modListData.Split(new string[] { " by " }, StringSplitOptions.None);

            ModInfo currentMod = new ModInfo() { ModName = modListParsed[0], ModNamespace = modListParsed[1] };

            for (int i = modStorage.Count - 1; i >= 0; i--)
            {
                ModInfo item = modStorage[i];

                if (item.ModName == currentMod.ModName && item.ModNamespace == item.ModNamespace)
                {
                    modStorage.Remove(item);

                    modListLog = modListLog.Replace(modListString + "\n", "");
                    ModTextBlock.Text = modListLog;
                }
            }

            if (modStorage.Count == 0)
            {
                PublishModButton.IsEnabled = false;
            }

            disableSpinner();
        }

        private void createModController(ModInfo mod)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                TextBox modNameTextBox = new TextBox();
                modNameTextBox.Text = $"- {mod.ModName} by {mod.ModNamespace}";
                modNameTextBox.Margin = new Thickness(10, 0, 0, 0);
                modNameTextBox.IsReadOnly = true;
                modNameTextBox.VerticalAlignment = VerticalAlignment.Center;
                modNameTextBox.Style = (Style)Application.Current.Resources["NoFocusVisualStyleTextBox"];
                modNameTextBox.BorderThickness = new Thickness(0);
                modNameTextBox.Foreground = new SolidColorBrush(Colors.White);
                modNameTextBox.FontFamily = new FontFamily(new Uri("pack://application:,,,/"), "./Fonts/#3270");
                modNameTextBox.FontSize = 15;
                modNameTextBox.Background = new SolidColorBrush(Colors.Transparent);

                Button addButton = new Button();
                addButton.Click += (s, e) => { AddMod_Click(s, e); };
                addButton.Content = "+";
                addButton.Margin = new Thickness(10, 5, 10, 5);
                addButton.Width = 30;
                addButton.Height = 20;
                addButton.FontFamily = new FontFamily("/Fonts/#3270");
                addButton.Style = (Style)Application.Current.Resources["ButtonTheme"];


                Button removeButton = new Button();
                removeButton.Click += (s, e) => { RemoveMod_Click(s, e); };
                removeButton.Content = "-";
                removeButton.Width = 30;
                removeButton.Height = 20;
                removeButton.FontFamily = new FontFamily("/Fonts/#3270");
                removeButton.Style = (Style)Application.Current.Resources["ButtonTheme"];

                StackPanel buttonStackPanel = new StackPanel
                {
                    Orientation = Orientation.Horizontal
                };

                buttonStackPanel.Children.Add(modNameTextBox);
                buttonStackPanel.Children.Add(addButton);
                buttonStackPanel.Children.Add(removeButton);

                ModListStackPanel.Children.Add(buttonStackPanel);
            });
        }

        private void ModBox_GotFocus(object sender, RoutedEventArgs e)
        {
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