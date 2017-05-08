# Azure-IoTHub-StressTest
Azure IoT Hub Stress test tool using azure batch service

## IoTHubStressTool
A GUI tool to simulate millions of IoT devices to test the capability of the Azure IoT Hub.

### User Guidance

#### 1. Set expectations for stress test
This tab helps to convert from expections, such as total device count, message rate, to the azure resources we need to run this test. 

- ![Tab1 Image](https://raw.githubusercontent.com/IoTChinaTeam/Azure-IoTHub-StressTest/doc/images/tab1.PNG)
#### 2. Provision or assign Azure resources
We need to use Azure Batch service to simulate the device (which need a storage account too) and azure IoT Hub to handle the devices.

- IoT Hub owner connection string and Event Hub endpoint of the same IoT Hub
- Azure Batch Service URL and access key
- Azure Storage Account connection string (needed for batch service)

If you are familiar with Application Configuration files(app.config), you can fill in those values in config file, and they will be filled in automatically.

#### 3.Create and start job in Batch Service

When you have filled out all fields, the button "Start Stress Load Test" will be enabled.
Click to see the progress and status.

If you see this , you can go ahead to the third tab to start monitoring stress test.
- ![Tab1 Image](https://raw.githubusercontent.com/IoTChinaTeam/Azure-IoTHub-StressTest/doc/images/tab2_2.PNG)

In addition,  you can start monitoring an existing batch job, by providing IoT Hub Credentials, Batch Credentials, and select an existing batch job id from the drop down list.
- ![Tab1 Image](https://raw.githubusercontent.com/IoTChinaTeam/Azure-IoTHub-StressTest/doc/images/existing.PNG)

#### 4.Monitor the test

Note that , depending on the number of device, you have to wait for a while to see the data and graph.
Here is what the monitoring screen looks like
- ![Tab1 Image](https://raw.githubusercontent.com/IoTChinaTeam/Azure-IoTHub-StressTest/doc/images/tab3.PNG)

The graph and the data will stop refreshing once the job is finished on Batch Service, and a full view of the entire curve will be displayed.
- ![Tab1 Image](https://raw.githubusercontent.com/IoTChinaTeam/Azure-IoTHub-StressTest/doc/images/finish.PNG)

#### 5.Monitor Values

- Batch
  - Job Id: This is the job id on Azure Batch Service. You can use it to find the job on Azure Portal.
  - Task Status: A job consists of several tasks. This field indicates the status of all tasks that belongs to the job.
  - Job running time: This is the duration since job creation time till now, and shows how long the test has been running on cloud.

- Last Fetched Message
  - Content: This is the latest message fetched by the tool from Event Hub.
  - From Device: Indicates which device the latest received message is from.

- Details
  - Monitor running time:The duration since local monitoring thread becomes enabled till now.
  - Device-To-Hub Delay (average): The average time used for a device message to reach IoT Hub (calculated from <b>all data</b>).
  - Device-To-Hub Delay (last minute): The average time used for a device message to reach IoT Hub (calculated from <b>last one minute data</b>).
  - Throughput for partition (last minute): The number of messages fetched <b>by monitoring thread</b>.
