using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using Microsoft.Azure.Batch;
using Microsoft.Azure.Batch.Auth;
using Microsoft.Azure.Batch.Common;
using StressLoadDemo.Helpers.Configuration;
using StressLoadDemo.Model.DataProvider;
using StressLoadDemo.Model.Utility;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace StressLoadDemo.ViewModel
{
    public class TabResourceViewModel : ViewModelBase
    {
        private readonly IStressDataProvider _dataProvider;

        //spec from Tab 1
        string _specDeviceCount, _specMsgFreq, _specDuration;

        //task deployment params
        private string _hubOwnerConnectionString;
        private string _eventHubEndpoint;
        private string _batchServiceUrl;
        private string _batchAccountKey;
        private string _storageAccountConnectionString;

        //monitor param
        private string _batchJobId;
        private ObservableCollection<string> _batchJobs;

        //UI control params
        private bool _canStartCreate;
        private bool _canStartMonitor;
        string logmsg;
        bool isLogsChangedPropertyInViewModel;
        bool _newJobId, _useExistingJobId;
        int _selectedJobIdIndex;

        //Progress bar params
        public DeployPhase _currentDeployPhase;
        public PhaseStatus _currentPhaseStatus;
        private int _progressBarValue;
        private Visibility[] _lableVisibilities;


        /// <summary>
        /// Initializes a new instance of the TabDashboardViewModel class.
        /// </summary>
        public TabResourceViewModel(IStressDataProvider provider)
        {
            //receive message from tab 1, append to provider.
            Messenger.Default.Register<RequirementMessage>(
                this,
                "AppendRequirementParam",
                data => AppendToProvider(data)
                );
            //receive message from data provider, show log
            Messenger.Default.Register<string>(
                this,
                "RunningLog",
                msg => ShowLog(msg)
                );
            //receive message from data provider, update phase and status
            Messenger.Default.Register<DeployStatusUpdateMessage>(
               this,
               "DeployStatus",
               message => SetDeployStatus(message)
               );
            //receive batch job id from data provider(middle)
            Messenger.Default.Register<string>(
               this,
               "BatchJobId",
               AppendBatchJobId
               );

            //init ui controls
            _selectedJobIdIndex = -1;
            UseExistingJobId = false;
            CreateNewJobId = true;
            _lableVisibilities = new Visibility[5] {Visibility.Hidden,Visibility.Hidden,Visibility.Hidden,Visibility.Hidden,Visibility.Hidden };
            _batchJobs = new ObservableCollection<string>();
            _specDeviceCount = _specDuration = _specMsgFreq = "Not Specified";
            _currentDeployPhase = DeployPhase.DeployStarted;
            _currentPhaseStatus = PhaseStatus.Succeeded;
            _dataProvider = provider;
            _canStartCreate = false;
            LoadConfig();
        }

        #region BindingProperties

        
        public int JobIdSelectedIndex
        {
            get
            {
                return _selectedJobIdIndex;
            }
            set
            {
                _selectedJobIdIndex = value;
                if(value!=-1&&_batchJobId!= BatchJobIds[value])
                {
                    _batchJobId = BatchJobIds[value];
                    TryActivateMonitorButton();
                    TryActivateCreateButton();
                    RaisePropertyChanged();
                }
            }
        }

        public bool UseExistingJobId
        {
            get { return _useExistingJobId; }
            set
            {
                _useExistingJobId = value;
                if(value == true)
                {
                    TryGetBatchJobIds();
                }
                TryActivateMonitorButton();
                RaisePropertyChanged();
            }
        }

        public bool CreateNewJobId
        {
            get { return _newJobId; }
            set
            {
                _newJobId = value;
                TryActivateCreateButton();
                TryActivateMonitorButton();
                RaisePropertyChanged();
            }
        }

        public ObservableCollection<string> BatchJobIds
        {
            get
            {
                return _batchJobs;
            }
            set
            {
                _batchJobs = value;
                RaisePropertyChanged();
            }
        }

        public bool CanStartCreate
        {
            get
            {
                return _canStartCreate;
            }
            set
            {
                _canStartCreate = value;
                RaisePropertyChanged();
            }
        }

        public bool CanStartMonitor
        {
            get { return _canStartMonitor; }
            set
            {
                _canStartMonitor = value;
                RaisePropertyChanged();
            }
        }

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

        public RelayCommand StartTest => new RelayCommand(ProcessRunConfigValue);

        public RelayCommand StartMonitor => new RelayCommand(RunMonitor);

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
        public string SpecDeviceCount
        {
            get { return _specDeviceCount; }
            set
            {
                _specDeviceCount = value;
                RaisePropertyChanged();
                TryActivateCreateButton();
            }
        }

        public string SpecMsgFreq
        {
            get { return _specMsgFreq; }
            set
            {
                _specMsgFreq = value;
                RaisePropertyChanged();
                TryActivateCreateButton();
            }
        }

        public string SpecDuration
        {
            get { return _specDuration; }
            set
            {
                _specDuration = value;
                RaisePropertyChanged();
                TryActivateCreateButton();
            }
        }

        public string HubOwnerConnectionString
        {
            get { return _hubOwnerConnectionString; }
            set
            {
                _hubOwnerConnectionString = value;
                RaisePropertyChanged();
                TryActivateCreateButton();
                TryActivateMonitorButton();
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
                TryActivateCreateButton();
                TryActivateMonitorButton();

            }
        }

        public string BatchServiceUrl
        {
            get { return _batchServiceUrl; }
            set
            {
                _batchServiceUrl = value;
                RaisePropertyChanged();
                TryActivateCreateButton();
            }
        }

        public string BatchAccountKey
        {
            get { return _batchAccountKey; }
            set
            {
                _batchAccountKey = value;
                RaisePropertyChanged();
                TryActivateCreateButton();
            }
        }

        public string StorageAccountConnectionString
        {
            get { return _storageAccountConnectionString; }
            set
            {
                _storageAccountConnectionString = value;
                RaisePropertyChanged();
                TryActivateCreateButton();
            }
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
        #endregion


        void LoadConfig()
        {
            var hubstring = ConfigurationHelper.ReadConfig(StressToolConstants.HubOwnerConectionString_ConfigName);
            var ehendpoint = ConfigurationHelper.ReadConfig(StressToolConstants.EventHubEndpoint_ConfigName);
            var sastring = ConfigurationHelper.ReadConfig(StressToolConstants.StorageAccountConectionString_ConfigName);
            var batchkey = ConfigurationHelper.ReadConfig(StressToolConstants.BatchKey_ConfigName);
            var batchurl = ConfigurationHelper.ReadConfig(StressToolConstants.BatchUrl_ConfigName);
            var batchjobid = ConfigurationHelper.ReadConfig(StressToolConstants.BatchJobId_ConfigName);
            if (!string.IsNullOrEmpty(hubstring))
            {
                HubOwnerConnectionString = hubstring;
            }
            if (!string.IsNullOrEmpty(batchjobid))
            {
                _batchJobId = batchjobid;
            }
            if (!string.IsNullOrEmpty(ehendpoint))
            {
                EventHubEndpoint = ehendpoint;
            }
            if (!string.IsNullOrEmpty(batchurl))
            {
                BatchServiceUrl = batchurl;
            }
            if (!string.IsNullOrEmpty(batchkey))
            {
                BatchAccountKey = batchkey;
            }
            if (!string.IsNullOrEmpty(sastring))
            {
                StorageAccountConnectionString = sastring;
            }
        }
        void ShowLog(object message)
        {
            LogMsg += message;
            LogMsg += "\n";
        }

        void AppendToProvider(RequirementMessage message)
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
                    RunMonitor();
                    break;
            }
            var CurrentPhaseStatus = status.Status;
        }

        void RunMonitor()
        {
            CanStartCreate = false;
            CanStartMonitor = false;
            _dataProvider.BatchKey = _batchAccountKey;
            _dataProvider.HubOwnerConectionString = _hubOwnerConnectionString;
            _dataProvider.EventHubEndpoint = _eventHubEndpoint;
            _dataProvider.BatchUrl = _batchServiceUrl;
            _dataProvider.BatchJobId = _batchJobId;
            _dataProvider.ConsumerGroupName = ConfigurationHelper.ReadConfig(StressToolConstants.ConsumerGroup_ConfigName, "$Default");
            Messenger.Default.Send<IStressDataProvider>(_dataProvider, "StartMonitor");
            new ViewModelLocator().Main.SelectedTabIndex = 2;
        }

        void ProcessRunConfigValue()
        {
            if (CanStartCreate)
            {
                StartLableVisibility = Visibility.Visible;
                CanStartCreate = false;
                CanStartMonitor = false;
                _dataProvider.BatchKey = _batchAccountKey;
                _dataProvider.HubOwnerConectionString = _hubOwnerConnectionString;
                _dataProvider.EventHubEndpoint = _eventHubEndpoint;
                _dataProvider.BatchUrl = _batchServiceUrl;
                _dataProvider.StorageAccountConectionString = _storageAccountConnectionString;
                _dataProvider.Run();
            }

        }

        void AppendBatchJobId(string batchJobId)
        {
            _batchJobId = batchJobId;
        }

        void TryActivateMonitorButton()
        {
            //hub connection string , event hub endpoint and batch job id
            //must be provided
            if (!(string.IsNullOrEmpty(_hubOwnerConnectionString) ||
              string.IsNullOrEmpty(_eventHubEndpoint)||
              string.IsNullOrEmpty(_batchJobId))&& UseExistingJobId)
            {
                CanStartMonitor = true;
            }
            else
            {
                CanStartMonitor = false;
            }
        }

        void TryGetBatchJobIds()
        {
            var result = new ObservableCollection<string>();
            try
            {
                if (!string.IsNullOrEmpty(BatchServiceUrl) &&
                    !string.IsNullOrEmpty(BatchAccountKey))
                {
                    var builder = new UriBuilder(BatchServiceUrl);
                    var username = builder.Host.Split('.').First();

                    BatchSharedKeyCredentials credentials = new BatchSharedKeyCredentials(BatchServiceUrl, username, BatchAccountKey);
                    using (BatchClient batchClient = BatchClient.Open(credentials))
                    {
                        var jobs = batchClient.JobOperations.ListJobs();
                        jobs.ForEachAsync(job =>
                        {
                            if(job.State!= JobState.Deleting)
                            {
                                result.Add(job.Id);
                            }
                        }).Wait();
                        BatchJobIds = result;
                    }
                }
            }
            catch
            {
                //silently continue
            }
        }

        void TryActivateCreateButton()
        {
            if (!(string.IsNullOrEmpty(_hubOwnerConnectionString) ||
                string.IsNullOrEmpty(_eventHubEndpoint) ||
                string.IsNullOrEmpty(_batchAccountKey) ||
                string.IsNullOrEmpty(_batchServiceUrl) ||
                string.IsNullOrEmpty(_storageAccountConnectionString) ||
                _dataProvider.MessagePerMinute == 0) && CreateNewJobId)
            {
                CanStartCreate = true;
            }
            else
            {
                CanStartCreate = false;
            }
        }
    }
}