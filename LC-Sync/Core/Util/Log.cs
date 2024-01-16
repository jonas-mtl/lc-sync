using LC_Sync.MVVM.View;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media;


namespace LC_Sync.Core.Util
{
    public class Log
    {
        private static string _logString = "";
        public static string LogString
        {
            get { return _logString; }
            set
            {
                if (_logString != value)
                {
                    _logString = value;
                    OnPropertyChanged();
                }
            }
        }

        public static void Info(string message)
        {
            LogString += "[" + GetCurrentTimestamp() + "] " + message + "\n";
            Console.WriteLine("[" + GetCurrentTimestamp() + "] " + message + "\n");
        }

        public static void Errored(string message)
        {
            LogString += "[" + GetCurrentTimestamp() + "] ERROR " + message + "\n";
            Console.WriteLine("[" + GetCurrentTimestamp() + "] ERROR " + message + "\n");
        }

        public static void ShowCustomMessageBox(string title, string text)
        {
            CustomMessageBox customMessageBox = new CustomMessageBox();
            customMessageBox.Title = title;
            customMessageBox.Text = text;
            customMessageBox.ShowDialog();
        }

        public static void ShowCustomMessageBox(string title, string text, SolidColorBrush backgroundColor)
        {
            CustomMessageBox customMessageBox = new CustomMessageBox();
            customMessageBox.Title = title;
            customMessageBox.Text = text;
            customMessageBox.setWindowBackground(backgroundColor);
            customMessageBox.ShowDialog();
        }

        public static void ShowHelp()
        {
            HelpView helpView = new HelpView();
            helpView.ShowDialog();
        }

        private static string GetCurrentTimestamp() {
            // Get the current time
            DateTime currentTime = DateTime.Now;

            // Format the time as a string in "HH:mm:ss" format
            string timestamp = currentTime.ToString("HH:mm:ss");

            return timestamp;
        }

        public static string FormatElapsedTime(TimeSpan elapsedTime)
        {
            if (elapsedTime.TotalMilliseconds < 1000)
            {
                return $"{elapsedTime.TotalMilliseconds:F2} ms";
            }
            else if (elapsedTime.TotalSeconds < 60)
            {
                return $"{elapsedTime.TotalSeconds:F2} s";
            }
            else
            {
                return $"{elapsedTime.TotalMinutes:F2} min";
            }
        }

        public static event PropertyChangedEventHandler PropertyChanged;

        protected static void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(null, new PropertyChangedEventArgs(propertyName));
        }
    }
}
