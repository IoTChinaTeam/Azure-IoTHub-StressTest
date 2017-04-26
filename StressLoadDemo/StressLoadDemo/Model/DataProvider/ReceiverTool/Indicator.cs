using Microsoft.ServiceBus.Messaging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace StressLoadDemo.Model.DataProvider.ReceiverTool
{
    class Indicators
    {
        private const int deviceToIoTHubDelayWeight = 15;
        private const int e2eDelayWeight = 15;

        public int TotalDevices
        {
            get
            {
                lock (devices)
                {
                    return devices.Count;
                }
            }
        }

        public int TotalMessages { get; private set; }
        public TimeSeriesContianer DeviceToIoTHubDelay { get; private set; } = new TimeSeriesContianer(TimeSpan.FromMinutes(1), 15);
        public TimeSeriesContianer E2EDelay { get; private set; } = new TimeSeriesContianer(TimeSpan.FromMinutes(1), 15);
        public string SampleEventSender { get; private set; }
        public string SampleEvent { get; private set; }

        private HashSet<string> devices = new HashSet<string>();

        public void Reset()
        {
            devices.Clear();
            TotalMessages = 0;
            DeviceToIoTHubDelay.Reset();
            E2EDelay.Reset();
            SampleEventSender = string.Empty;
            SampleEvent = string.Empty;
        }

        public void Push(EventData eventData)
        {
            var receiveTimeUtc = DateTime.UtcNow;
            TotalMessages++;

            var deviceId = eventData.SystemProperties["iothub-connection-device-id"].ToString();
            if (!devices.Contains(deviceId))
            {
                lock (devices)
                {
                    devices.Add(deviceId);
                }
            }

            var bytes = eventData.GetBytes();
            var content = Encoding.UTF8.GetString(bytes);
            SampleEventSender = deviceId;
            SampleEvent = content;

            var root = JsonConvert.DeserializeObject(content) as JToken;
            if (root != null)
            {
                var sendTimeUtc = root.Value<DateTime>("DeviceUtcDatetime");
                if (sendTimeUtc > DateTime.MinValue)
                {
                    var enqueueTimeUtc = eventData.EnqueuedTimeUtc;

                    var deviceToIoTHubDelay = (enqueueTimeUtc - sendTimeUtc).TotalMilliseconds;
                    var e2eDelay = (receiveTimeUtc - sendTimeUtc).TotalMilliseconds;

                    DeviceToIoTHubDelay.Push(receiveTimeUtc, deviceToIoTHubDelay);
                    E2EDelay.Push(receiveTimeUtc, e2eDelay);
                }
            }
        }
    }
}
