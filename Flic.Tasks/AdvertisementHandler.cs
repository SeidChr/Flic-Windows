using System;
using System.Collections;
using System.Diagnostics;
using System.Linq;
using Windows.ApplicationModel.Background;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.Advertisement;
using Windows.Devices.Bluetooth.Background;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
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

        private static async void Print(BluetoothLEAdvertisementReceivedEventArgs advertisementEvent, int indent = 0)
        {
            DebugLine("-- Beacon ----------", indent);
            DebugLine(Enum.GetName(typeof(BluetoothLEAdvertisementType), advertisementEvent.AdvertisementType), indent);
            DebugLine($"BluetoothAddress: {advertisementEvent.BluetoothAddress:X}", indent);
            DebugLine($"Raw Signal Strength In DBm: {advertisementEvent.RawSignalStrengthInDBm}", indent);
            DebugLine($"Timestamp: {advertisementEvent.Timestamp}", indent);


            Print(advertisementEvent.Advertisement, indent + 1);

            try
            {
                var device = await BluetoothLEDevice.FromBluetoothAddressAsync(advertisementEvent.BluetoothAddress);
                Print(device, indent + 1);
            }
            catch
            {
                DebugLine("Failure getting device from Bluetooth Address.", indent);
            }
        }

        private static async void PrintService(Guid serviceGuid, int indent)
        {
            try
            {
                DebugLine("BT LE SERVICE LOOKUP", indent);
                var device = await BluetoothLEDevice.FromIdAsync(serviceGuid.ToString());
                Print(device, indent + 1);
            }
            catch
            {
                DebugLine("Failure getting device from service id.", indent);
            }

            try
            {
                DebugLine("DEVICE INFORMATION GATT SERVICE LOOKUP", indent);
                var devices = await DeviceInformation.FindAllAsync(GattDeviceService.GetDeviceSelectorFromUuid(serviceGuid));
                DebugLine($"Found Devices: {devices.Count}", indent);
                foreach (var device in devices)
                {
                    Print(device, indent + 1);
                }
            }
            catch
            {
                DebugLine("Failure getting device from service id.", indent);
            }

        }

        private static void Print(BluetoothLEDevice device, int indent)
        {
            var services = device.GattServices;
            DebugLine(" Services:", indent);
            foreach (var service in services)
            {
                DebugLine($"Service {service.Uuid}:", indent+1);
                var characteristics = service.GetAllCharacteristics();
                DebugLine("Characteristics:", indent+1);
                foreach (var characteristic in characteristics)
                {
                    DebugLine($"Char. Description:{characteristic.UserDescription}", indent + 2);
                }
            }
        }

        private static async void Print(DeviceInformation device, int indent)
        {
            var service = await GattDeviceService.FromIdAsync(device.Id);

            if (service != null)
            {
                DebugLine($"Service {service.Uuid}:", indent);
                var characteristics = service.GetAllCharacteristics();
                DebugLine("Characteristics:", indent);
                foreach (var characteristic in characteristics)
                {
                    DebugLine($" Char. Description:{characteristic.UserDescription}", indent + 1);
                }
            }
            else
            {
                DebugLine("Access to device is denied. App not granted or in use by other application.", indent);
            }
        }

        private static void Print(BluetoothLEAdvertisement advertisement, int indent)
        {
            DebugLine("Local Name: " + advertisement.LocalName, indent);
            DebugLine("Service Uuids:", indent);

            var serviceUuids = advertisement.ServiceUuids;
            foreach (var serviceUuid in serviceUuids)
            {
                DebugLine($"{serviceUuid}", indent + 1);
                PrintService(serviceUuid, indent + 2);
            }

            DebugLine("Flags:", indent);
            DebugLine(advertisement.Flags == null
                ? "null"
                : $"(0x{(uint)advertisement.Flags.Value:X}) {Enum.Format(typeof(BluetoothLEAdvertisementFlags), advertisement.Flags, "g")}",
                indent);

            DebugLine("Manufacturer Data:", indent);
            var manufacturerDatas = advertisement.ManufacturerData;
            foreach (var manufacturerData in manufacturerDatas)
            {
                DebugLine($"Company Id {manufacturerData.CompanyId:X}", indent + 1);
                DebugLine("Data:", indent + 1);
                var data = manufacturerData.Data;
                DebugLine(CryptographicBuffer.EncodeToHexString(data), indent + 1);
            }

            DebugLine("Data Sections:", indent);
            var dataSections = advertisement.DataSections;
            foreach (var dataSection in dataSections)
            {
                DebugLine($"Date Type {dataSection.DataType:X}", indent + 1);
                DebugLine("Data:", indent + 1);
                var data = dataSection.Data;
                DebugLine(CryptographicBuffer.EncodeToHexString(data), indent + 1);
            }
        }

        private static void DebugLine(string message, int indent)
        {
            Debug.WriteLine(string.Concat(Enumerable.Repeat("    ", indent)) + message);
        }
    }
}

