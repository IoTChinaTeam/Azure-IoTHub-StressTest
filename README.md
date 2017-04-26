# Azure-IoTHub-StressTest
Azure IoT Hub Stress test tool using azure batch service
## IoTHubStressTool
A GUI tool to create and monitor Azure IoT Hub stress capabilities.

### User Guidance
#### 1. Input expectations for stress test
for every combination of expectation, recommendation for azure resources will be given.

![Tab1 Image](https://raw.githubusercontent.com/IoTChinaTeam/Azure-IoTHub-StressTest/YihuiWpf/ScreenShots/tab1.PNG)
#### 2.Assign Azure resources the test is going to use.
below credentials need to be specified in the second tab page.

![Tab1 Image](https://raw.githubusercontent.com/IoTChinaTeam/Azure-IoTHub-StressTest/YihuiWpf/ScreenShots/tab2.PNG)

##### a.IoT Hub owner connection string and Event Hub endpoint of the same IoT Hub
You can find them here in azure portal

![Tab1 Image](https://raw.githubusercontent.com/IoTChinaTeam/Azure-IoTHub-StressTest/YihuiWpf/ScreenShots/iothubownerstr.PNG)
![Tab1 Image](https://raw.githubusercontent.com/IoTChinaTeam/Azure-IoTHub-StressTest/YihuiWpf/ScreenShots/eventhubendpoint.PNG)

##### b.Azure Batch Service URL and access key
You can find them here in azure portal

![Tab1 Image](https://raw.githubusercontent.com/IoTChinaTeam/Azure-IoTHub-StressTest/YihuiWpf/ScreenShots/batchurl.PNG)
![Tab1 Image](https://raw.githubusercontent.com/IoTChinaTeam/Azure-IoTHub-StressTest/YihuiWpf/ScreenShots/batchkey.PNG)

##### c.Azure Storage Account connection string.
You can find them here in azure portal

![Tab1 Image](https://raw.githubusercontent.com/IoTChinaTeam/Azure-IoTHub-StressTest/YihuiWpf/ScreenShots/storageaccountstring.PNG)

#### 3.Create and start job in Batch Service

When you have filled out all fields, the button "Start Stress Load Test" will be enabled.
Click to see the progress and status.

If you see this , you can go ahead to the third tab to start monitoring stress test.
![Tab1 Image](https://raw.githubusercontent.com/IoTChinaTeam/Azure-IoTHub-StressTest/YihuiWpf/ScreenShots/tab2_2.PNG)

#### 4.Monitor the test

Note that , depending on the number of device, you have to wait for a while to see the data and graph.
Here is what the monitoring screen looks like
![Tab1 Image](https://raw.githubusercontent.com/IoTChinaTeam/Azure-IoTHub-StressTest/YihuiWpf/ScreenShots/tab3.PNG)

The graph and the data will stop refreshing once the job is finished on Batch Service, and a full view of the entire curve will be displayed.
![Tab1 Image](https://raw.githubusercontent.com/IoTChinaTeam/Azure-IoTHub-StressTest/YihuiWpf/ScreenShots/finish.PNG)

#### 5.Monitor Values

##### a. Batch- Job Id

This is the job id on Azure Batch Service. You can use it to find the job on Azure Portal.

##### b. Batch- Task Status

A job consists of several tasks. The job will not complete unless all task are completed.
This field indicates the status of all tasks that belongs to the job.

##### c. Batch- Job running time

This is the duration since job creation time till now, and shows how long the test has been running on cloud.

##### d. Last Fetched Message- Content

This is the latest message fetched by the tool from Event Hub.

##### e. Last Fetched Message- From Device

Indicates which device the latest received message is from.

##### f. Details- Monitor running time

The duration since local monitoring thread becomes enabled till now.

##### g. Details- Device-To-Hub Delay (average)

The average time used for a device message to reach IoT Hub (calculated from <b>all data</b>).

##### h. Details- Device-To-Hub Delay (last minute)

The average time used for a device message to reach IoT Hub (calculated from <b>last one minute data</b>).

##### i. Details- Throughput for partition (last minute)

The number of messages fetched <b>by monitoring thread</b>.
