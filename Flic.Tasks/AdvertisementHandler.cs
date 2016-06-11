using System;
using System.Diagnostics;

using Windows.ApplicationModel.Background;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.Advertisement;
using Windows.Devices.Bluetooth.Background;
using Windows.Devices.Enumeration;
using Windows.Graphics.Printing;
using Windows.Media.SpeechRecognition;
using Windows.Security.Cryptography;

namespace Flic.Tasks
{
    public sealed class AdvertisementHandler : IBackgroundTask
    {
        public void Run(IBackgroundTaskInstance taskInstance)
        {
            var details = taskInstance.TriggerDetails as BluetoothLEAdvertisementWatcherTriggerDetails;
            if (details == null)
            {
                return;
            }

            var advertisements = details.Advertisements;
            foreach (var advertisement in advertisements)
            {
                Print(advertisement);
            }
        }

        private static async void Print(BluetoothLEAdvertisementReceivedEventArgs advertisementEvent)
        {
            Debug.WriteLine("-- Beacon ----------");
            Debug.WriteLine(Enum.GetName(typeof(BluetoothLEAdvertisementType), advertisementEvent.AdvertisementType));
            Debug.WriteLine("BluetoothAddress: {0:X}", advertisementEvent.BluetoothAddress);
            Debug.WriteLine("Raw Signal Strength In DBm: {0}", advertisementEvent.RawSignalStrengthInDBm);
            Debug.WriteLine("Timestamp: {0}", advertisementEvent.Timestamp);

            
            Print(advertisementEvent.Advertisement);

            try
            {
                var device = await BluetoothLEDevice.FromBluetoothAddressAsync(advertisementEvent.BluetoothAddress);
                Print(device);
            }
            catch
            {
                Debug.WriteLine("Failure getting device from Bluetooth Address.");
            }
        }

        private static async void PrintService(Guid serviceGuid)
        {
            try
            {
                var device = await BluetoothLEDevice.FromIdAsync(serviceGuid.ToString());
                Print(device);
            }
            catch
            {
                Debug.WriteLine("Failure getting device from service id.");
            }

        }

        private static void Print(BluetoothLEDevice device)
        {
            var services = device.GattServices;
            Debug.WriteLine("\t Services:");
            foreach (var service in services)
            {
                Debug.WriteLine($"\t Service {service.Uuid}:");
                var characteristics = service.GetAllCharacteristics();
                Debug.WriteLine("\t Characteristics:");
                foreach (var characteristic in characteristics)
                {
                    Debug.WriteLine($"\t\t Char. Description:{characteristic.UserDescription}");
                }
            }
        }

        private static void Print(BluetoothLEAdvertisement advertisement)
        {
            Debug.WriteLine("Local Name: " + advertisement.LocalName);
            Debug.WriteLine("Service Uuids:");

            var serviceUuids = advertisement.ServiceUuids;
            foreach (var serviceUuid in serviceUuids)
            {
                Debug.WriteLine($"\t{serviceUuid}");
                PrintService(serviceUuid);
            }



            Debug.WriteLine("Flags:");
            Debug.WriteLine(advertisement.Flags == null
                ? "\tnull"
                : $"\t(0x{(uint) advertisement.Flags.Value:X}) {Enum.Format(typeof (BluetoothLEAdvertisementFlags), advertisement.Flags, "g")}");

            Debug.WriteLine("Manufacturer Data:");
            var manufacturerDatas = advertisement.ManufacturerData;
            foreach (var manufacturerData in manufacturerDatas)
            {
                Debug.WriteLine($"\tCompany Id {manufacturerData.CompanyId:X}");
                Debug.WriteLine("\tData:");
                var data = manufacturerData.Data;
                Debug.WriteLine("\t" + CryptographicBuffer.EncodeToHexString(data));
            }

            Debug.WriteLine("Data Sections:");
            var dataSections = advertisement.DataSections;
            foreach (var dataSection in dataSections)
            {
                Debug.WriteLine($"\tDate Type {dataSection.DataType:X}");
                Debug.WriteLine("\tData:");
                var data = dataSection.Data;
                Debug.WriteLine("\t" + CryptographicBuffer.EncodeToHexString(data));
            }
        }
    }
}

