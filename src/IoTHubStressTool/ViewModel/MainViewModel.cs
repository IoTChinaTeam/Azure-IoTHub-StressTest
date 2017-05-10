using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Messaging;
using StressLoadDemo.Model.DataProvider;
using StressLoadDemo.Model.Utility;

namespace StressLoadDemo.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        private readonly IStressDataProvider _dataProvider;

        private int _selectedTabIndex;
        int[] tabW = { 830, 950, 950 };
        int[] tabH = { 400, 710, 600 };
        int _mainW, _mainH;

        public int SelectedTabIndex
        {
            get { return _selectedTabIndex; }
            set
            {
                MainWidth = tabW[_selectedTabIndex];
                MainHeight = tabH[_selectedTabIndex];
                handleTabSwitch(value,_selectedTabIndex);
                _selectedTabIndex = value;
                RaisePropertyChanged();
            }
        }

        void handleTabSwitch(int newValue,int prevValue)
        {
            if (prevValue == 0)
            {
                //every time user leaves tab #1, pass user input into tab#2.
                var reqTabVM = new ViewModelLocator().RequireTab;
                if (reqTabVM.SendSpecToTab2.CanExecute(reqTabVM))
                {
                    reqTabVM.SendSpecToTab2.Execute(reqTabVM);
                }
            }
        }

        public int MainWidth
        {
            get { return _mainW; }
            set {
                _mainW = value;
                RaisePropertyChanged();
            }
        }

        public int MainHeight
        {
            get { return _mainH; }
            set
            {
                _mainH = value;
                RaisePropertyChanged();
            }
        }

        public MainViewModel(IStressDataProvider provider)
        {
            _dataProvider = provider;
        }
    }
}