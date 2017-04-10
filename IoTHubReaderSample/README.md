# IoTHubReaderSample
A benchmark application to measure the read-out throughput of a single IoT Hub partition

It will periodically output the total messages received, total devices, average and last minute message throughput, delays and sample message


## Pre-config
Set appSettings nodes below of the configuration file IoTHubReaderSample.exe.config:

* ConnectionString - the Event Hub-compatible connection string in form like: "Endpoint=<_endpoint_>;SharedAccessKeyName=<_policy name_>;SharedAccessKey=<_key_>"
  > The endpoint could be found at Portal \Endpoints\Events\Event Hub-compatible endpoint
  > 
  > The policy name and key could be found at Portal \Shared access policies  

* Path - the Event Hub-compatible entity path
  > It could be found at Portal \Endpoints\Events\Event Hub-compatible name

* PartitionId - the interesting partition ID
  > Default: "0"

* GroupName - the consumer group name
  > Default: "$Default"

* Offset - the offset of desired start position for reading, in minutes
  > Default: 60

* DeviceID - the interesting device ID
  > Default: null

* AsyncEventProcess - true means use the _Async_ style for event process
  > Default: false


## Usage
Run IoTHubReaderSample.exe to read messages