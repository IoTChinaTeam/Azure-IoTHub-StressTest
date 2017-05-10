using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using LiveCharts;
using LiveCharts.Wpf;
using Microsoft.Azure.Batch;
using Microsoft.Azure.Batch.Auth;
using Microsoft.Azure.Batch.Common;
using Microsoft.WindowsAzure.Storage;
using StressLoadDemo.Helpers.Configuration;
using StressLoadDemo.Model.DataProvider;
using StressLoadDemo.Model.Utility;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Timers;
using System.Windows;

namespace StressLoadDemo.ViewModel
{
    public class TabMonitorViewModel : ViewModelBase
    {
        const string ContainerName = "stresstest";
        const string AzureCloudAllResourcesPage = "https://ms.portal.azure.com/#blade/HubsExtension/Resources/resourceType/Microsoft.Resources%2Fresources";
        const string AzureChinaCloudAllResourcesPage = "https://portal.azure.cn/#blade/HubsExtension/Resources/resourceType/Microsoft.Resources%2Fresources";

        private IStressDataProvider _dataProvider;
        private HubReceiver _hubDataReceiver;
        bool _portalBtnEnabled;

        //timer to control refresh frequency
        private readonly Timer _refreshDataTimer, _refreshTaskTimer;

        //graph-related
        private double _deviceRealTimeNumber, _messageRealTimeNumber;

        //partition-specification
        private string _selectedPartition, _consumerGroupName;
        private bool _refreshBtnEnabled;
        private Visibility _shadeVisibility;
        bool _txtEnabled, _comboEnabled;

        //monitor data
        Stopwatch localwatch;
        string _localRunTime, _timestamp;
        private string _azureAllResourceUrl;
        private int _taskActiveCount, _taskRunningCount, _taskCompletedCount, _taskTotalCount;
        private string _messageContent, _fromDevice, _taskStatus;
        private string _batchJobId, _elapasedTime, _startTime, _testRunTime, _throughput, _d2hAvg, _d2h1Min, _hubthroughput, _partitioncount;
        private bool _deleteFlag;

        public TabMonitorViewModel(IStressDataProvider provider)
        {
            _dataProvider = provider;
            _consumerGroupName  = ConfigurationHelper.ReadConfig(StressToolConstants.ConsumerGroup_ConfigName, "$Default");
            InitBindingData();
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
           
            InitChart();
        }

        #region UIBindingPropertiesAndCommands

        public RelayCommand ShowAzurePortal =>
           new RelayCommand(() =>
           {
               Process.Start(_azureAllResourceUrl);
           });

        private ObservableCollection<string> _partitions { get; set; }

        public string PartitionCount
        {
            get { return _partitioncount; }
            set
            {
                _partitioncount = value;
                RaisePropertyChanged();
            }
        }

        public string HubThroughput
        {
            get { return _hubthroughput; }
            set
            {
                _hubthroughput = value;
                RaisePropertyChanged();
            }
        }
        public string TimeStamp
        {
            get { return _timestamp; }
            set
            {
                _timestamp = value;
                RaisePropertyChanged();
            }
        }


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
        public string CurrentTimeString
        {
            get { return _elapasedTime; }
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


        // live chart binding properties
        public SeriesCollection DeviceSeriesCollection { get; set; }

        public SeriesCollection MessageSeriesCollection { get; set; }
        public List<string> Labels { get; set; }
        public Queue<DateTime> TimeStamps { get; set; }
        public Func<double, string> YFormatter { get; set; }

        public Func<DateTime,string> XFormatter { get; set; }

        void InitBindingData()
        {
            _selectedPartition = "0";
            _startTime = "0";
            _shadeVisibility = Visibility.Hidden;
            _partitions = new ObservableCollection<string>();
            _messageContent = "N/A"; _fromDevice = "N/A";
            TestRunTime = "N/A"; Throughput = "N/A";
            DeviceToHubDelayAvg = "N/A"; DeviceToHubDelay1Min = "N/A";
            _taskStatus = "N/A"; _localRunTime = "N/A";
            BatchJobId = string.Empty;
            Reload = new RelayCommand(StartCollecting);
            Labels = new List<string>();
            TimeStamps = new Queue<DateTime>();
            _deleteFlag = false;
        }
        void InitChart()
        {
            MessageSeriesCollection = new SeriesCollection
            {
                new LineSeries
               {
                    Title = "",
                    Values = new ChartValues<double> (),
                    PointGeometry = DefaultGeometries.None

                    //PointGeometrySize = 10

                }
            };
            DeviceSeriesCollection = new SeriesCollection
            {
                new LineSeries
               {
                    Title = "DeviceNumber",
                    Values = new ChartValues<double> (),
                    PointGeometry = DefaultGeometries.None
                    //PointGeometrySize = 10
                }
            };
            YFormatter = value => Math.Round(value).ToString();

        }

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
            MessageSeriesCollection[0].Values = new ChartValues<double>();
            DeviceSeriesCollection[0].Values = new ChartValues<double>();
            DeviceRealTimeNumber = 0; MessageRealTimeNumber = 0;
            TaskTotalCount = 0; TaskCompleteCount = 0;
        }

        void ProcessMonitorConfig(IStressDataProvider provider)
        {
            InitBindingData();
            _hubDataReceiver = new HubReceiver(provider);
            _hubDataReceiver.SetConsumerGroup(_consumerGroupName);
            BatchJobId = provider.BatchJobId;
            _hubDataReceiver.StartReceive();
            localwatch = Stopwatch.StartNew();
            StartCollecting();
            Messenger.Default.Send($"Monitoring started, please switch to monitor tab", "RunningLog");
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
            //Connect to batch to get task status.
            //todo: job status sometimes overlaps.
            var builder = new UriBuilder(_dataProvider.BatchUrl);
            var BatchAccountName = builder.Host.Split('.').First();
            BatchSharedKeyCredentials credentials = new BatchSharedKeyCredentials(_dataProvider.BatchUrl, BatchAccountName, _dataProvider.BatchKey);
            using (BatchClient batchClient = BatchClient.Open(credentials))
            {
                var job = batchClient.JobOperations.GetJob(BatchJobId);
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
                var activeCount = list.Count(m => m.State == TaskState.Active);
                var runningCount = list.Count(m => m.State == TaskState.Running || m.State == TaskState.Preparing);
                var completeCount = list.Count(m => m.State == TaskState.Completed);
                _taskTotalCount = totalCount; _taskActiveCount = activeCount;
                _taskCompletedCount = completeCount; _taskRunningCount = runningCount;
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
            PartitionCount = Partitions.Count.ToString();

            //get real time number to mark on the curve
            MessageRealTimeNumber = _hubDataReceiver.totalMessage;
            DeviceRealTimeNumber = _hubDataReceiver.totalDevice;

            //update properties for monitoring data
            var delaystringavg = _hubDataReceiver.deviceToHubDelayAvg;
            if (!string.IsNullOrEmpty(delaystringavg))
            {
                try
                {
                    DeviceToHubDelayAvg = delaystringavg.Substring(0, 11);
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
            Throughput = (_hubDataReceiver.throughput).ToString() + " messages/minute";
            HubThroughput = $"≈ {_hubDataReceiver.throughput * Partitions.Count}  messages/minute";
            FromDevice = _hubDataReceiver.sampleEventSender;
            var allsec = (int)_hubDataReceiver.runningTime.TotalSeconds;
            var datetimestring = DateTime.Now.ToString("HH:mm:ss");
            CurrentTimeString = datetimestring;
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

            //Update chart
            var currentTime = DateTime.Now;
            TimeStamps.Enqueue(currentTime);
            Labels.Add(currentTime.ToString("hh:mm:ss"));
            DeviceSeriesCollection[0].Values.Add(_deviceRealTimeNumber);
            MessageSeriesCollection[0].Values.Add(_messageRealTimeNumber);
            var earliestTime = currentTime;
            if (TimeStamps.Count != 0)
            {
                earliestTime = TimeStamps.Peek();
            }
            //only collect recent 15min data.
            var timeDiff = currentTime.Subtract(earliestTime);
            if(timeDiff>new TimeSpan(0, 15, 1))
            {
                Labels.RemoveAt(0);
                DeviceSeriesCollection[0].Values.RemoveAt(0);
                MessageSeriesCollection[0].Values.RemoveAt(0);
                TimeStamps.Dequeue();
            }
            

            //check job status and stop refreshing.
            if (TaskTotalCount == TaskCompleteCount && TaskTotalCount != 0)
            {
                //stop working threads
                localwatch.Stop();
                _hubDataReceiver.PauseReceive();
                _refreshTaskTimer.Enabled = false;
                _refreshDataTimer.Enabled = false;
                if (!_deleteFlag)
                {//avoid "Conflict"
                    CleanUpBatch();
                    CleanUpStorage();
                    _deleteFlag = true;
                }


            }
        }

        async void CleanUpStorage()
        {
            try { 
            var sa = CloudStorageAccount.Parse(_dataProvider.StorageAccountConectionString);
            var cloudBlobClient = sa.CreateCloudBlobClient();
            var containers = cloudBlobClient.ListContainers(ContainerName);

            foreach (var c in containers)
            {
                await c.DeleteIfExistsAsync();
            }
            }
            catch
            {
                //silently continue
            }
        }

        async void CleanUpBatch()
        {
            var builder = new UriBuilder(_dataProvider.BatchUrl);
            var username = builder.Host.Split('.').First();

            BatchSharedKeyCredentials credentials = new BatchSharedKeyCredentials(_dataProvider.BatchUrl, username, _dataProvider.BatchKey);
            using (BatchClient batchClient = BatchClient.Open(credentials))
            {
                await batchClient.JobOperations.DeleteJobAsync(BatchJobId);
            }
        }

        void IsSwitchingEnabled(bool flag)
        {
            TxtEnabled = flag;
            ComboEnabled = flag;
        }
    }
}
