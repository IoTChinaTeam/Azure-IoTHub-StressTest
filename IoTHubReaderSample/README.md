# IoTHubReaderSample
A benchmark application to measure the read-out throughput of a single IoT Hub partition

It will periodically output the total messages received, total devices, average and last minute message throughput, delays and sample message


## Pre-config
Set appSettings nodes below of the configuration file IoTHubReaderSample.exe.config:

* ConnectionString - the connection string of the target IoT Hub

* EventHubEndpoint - the Event Hub-compatible endpoint of the target IoT Hub

* PartitionId - the interesting partition ID
  > Default: "0"

* GroupName - the consumer group name
  > Default: "$Default"

* Offset - the offset of desired start position for reading, in minutes
  > Default: 60


## Usage
Run IoTHubReaderSample.exe to read messages