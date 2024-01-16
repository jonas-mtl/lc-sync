using LC_Sync_updater.Core;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;


namespace LC_Sync_updater
{
    public partial class MainWindow : Window
    {
        private readonly string _currentFolder = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);

        public MainWindow()
        {
            InitializeComponent();

            BackgroundWorker worker = new BackgroundWorker();
            worker.RunWorkerCompleted += worker_RunWorkerCompleted;
            worker.WorkerReportsProgress = true;
            worker.DoWork += worker_DoWork;
            worker.ProgressChanged += worker_ProgressChanged;
            worker.RunWorkerAsync();
        }

        private void worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            ProgressBar.Value = e.ProgressPercentage;
        }

        private void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            var worker = sender as BackgroundWorker;
            worker.ReportProgress(0);

            Task.Run(async () => await Updater.DownloadNewestVersion()).Wait();
            worker.ReportProgress(50);

            Updater.CopyDirectory(_currentFolder + @"\LCSync", _currentFolder);
            worker.ReportProgress(70);

            Directory.Delete(_currentFolder + @"\LCSync", true);
            worker.ReportProgress(100);
        }

        private void worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            string filter = "*.exe";
            string[] files = Directory.GetFiles(_currentFolder, filter);

            foreach (string file in files)
            {
                if (file.EndsWith("LC-Sync.exe"))
                {
                    try
                    {
                        Process.Start(file);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error executing file: {ex.Message}");
                    }

                    Environment.Exit(0);
                }
            }

            MessageBox.Show("File could not be found/exectued");
        }
    }
}
