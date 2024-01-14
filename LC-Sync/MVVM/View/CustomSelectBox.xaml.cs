using System.Windows;
using System.Windows.Media;

namespace LC_Sync.MVVM.View
{
    public partial class CustomSelectBox : Window
    {
        public static new readonly DependencyProperty TitleProperty = DependencyProperty.Register("Title", typeof(string), typeof(CustomSelectBox));
        public static readonly DependencyProperty TextProperty = DependencyProperty.Register("Text", typeof(string), typeof(CustomSelectBox));

        public new string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }

        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        public bool returnValue;
        public bool closed = false;

        public CustomSelectBox()
        {
            InitializeComponent();
            DataContext = this;

            // Set WindowStartupLocation to Manual
            WindowStartupLocation = WindowStartupLocation.Manual;

            // Calculate the position to center the CustomMessageBox in the main window
            if (Application.Current.MainWindow != null)
            {
                double mainWinLeft = Application.Current.MainWindow.Left;
                double mainWinTop = Application.Current.MainWindow.Top;
                double mainWinWidth = Application.Current.MainWindow.Width;
                double mainWinHeight = Application.Current.MainWindow.Height;

                Left = mainWinLeft + (mainWinWidth - Width) / 2;
                Top = mainWinTop + 20;
            }
        }

        public void setWindowBackground(SolidColorBrush color)
        {
            Wrapper.Background = color;
        }

        private void Window_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.LeftButton == System.Windows.Input.MouseButtonState.Pressed)
            {
                DragMove();
            }
        }

        private void ButtonYes_Click(object sender, RoutedEventArgs e)
        {
            returnValue = true;
            closed = true;

            Close();
        }

        private void ButtonNo_Click(object sender, RoutedEventArgs e)
        {
            returnValue = false;
            closed = true;

            Close();
        }
    }
}