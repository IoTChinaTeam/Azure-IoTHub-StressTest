using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Messaging;
using StressLoadDemo.Model.DataProvider;
using StressLoadDemo.Model.Utility;

namespace StressLoadDemo.ViewModel
{
    /// <summary>
    /// This class contains properties that the main View can data bind to.
    /// <para>
    /// Use the <strong>mvvminpc</strong> snippet to add bindable properties to this ViewModel.
    /// </para>
    /// <para>
    /// You can also use Blend to data bind with the tool's support.
    /// </para>
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class MainViewModel : ViewModelBase
    {
        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        private readonly IStressDataProvider _dataProvider;

        private int _selectedTabIndex;
        private bool _testStart;
        private bool _monitorStart;
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

        public bool MonitorStart
        {
            get { return _monitorStart; }
            set
            {
                if (value!=_monitorStart)
                {
                    _monitorStart = value;
                    Messenger.Default.Send<IStressDataProvider>(_dataProvider, "StartMonitor");
                }
            }
        }

        public bool TestStart
        {
            get { return _testStart;}
            set
            {
                if (value)
                {
                    Messenger.Default.Send<IStressDataProvider>(_dataProvider, "StartTest");
                }
            }
        }
        
        public MainViewModel(IStressDataProvider provider)
        {
            _dataProvider = provider;

        }

        public void AppendBatchJobId(string batchJobId)
        {
            _dataProvider.BatchJobId = batchJobId;
        }
        public void AppendToProvider(RequirementMessage message)
        {
            _dataProvider.NumOfVm = message.VmCount.ToString();
            _dataProvider.DevicePerVm = message.NumberOfDevicePerVm.ToString();
            _dataProvider.ExpectTestDuration = message.TestDuration.ToString();
            _dataProvider.MessagePerMinute = message.MessagePerMinPerDevice;
            _dataProvider.VmSize = message.AzureVmSize.ToString();
        }
    }
}