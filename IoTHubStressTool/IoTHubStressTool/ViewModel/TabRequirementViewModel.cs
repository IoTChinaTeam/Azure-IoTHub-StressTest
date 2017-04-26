using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using StressLoadDemo.Helpers;
using StressLoadDemo.Model.AzureConstants;
using StressLoadDemo.Model.DataProvider;
using StressLoadDemo.Model.Utility;

namespace StressLoadDemo.ViewModel
{
    /// <summary>
    /// This class contains properties that a View can data bind to.
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class TabRequirementViewModel : ViewModelBase
    {
        private int _totalDevice;
        private int _totalMessagePerMinute,_messagePerMinutePerDevice;
        private int _testDuration;
        private string _iothubrecommendation;
        private string _vmRecommendation;
        private HubSku _hubInfo;
        private VmSku _vmInfo;
        private bool _buttonEnabled;
        private IStressDataProvider _dataProvider;

        /// <summary>
        /// Initializes a new instance of the TabDashboardViewModel class.
        /// </summary>
        public TabRequirementViewModel(IStressDataProvider provider,MainViewModel mainVm)
        {
            _dataProvider = provider;        
            _iothubrecommendation = "";
            _vmRecommendation= "";
            _buttonEnabled = false;
        }

        public RelayCommand OpenHubLink => new RelayCommand(()=>
        {
            System.Diagnostics.Process.Start("https://docs.microsoft.com/en-us/azure/iot-hub/iot-hub-devguide-quotas-throttling");
        });

        public RelayCommand OpenPriceLinkChina => new RelayCommand(()=> 
        {
            System.Diagnostics.Process.Start("https://www.azure.cn/pricing/details/iot-hub/");
        });
        
        public RelayCommand OpenPriceLinkGlobal => new RelayCommand(() =>
        {
            System.Diagnostics.Process.Start("https://azure.microsoft.com/zh-cn/pricing/details/iot-hub/");
        });

        public RelayCommand SendSpecToTab2 => new RelayCommand(
            () =>
            {
                RequirementMessage message = new RequirementMessage()
                {
                    IoTHubSize = _hubInfo.UnitSize,
                    IoTHubUnitCount = _hubInfo.UnitCount,
                    AzureVmSize = _vmInfo.Size,
                    VmCount = _vmInfo.VmCount,
                    MessagePerMinPerDevice = _totalMessagePerMinute / _totalDevice,
                    NumberOfDevicePerVm = int.Parse(TotalDevice) / _vmInfo.VmCount,
                    TestDuration = _testDuration

                };

                Messenger.Default.Send<RequirementMessage>(message, "AppendRequirementParam");
            },
            () => ButtonEnabled
            );

        public bool ButtonEnabled
        {
            get { return _buttonEnabled; }
            set
            {
                _buttonEnabled = value;
                RaisePropertyChanged();
            }
        }

        public string TotalDevice
        {
            get {return _totalDevice.ToString();}
            set
            {
                int.TryParse(value, out _totalDevice);
                if (_messagePerMinutePerDevice != 0)
                {
                    MessagePerMin = (_totalDevice * _messagePerMinutePerDevice).ToString();
                }
                if (_totalDevice != 0)
                {
                    RecommendVm(_totalDevice);
                }
                RaisePropertyChanged();
                TryActivateButton();
            }
        }

        public string MessagePerMinPerDevice
        {
            get { return _messagePerMinutePerDevice.ToString(); }
            set
            {
                int.TryParse(value, out _messagePerMinutePerDevice);
                if (_totalDevice != 0)
                {
                    MessagePerMin = (_totalDevice * _messagePerMinutePerDevice).ToString();
                }
                TryActivateButton();
                RaisePropertyChanged();
            }
        }

        public string MessagePerMin
        {
            get { return _totalMessagePerMinute.ToString(); }
            set
            {
                int.TryParse(value,out _totalMessagePerMinute);
                RaisePropertyChanged();
                TryActivateButton();
                RecommendHub(_totalMessagePerMinute);
            }
        }

        public string TestDuration
        {
            get
            { return _testDuration.ToString(); }
            set
            {
                int.TryParse(value,out _testDuration);
                TryActivateButton();
            }
        }

        public string HubSkuRecommendation
        {
            get { return _iothubrecommendation;}
            set
            {
                _iothubrecommendation = value;
                RaisePropertyChanged();
            }
        }

        public string VmSkuRecommendation
        {
            get { return _vmRecommendation; }
            set
            {
                _vmRecommendation = value;
                RaisePropertyChanged();
            }
        }

        public void RecommendHub(int messagePerminute)
        {
            _hubInfo = SkuCalculator.CalculateHubSku(messagePerminute);
            HubSkuRecommendation = _hubInfo.UnitSize.ToString() + " x " + _hubInfo.UnitCount;
        }

        public void RecommendVm(int totalDevice)
        {
            _vmInfo = SkuCalculator.CalculateVmSku(totalDevice);
            VmSkuRecommendation = _vmInfo.Size.ToString() + " x " + _vmInfo.VmCount;
        }

        public void TryActivateButton()
        {
            if (_testDuration != 0
                && _totalMessagePerMinute != 0
                && _totalDevice != 0)
            {
                _vmInfo = SkuCalculator.CalculateVmSku(_totalDevice);
                VmSkuRecommendation = _vmInfo.Size.ToString() + " x " + _vmInfo.VmCount;
                ButtonEnabled = true;
            }
            else
            {
                ButtonEnabled = false;
            }
        }
    }
}