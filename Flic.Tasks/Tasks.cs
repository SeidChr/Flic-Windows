namespace Flic.Tasks
{
    using System;
    using System.Diagnostics;

    using Windows.ApplicationModel.Background;
    using Windows.Devices.Bluetooth.Advertisement;
    using Windows.Devices.Bluetooth.Background;
    using Windows.Security.Cryptography;
    using Windows.System;

    public sealed class Tasks : IBackgroundTask
    {
        public void Run(IBackgroundTaskInstance taskInstance)
        {
            var details = taskInstance.TriggerDetails as BluetoothLEAdvertisementWatcherTriggerDetails;
            if (details == null)
            {
                return;
            }

            var advertisements = details.Advertisements;
            foreach (var args in advertisements)
            {
                if (!args.Advertisement.Flags.HasValue || args.Advertisement.Flags.Value == (BluetoothLEAdvertisementFlags)0x6)
                {
                    continue;
                }

                Debug.WriteLine("-- Beacon ----------");

                Debug.WriteLine(Enum.GetName(typeof(BluetoothLEAdvertisementType), args.AdvertisementType));
                Debug.WriteLine("BluetoothAddress: {0:X}", args.BluetoothAddress);
                Debug.WriteLine("Raw Signal Strength In DBm: {0}", args.RawSignalStrengthInDBm);
                Debug.WriteLine("Timestamp: {0}", args.Timestamp);

                Debug.WriteLine("Local Name: " + args.Advertisement.LocalName);
                Debug.WriteLine("Service Uuids:");

                var serviceUuids = args.Advertisement.ServiceUuids;
                foreach (var serviceUuid in serviceUuids)
                {
                    Debug.WriteLine("\t{0}", serviceUuid);
                }

                Debug.WriteLine("Flags:");
                if (args.Advertisement.Flags == null)
                {
                    Debug.WriteLine("\tnull");

                }
                else
                {
                    Debug.WriteLine("\t(0x{0:X}) {1}", (uint)args.Advertisement.Flags.Value, Enum.Format(typeof(BluetoothLEAdvertisementFlags), args.Advertisement.Flags, "g"));
                }

                Debug.WriteLine("Manufacturer Data:");
                var manufacturerDatas = args.Advertisement.ManufacturerData;
                foreach (var manufacturerData in manufacturerDatas)
                {
                    Debug.WriteLine("\tCompany Id {0:X}", manufacturerData.CompanyId);
                    Debug.WriteLine("\tData:");
                    var data = manufacturerData.Data;
                    Debug.WriteLine("\t" + CryptographicBuffer.EncodeToHexString(data));
                }

                Debug.WriteLine("Data Sections:");
                var dataSections = args.Advertisement.DataSections;
                foreach (var dataSection in dataSections)
                {
                    Debug.WriteLine("\tDate Type {0:X}", dataSection.DataType);
                    Debug.WriteLine("\tData:");
                    var data = dataSection.Data;
                    Debug.WriteLine("\t" + CryptographicBuffer.EncodeToHexString(data));
                }
            }
        }
    }
}

