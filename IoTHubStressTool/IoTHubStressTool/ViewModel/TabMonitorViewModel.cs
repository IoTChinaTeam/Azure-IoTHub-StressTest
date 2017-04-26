using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Timers;
using System.Windows;
using GalaSoft.MvvmLight.Messaging;
using Microsoft.Azure.Batch;
using Microsoft.Azure.Batch.Auth;
using StressLoadDemo.Model.DataProvider;
using Microsoft.Azure.Batch.Common;
using StressLoadDemo.Model.Utility;
using GalaSoft.MvvmLight.Command;

namespace StressLoadDemo.ViewModel
{
    public class TabMonitorViewModel : ViewModelBase
    {
        const string AzureCloudAllResourcesPage ="https://ms.portal.azure.com/#blade/HubsExtension/Resources/resourceType/Microsoft.Resources%2Fresources";
        const string AzureChinaCloudAllResourcesPage ="https://portal.azure.cn/#blade/HubsExtension/Resources/resourceType/Microsoft.Resources%2Fresources";
        private const double CanvasWidth = 415;
        private const double CanvasHeight = 216;
        private string _batchJobId;
        private readonly Timer _refreshDataTimer, _refreshTaskTimer;
        private double _deviceRealTimeNumber, _messageRealTimeNumber;
        private ObservableCollection<MonitorDataLine> _deviceLines, _messageLines;
        private List<MonitorDataLine> _deviceLineBuffer, _messageLineBuffer;
        private List<double> _deviceNumberBuffer, _messageNumberBuffer;
        private IStressDataProvider _dataProvider;
        private string _selectedPartition;
        private HubReceiver _hubDataReceiver;
        private DateTime _firstDataArriveTime;
        private string _consumerGroupName;
        private string _azureAllResourceUrl;
        private bool _refreshBtnEnabled;
        private Visibility _shadeVisibility;
        private string _taskStatus;
        private int _taskActiveCount, _taskRunningCount, _taskCompletedCount, _taskTotalCount;
        private string _messageContent, _fromDevice;
        bool _txtEnabled, _comboEnabled;
        string _elapasedTime;
        string _startTime;
        private ObservableCollection<string> _partitions { get; set; }
        string _testRunTime, _throughput, _d2hAvg, _d2h1Min,_e2eDelay;
        Stopwatch localwatch;
        string _localRunTime, _timestamp;
        bool _portalBtnEnabled;

        public string TimeStamp
        {
            get{ return _timestamp; }
            set
            {
                _timestamp = value;
                RaisePropertyChanged();
            }
        }
        public TabMonitorViewModel(IStressDataProvider provider)
        {
            _consumerGroupName = "$Default";
            _selectedPartition = "0";
            _startTime = "0";
            _dataProvider = provider;
            _shadeVisibility = Visibility.Hidden;
            _partitions = new ObservableCollection<string>();
            _messageContent = "N/A";_fromDevice = "N/A";
            TestRunTime = "N/A"; Throughput = "N/A";
            DeviceToHubDelayAvg = "N/A";DeviceToHubDelay1Min = "N/A";
            _taskStatus = "N/A"; _localRunTime= "N/A";
            Messenger.Default.Register<IStressDataProvider>(
               this,
               "StartMonitor",
               ProcessMonitorConfig
               );
            _refreshDataTimer = new Timer();
            _refreshTaskTimer = new Timer();
            _refreshDataTimer.Elapsed += ObserveData;
            _refreshTaskTimer.Elapsed += ObserveTask;
            _refreshDataTimer.AutoReset = true;
            _refreshTaskTimer.AutoReset = true;
            //fetch data and refresh UI every sec
            //fetch task and refresh UI every 5 sec
            _refreshDataTimer.Interval = 1000;
            _refreshTaskTimer.Interval = 5000;
            Reload = new RelayCommand(StartCollecting);
        }


        #region UIBindingProperties

        public RelayCommand ShowAzurePortal => 
            new RelayCommand(() => 
            {
                Process.Start(_azureAllResourceUrl);
            });

        public string LocalElapsedTime
        {
            get { return _localRunTime; }
            set
            {
                _localRunTime = value;
                RaisePropertyChanged();
            }
        }

        public string BatchJobId
        {
            get { return _batchJobId; }
            set
            {
                _batchJobId = value;
                RaisePropertyChanged();
            }
        } 
        public bool PortalBtnEnabled
        {
            get { return _portalBtnEnabled; }
            set
            {
                _portalBtnEnabled = value;
                RaisePropertyChanged();
            }
        }
        public string ElapsedTime
        {   get { return _elapasedTime; }
            set
            {
                _elapasedTime = value;
                RaisePropertyChanged();
            }
        }
        public string StartTime
        {
            get { return _startTime; }
            set
            {
                _startTime = value;
                RaisePropertyChanged();
            }
        }

        public bool TxtEnabled
        {
            get { return _txtEnabled; }
            set
            {
                _txtEnabled = value;
                RaisePropertyChanged();
            }
        }

        public bool ComboEnabled
        {
            get { return _comboEnabled; }
            set
            {
                _comboEnabled = value;
                RaisePropertyChanged();
            }
        }

        public string TaskStatus
        {
            get { return _taskStatus; }
            set
            {
                _taskStatus = value;
                RaisePropertyChanged();
            }
        }

        public RelayCommand OpenBrowser { get; set; }
        public RelayCommand Reload { get; set; }

        public string MessageContent
        {
            get { return _messageContent; }
            set
            {
                _messageContent = value;
                RaisePropertyChanged();
            }
        }

        public string FromDevice
        {
            get { return _fromDevice; }
            set
            {
                _fromDevice = value;
                RaisePropertyChanged();
            }
        }

        public string TestRunTime
        {
            get { return _testRunTime; }
            set
            {
                _testRunTime = value;
                RaisePropertyChanged();
            }
        }

        public string Throughput
        {
            get { return _throughput; }
            set
            {
                _throughput = value;
                RaisePropertyChanged();
            }
        }

        public string DeviceToHubDelay1Min
        {
            get { return _d2h1Min; }
            set
            {
                _d2h1Min = value;
                RaisePropertyChanged();
            }
        }

        public string DeviceToHubDelayAvg
        {
            get { return _d2hAvg; }
            set
            {
                _d2hAvg = value;
                RaisePropertyChanged();
            }
        }

        public ObservableCollection<MonitorDataLine> MessageLines
        {
            get { return _messageLines; }
            set
            {
                _messageLines = value;
                RaisePropertyChanged();
            }
        }

        public ObservableCollection<MonitorDataLine> DeviceLines
        {
            get
            {
                return _deviceLines;
            }
            set
            {
                _deviceLines = value;
                RaisePropertyChanged();
            }
        }

        public double MessageRealTimeNumber
        {
            get { return _messageRealTimeNumber; }
            set
            {
                _messageRealTimeNumber = value;
                RaisePropertyChanged();
            }
        }

        public double DeviceRealTimeNumber
        {
            get { return _deviceRealTimeNumber; }
            set
            {
                _deviceRealTimeNumber = value;
                RaisePropertyChanged();
            }
        }

        public int TaskTotalCount
        {
            get { return _taskTotalCount; }
            set
            {
                _taskTotalCount = value;
                RaisePropertyChanged();
            }
        }

        public int TaskActiveCount
        {
            get { return _taskActiveCount; }
            set
            {
                _taskActiveCount = value;
                RaisePropertyChanged();
            }
        }

        public int TaskRunningCount
        {
            get { return _taskRunningCount; }
            set
            {
                _taskRunningCount = value;
                RaisePropertyChanged();
            }
        }

        public int TaskCompleteCount
        {
            get { return _taskCompletedCount; }
            set
            {
                _taskCompletedCount = value;
                RaisePropertyChanged();
            }
        }

        public Visibility ShadeVisibility
        {
            get { return _shadeVisibility; }
            set
            {
                _shadeVisibility = value;
                RaisePropertyChanged();
            }
        }

        public ObservableCollection<string> Partitions
        {
            get { return _partitions; }
            set
            {
                //do not refresh the combobox too fast.
                if (_partitions.Count != value.Count)
                {
                    _partitions = value;
                    RaisePropertyChanged();
                }

            }
        }

        public string ConsumerGroupName
        {
            get { return _consumerGroupName; }
            set
            {
                _consumerGroupName = value;
                RaisePropertyChanged();
                StopCollecting();
                _hubDataReceiver.SetConsumerGroup(value);
                ShadeVisibility = Visibility.Visible;
            }
        }

        public string SelectedPartition
        {
            get { return _selectedPartition; }
            set
            {
                //if value changed after the partitions are fetched from azure,
                //pause the graph for reloading.
                if (value != _selectedPartition && Partitions != null)
                {
                    StopCollecting();
                    _hubDataReceiver.SetPartitionId(int.Parse(value));
                }
                _selectedPartition = value;

                RaisePropertyChanged();
            }
        }

        public bool RefreshBtnEnabled
        {
            get { return _refreshBtnEnabled; }
            set
            {
                _refreshBtnEnabled = value;
                RaisePropertyChanged();
            }
        }
        #endregion
        void GoToPortal()
        {
            Process.Start("http://www.webpage.com");
        }

        void StopCollecting()
        {
            _refreshDataTimer.Enabled = false;
            ShadeVisibility = Visibility.Visible;
            _hubDataReceiver.PauseReceive();
            RefreshBtnEnabled = true;
        }

        void StartCollecting()
        {
            _hubDataReceiver.StartReceive();
            ShadeVisibility = Visibility.Hidden;
            RefreshBtnEnabled = false;
            _refreshTaskTimer.Enabled = true;
            _refreshDataTimer.Enabled = true;
            _firstDataArriveTime = DateTime.Now;
            _messageLineBuffer = new List<MonitorDataLine>();
            _deviceLineBuffer = new List<MonitorDataLine>();
            _messageNumberBuffer = new List<double>();
            _deviceNumberBuffer = new List<double>();
            DeviceLines = new ObservableCollection<MonitorDataLine>();
            MessageLines = new ObservableCollection<MonitorDataLine>();
            DeviceRealTimeNumber = 0; MessageRealTimeNumber = 0;           
        }

        public void ProcessMonitorConfig(IStressDataProvider provider)
        {
            _hubDataReceiver = new HubReceiver(provider);
            BatchJobId = provider.BatchJobId;
            _hubDataReceiver.StartReceive();
            localwatch = Stopwatch.StartNew();
            StartCollecting();
            IsSwitchingEnabled(true);
            EnablePortalBtn();
        }

        void EnablePortalBtn()
        {
            PortalBtnEnabled = true;
            if (_dataProvider.BatchUrl.Contains("chinacloudapi.cn"))
            {
                _azureAllResourceUrl = AzureChinaCloudAllResourcesPage;
            }
            else
            {
                _azureAllResourceUrl = AzureCloudAllResourcesPage;
            }
        }

        void ObserveTask(object sender, ElapsedEventArgs e)
        {
            //get task running status
            var builder = new UriBuilder(_dataProvider.BatchUrl);
            var BatchAccountName = builder.Host.Split('.').First();
            BatchSharedKeyCredentials credentials = new BatchSharedKeyCredentials(_dataProvider.BatchUrl, BatchAccountName, _dataProvider.BatchKey);
            using (BatchClient batchClient = BatchClient.Open(credentials))
            {
                var job = batchClient.JobOperations.GetJob(_dataProvider.BatchJobId);
                var list = job.ListTasks();
                var runningtime = (DateTime.UtcNow - job.CreationTime).ToString();
                if (!string.IsNullOrEmpty(runningtime))
                {
                    try
                    {
                        TestRunTime = runningtime.Substring(0, 11);
                    }
                    catch
                    {
                        TestRunTime = "N/A";
                    }
                }
                var totalCount = list.Count();
                var activeCount = list.Count(m => m.State == TaskState.Active || m.State == TaskState.Preparing);
                var runningCount = list.Count(m => m.State == TaskState.Running);
                var completeCount = list.Count(m => m.State == TaskState.Completed);
                _taskTotalCount = totalCount;_taskActiveCount = activeCount;
                _taskCompletedCount = completeCount;_taskRunningCount = runningCount;
                TaskStatus = $"(Total:{totalCount}  Active:{activeCount}  Running:{runningCount}  Completed:{completeCount})";
            }
        }

        void ObserveData(object sender, ElapsedEventArgs e)
        {
            //get partition counts, fill in combo box("lazy" update)
            var queryPartitionNumber = _hubDataReceiver.partitionNumber;
            if (queryPartitionNumber != Partitions.Count)
            {
                ObservableCollection<string> partitionIds = new ObservableCollection<string>();
                for (int i = 0; i < queryPartitionNumber; i++)
                {
                    partitionIds.Add(i.ToString());
                }
                Partitions = partitionIds;
            }

            //get real time number and calculate the curve
            MessageRealTimeNumber = _hubDataReceiver.totalMessage;
            DeviceRealTimeNumber = _hubDataReceiver.totalDevice;

            //collect data for generating graph
            _messageNumberBuffer.Add(_messageRealTimeNumber);
            _deviceNumberBuffer.Add(_deviceRealTimeNumber);

            var runtimestring = _hubDataReceiver.runningTime.ToString();

            var delaystringavg = _hubDataReceiver.deviceToHubDelayAvg;
            if (!string.IsNullOrEmpty(delaystringavg))
            {
                try
                {
                    DeviceToHubDelayAvg = runtimestring.Substring(0, 11);
                }
                catch
                {
                    DeviceToHubDelayAvg = "N/A";
                }
            }

            var delaystring1min = _hubDataReceiver.deviceToHubDelayOneMin;
            if (!string.IsNullOrEmpty(delaystring1min))
            {
                try
                {
                    DeviceToHubDelay1Min = delaystring1min.Substring(0, 11);
                }
                catch
                {
                    DeviceToHubDelay1Min = "N/A";
                }
            }

            MessageContent = _hubDataReceiver.sampleContent;
            Throughput = _hubDataReceiver.throughPut.ToString() + " messages/minute";
            FromDevice = _hubDataReceiver.sampleEventSender;

            if (TaskTotalCount == TaskCompleteCount && TaskTotalCount != 0)
            {
                //stop working threads
                localwatch.Stop();
                _hubDataReceiver.PauseReceive();
                _refreshTaskTimer.Enabled = false;
                _refreshDataTimer.Enabled = false;
                StartTime = "0";
                var allsec = (int)_hubDataReceiver.runningTime.TotalSeconds;
                ElapsedTime = $"{allsec/60} m {allsec%60} s";
                TransformDataToLines(_deviceNumberBuffer, ref _deviceLineBuffer);
                TransformDataToLines(_messageNumberBuffer, ref _messageLineBuffer);
            }

            else
            {
                StartTime = _deviceNumberBuffer.Count >( CanvasWidth / 2)? "...":"0";
                var allsec = (int)_hubDataReceiver.runningTime.TotalSeconds;
                ElapsedTime = $"{allsec / 60} m {allsec % 60} s";
                TransformDataToLines(
                    _deviceNumberBuffer
                    .ToList()
                    .Skip((int)Math.Max(0, _deviceNumberBuffer.Count - CanvasWidth / 2))
                    .ToList()
                    , ref _deviceLineBuffer);
                TransformDataToLines(
                    _messageNumberBuffer
                    .ToList()
                    .Skip((int)Math.Max(0, _messageNumberBuffer.Count - CanvasWidth / 2))
                    .ToList()
                    , ref _messageLineBuffer);
            }

            var datetimestring = DateTime.Now.ToString("HH:mm:ss");
            TimeStamp = $"Details(updated at {datetimestring})";
            var elapsedstring = localwatch.Elapsed.ToString();
            if (!string.IsNullOrEmpty(localwatch.Elapsed.ToString()))
            {
                try
                {
                    LocalElapsedTime = elapsedstring.Substring(0, 11);
                }
                catch
                {
                    LocalElapsedTime = "N/A";
                }
            }

            DeviceLines = new ObservableCollection<MonitorDataLine>(_deviceLineBuffer);
            MessageLines = new ObservableCollection<MonitorDataLine>(_messageLineBuffer);
        }

        void IsSwitchingEnabled(bool flag)
        {
            TxtEnabled = flag;
            ComboEnabled = flag;
        }

        void TransformDataToLines(List<double> data, ref List<MonitorDataLine> targetLines)
        {
            targetLines = new List<MonitorDataLine>();
            if (data.Count > 1)
            {
                var maxY = data.Max();
                var rangeY = maxY - data.Min();
                var scaleY = CanvasHeight / rangeY;
                if (rangeY == 0 && maxY == 0) return;

                var verticalShift = maxY > 0 ? scaleY * maxY : -scaleY * maxY;
                var xUnit = CanvasWidth / (data.Count - 1);
                double prevX = 0, prevY = verticalShift - scaleY * data[0];
                var temp = new List<MonitorDataLine>();
                data.Take(data.Count - 1).ToList().ForEach(p =>
                {
                    p = verticalShift - p * scaleY;

                    temp.Add(new MonitorDataLine() { X1 = prevX, Y1 = prevY, X2 = prevX + xUnit, Y2 = p });

                    prevX += xUnit;
                    prevY = p;
                });
                targetLines = temp;
            }
        }
    }
}
