using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using StressLoadDemo.Model;
using StressLoadDemo.Model.DataProvider;
using StressLoadDemo.Model.Utility;
using System.Windows.Media;

namespace StressLoadDemo.ViewModel
{
    /// <summary>
    /// This class contains properties that a View can data bind to.
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class TabResourceViewModel : ViewModelBase
    {
        //max length of data buffered for drawing graph.
        //Hardcoded to the width of the canvas.
        private const double CanvasWidth = 265;
        private const double CanvasHeight = 111;

        private readonly IStressDataProvider _dataProvider;

        string _specDeviceCount, _specMsgFreq, _specDuration;
        private string _hubOwnerConnectionString;
        private string _eventHubEndpoint;
        private string _batchServiceUrl;
        private string _batchAccountKey;
        private string _storageAccountConnectionString;
        private bool _canStartTest;
        string logmsg;
        bool isLogsChangedPropertyInViewModel;

        public DeployPhase _currentDeployPhase;
        public PhaseStatus _currentPhaseStatus;

        private int _progressBarValue;
        private Visibility[] _lableVisibilities;
        /// <summary>
        /// Initializes a new instance of the TabDashboardViewModel class.
        /// </summary>
        public TabResourceViewModel(IStressDataProvider provider)
        {
            Messenger.Default.Register<RequirementMessage>(
                this,
                "AppendRequirementParam",
                data => AppendToProvider(data)
                );
            
            Messenger.Default.Register<IStressDataProvider>(
                this,
                "StartTest",
                data => ProcessRunConfigValue(data)
                );
            Messenger.Default.Register<string>(
                this,
                "RunningLog",
                msg => ShowLog(msg)
                );
            Messenger.Default.Register<DeployStatusUpdateMessage>(
               this,
               "DeployStatus",
               message => SetDeployStatus(message)
               );
            Messenger.Default.Register<string>(
               this,
               "BatchJobId",
               AppendBatchJobId
               );
            _lableVisibilities = new Visibility[5] {Visibility.Hidden,Visibility.Hidden,Visibility.Hidden,Visibility.Hidden,Visibility.Hidden };
               
            _specDeviceCount = _specDuration = _specMsgFreq = "Not Specified";
            _currentDeployPhase = DeployPhase.DeployStarted;
            _currentPhaseStatus = PhaseStatus.Succeeded;
            _dataProvider = provider;
            _canStartTest = false;

        }

        #region BindingProperties

        public string SpecDeviceCount
        {
            get { return _specDeviceCount; }
            set
            {
                _specDeviceCount = value;
                RaisePropertyChanged();
                TryActivateButton();
            }
        }

        public string SpecMsgFreq
        {
            get { return _specMsgFreq; }
            set
            {
                _specMsgFreq = value;
                RaisePropertyChanged();
                TryActivateButton();
            }
        }

        public string SpecDuration
        {
            get { return _specDuration; }
            set
            {
                _specDuration = value;
                RaisePropertyChanged();
                TryActivateButton();
            }
        }

        public bool CanStartTest
        {
            get
            {
                return _canStartTest;
            }
            set
            {
                _canStartTest = value;
                RaisePropertyChanged();
            }
        }

        public string HubOwnerConnectionString
        {
            get { return _hubOwnerConnectionString; }
            set
            {
                _hubOwnerConnectionString = value;
                RaisePropertyChanged();
                TryActivateButton();
            }
        }

        public string EventHubEndpoint
        {
            get
            {
                return _eventHubEndpoint;
            }
            set
            {
                _eventHubEndpoint = value;
                RaisePropertyChanged();
                TryActivateButton();

            }
        }

        public string BatchServiceUrl
        {
            get { return _batchServiceUrl; }
            set
            {
                _batchServiceUrl = value;
                RaisePropertyChanged();
                TryActivateButton();
            }
        }

        public string BatchAccountKey
        {
            get { return _batchAccountKey; }
            set
            {
                _batchAccountKey = value;
                RaisePropertyChanged();
                TryActivateButton();
            }
        }

        public string StorageAccountConnectionString
        {
            get { return _storageAccountConnectionString; }
            set
            {
                _storageAccountConnectionString = value;
                RaisePropertyChanged();
                TryActivateButton();
            }
        }
        #endregion
        public int ProgressValue
        {
            get { return _progressBarValue; }
            set
            {
                _progressBarValue = value;
                RaisePropertyChanged();
            }
        }
        public Visibility StartLableVisibility
        {
            get
            {
                return _lableVisibilities[0];
            }
            set
            {
                _lableVisibilities[0] = value;
                RaisePropertyChanged();
            }
        }
        public Visibility PoolLableVisibility
        {
            get
            {
                return _lableVisibilities[1];
            }
            set
            {
                _lableVisibilities[1] = value;
                RaisePropertyChanged();
            }
        }
        public Visibility AssemblyLableVisibility
        {
            get
            {
                return _lableVisibilities[2];
            }
            set
            {
                _lableVisibilities[2] = value;
                RaisePropertyChanged();
            }
        }

        public Visibility JobLableVisibility
        {
            get
            {
                return _lableVisibilities[3];
            }
            set
            {
                _lableVisibilities[3] = value;
                RaisePropertyChanged();
            }
        }
        public Visibility FinishLableVisibility
        {
            get
            {
                return _lableVisibilities[4];
            }
            set
            {
                _lableVisibilities[4] = value;
                RaisePropertyChanged();
            }
        }

        public RelayCommand StartTest => new RelayCommand(RunTest);

        public string LogMsg
        {
            get { return logmsg; }
            set
            {
                logmsg = value;
                IsLogsChangedPropertyInViewModel = true;
                RaisePropertyChanged();
            }
        }
        void RunTest()
        {
            new ViewModelLocator().Main.TestStart = true;
            StartLableVisibility = Visibility.Visible;
        }
        public bool IsLogsChangedPropertyInViewModel
        {
            get { return isLogsChangedPropertyInViewModel; }
            set
            {
                isLogsChangedPropertyInViewModel = value;
                RaisePropertyChanged();
            }
        }

        void ShowLog(object message)
        {
            LogMsg += message;
            LogMsg += "\n";
        }


        public void AppendToProvider(RequirementMessage message)
        {
            _dataProvider.NumOfVm = message.VmCount.ToString();
            _dataProvider.DevicePerVm = message.NumberOfDevicePerVm.ToString();
            _dataProvider.ExpectTestDuration = message.TestDuration.ToString();
            _dataProvider.MessagePerMinute = message.MessagePerMinPerDevice;
            _dataProvider.VmSize = message.AzureVmSize.ToString();

            var devicecountint = int.Parse(_dataProvider.DevicePerVm) * int.Parse(_dataProvider.NumOfVm);
            var messagefreqint = _dataProvider.MessagePerMinute;
            SpecDeviceCount = $"Device Number : {devicecountint}";
            SpecMsgFreq = $"Message Rate: {messagefreqint} messages/minute/device";
            SpecDuration = $"Duration: {_dataProvider.ExpectTestDuration} minutes";
        }

        void SetDeployStatus(DeployStatusUpdateMessage status)
        {
            var deployPhase = (int)status.Phase;
            switch (deployPhase)
            {
                case 1:
                    PoolLableVisibility = Visibility.Visible;
                    ProgressValue = 1;
                    break;
                case 2:
                    AssemblyLableVisibility = Visibility.Visible;
                    ProgressValue = 2;
                    break;
                case 3:
                    JobLableVisibility = Visibility.Visible;
                    ProgressValue = 3;
                    break;
                case 4:
                    FinishLableVisibility = Visibility.Visible;
                    ProgressValue = 4;
                    MoveOnToMonitor();
                    break;
            }
            var CurrentPhaseStatus = status.Status;
        }

        void MoveOnToMonitor()
        {
            var mainvm = new ViewModelLocator().Main;
            mainvm.MonitorStart = true;
        }

        bool IsDeployValuesValid(IStressDataProvider provider)
        {
            return false;
        }

        void ProcessRunConfigValue(IStressDataProvider provider)
        {
            provider.BatchKey = _batchAccountKey;
            provider.HubOwnerConectionString = _hubOwnerConnectionString;
            provider.EventHubEndpoint = _eventHubEndpoint;
            provider.BatchUrl = _batchServiceUrl;
            provider.StorageAccountConectionString = _storageAccountConnectionString;
            provider.Run();
        }

        public void AppendBatchJobId(string batchJobId)
        {
            _dataProvider.BatchJobId = batchJobId;
        }

        void TryActivateButton()
        {
            if (!(string.IsNullOrEmpty(_hubOwnerConnectionString) ||
                string.IsNullOrEmpty(_eventHubEndpoint) ||
                string.IsNullOrEmpty(_batchAccountKey) ||
                string.IsNullOrEmpty(_batchServiceUrl) ||
                string.IsNullOrEmpty(_storageAccountConnectionString) ||
                _dataProvider.MessagePerMinute == 0))
            {
                CanStartTest = true;
            }
            else
            {
                CanStartTest = false;
            }
        }
    }
}