# StressLoad
A benchmark application to measure the write-in throughput of IoT Hub

By distribute the job to virtual machines, it could be used to generate huge load

Reminder: _It will generate lots of devices to test. User need to remove those device once test completed, or directly remove the whole IoT Hub_

## Required Azure resources
1. One Batch account
1. One Storage account

Reminder: _To avoid unnecessary cost, user should remove Azure resources once test completed_

## Pre-config
Set appSettings nodes below of the configuration file StressLoad.exe.config:

* NumofVm - The desired VMs to be used in test

* DevicePerVm - The desired # of simulated device on each VM
  > The upper boudary depends on VM size. For small (A0) VM, it was suggest not to exceed 12000
  > 
  > Reminder: _Double check the IoTHub SKU and # of units to ensure the throughput will not exceed the quota. For example, a S3 unit could only handle 6000 message per second_

* MessagePerMin - The desired message rate per device
 
* DurationInMin - The test duration in minute

* DeviceIdPrefix - The prefix of device ID

* Message - The test message
  > If it is empty, the built-in message template will be used to generate message in form like: _{"DeviceData": -2,"DeviceUtcDatetime": "4/7/2017 12:05:36 PM"}_

* BatchAccountName - The batch account name

* BatchAccountKey - The batch account key
  > It could be found at Portal \Keys

* BatchServiceUrl - The URL of batch account
  > It could be found at Portal \Keys

* StorageConnectionString - the storage account used to distribute tasks

* DeviceClientEndpoint - the IoT Hub connection string

## Usage
Run StressLoad.exe to generate the traffic

First, it will generate necessary devices and open the connection. The time used for this step depends on the # of devices on each VM. A small (A0) VM could establish about 1.5K connections in one minute. In other words, 12000 devices need 8 minutes or longer to be setup.

Once all the devices were setup, it will kick-off the sending task for each device at the same time.