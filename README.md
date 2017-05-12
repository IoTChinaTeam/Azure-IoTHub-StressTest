# Azure-IoTHub-StressTest
A GUI tool to simulate millions of IoT devices to test the capability of the Azure IoT Hub. The simulated devices are running in Azure Batch service.

## User Guidance

### 1. Set expectations for stress test
This tab helps to convert from expections, such as total device count, message rate, to the azure resources we need to run this test. 

- ![Tab1 Image](https://raw.githubusercontent.com/IoTChinaTeam/Azure-IoTHub-StressTest/master/doc/images/tab1.PNG)
### 2.1 Provision Azure resources
Here is the list of azure resources and credentials we need to run the test:

- Azure IoT Hub: owner connection string and Event Hub endpoint
- Azure Batch: service URL and access key 
- Azure Storage Account: connection string (needed for batch service)

Hint:
- You can also put credentials into app.config.
- You should create more partitions for the IoT Hub. It will give better monitor performance. 

### 3. Start running test

After you filled out all fields, the button "Start Stress Load Test" will be enabled. Click to see the progress and status.

If you see this , you can go ahead to the third tab to start monitoring stress test.
- ![Tab1 Image](https://raw.githubusercontent.com/IoTChinaTeam/Azure-IoTHub-StressTest/master/doc/images/tab2_2.PNG)

Hint:
- If the test is already running, you can monitor it by choosing an existing batch job from the drop down list.
- ![Tab1 Image](https://raw.githubusercontent.com/IoTChinaTeam/Azure-IoTHub-StressTest/master/doc/images/existing.PNG)

### 4.Monitor the test

We will start to pull message for a given partition to measure the performance. It may take a while for the connection to be established. 

Here is what the monitoring screen looks like
- ![Tab1 Image](https://raw.githubusercontent.com/IoTChinaTeam/Azure-IoTHub-StressTest/master/doc/images/tab3.PNG)

Test result section:
- Batch
  - Job Id: This is the job id on Azure Batch Service. You can use it to find the job on Azure Portal.
  - Task Status: A job consists of several tasks. This field indicates the status of all tasks that belongs to the job.
  - Job running time: It shows how long the test job has been running on cloud.

- Last Fetched Message
  - From Device: Indicates the device from which we received the latest message.
  - Content: This is the latest message fetched from IoT Hub.
  
- Details
  - Monitor running time: How long since local monitoring thread started.
  - Device-To-Hub Delay (average): The average time used for a message to reach IoT Hub (calculated from <b>all received messages</b>).
  - Device-To-Hub Delay (last minute): The average time used for a message to reach IoT Hub (calculated from <b>last one minute messages</b>).
  - Throughput for partition (last minute): The number of messages fetched <b>by monitoring thread</b>.
  - Throughput for IoT Hub (last minute): Estimated total throughput.
