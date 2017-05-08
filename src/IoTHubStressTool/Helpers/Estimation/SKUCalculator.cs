using System;
using StressLoadDemo.Model.AzureConstants;

namespace StressLoadDemo.Helpers
{
    public static class SkuCalculator
    {
        public const double IothubS1Speed = 278;
        public const double IothubS2Speed = 4166;
        public const double IothubS3Speed = 208333;

        public const double VmSmallCapacity = 10000;


        public static HubSku CalculateHubSku(int messagePerMinute)
        {
            HubSku sku = new HubSku();

            if (messagePerMinute < IothubS2Speed)
            {
                sku.UnitSize=HubSize.S1;
                sku.UnitCount = (int) Math.Ceiling(messagePerMinute/IothubS1Speed);
            }

            else if (messagePerMinute < IothubS3Speed)
            {
                sku.UnitSize = HubSize.S2;
                sku.UnitCount = (int) Math.Ceiling(messagePerMinute/IothubS2Speed);
            }

            else
            {
                sku.UnitSize=HubSize.S3;
                sku.UnitCount= (int)Math.Ceiling(messagePerMinute / IothubS3Speed);
            }

            return sku;
        }

        public static VmSku CalculateVmSku(int deviceCount)
        {
            var sku = new VmSku();
            sku.Size = VmSize.small;
            sku.VmCount = (int)Math.Ceiling(deviceCount / VmSmallCapacity);
            return sku;
        } 
    }
}
