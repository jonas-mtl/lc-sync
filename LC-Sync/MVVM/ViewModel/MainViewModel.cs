using LC_Sync.Core;


namespace LC_Sync.MVVM.ViewModel
{
    internal class MainViewModel : ObservableObject
    {
        public RelayCommand SyncViewCommand { get; set; }
        public RelayCommand CreateModViewCommand { get; set; }

        public SyncViewModel SyncVM { get; set; }
        public CreateModViewModel CreateModVM { get; set; }

        private object _currentView;

        public object CurrentView
        {
            get { return _currentView; }
            set 
            { 
                _currentView = value;
                OnPropertyChanged();
            }
        }


        public MainViewModel() 
        {
            SyncVM = new SyncViewModel();
            CreateModVM = new CreateModViewModel();

            CurrentView = SyncVM;

            SyncViewCommand = new RelayCommand(o =>
            {
                CurrentView = SyncVM;
            });

            CreateModViewCommand = new RelayCommand(o =>
            {
                CurrentView = CreateModVM;
            });
        }
    }
}
