using DocumentFormat.OpenXml.Wordprocessing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Toolbox_Class_Library.CtrUpdate;

namespace Toolbox_Class_Library.CtrUpdate
{
    public class CTR
    {
        public string Name { get; set; }
        public List<Device> DeviceList { get; set; }

        public CTR(string name, List<string> devices, string AltNames)
        {
            Name = name;
            DeviceList = GenerateDeviceList(devices, AltNames);
        }
        public List<Device> GenerateDeviceList(List<string> devices, string AltNames)
        {
            List<Device> ReturnList = new();
            string[] joinedDevices = AltNames.Split(", ");
            List<string[]> AssociatedDevices = [];

            // Parse associated devices
            foreach (string AssociatedDevice in joinedDevices)
            {
                string[] temp = AssociatedDevice.Split("+");
                AssociatedDevices.Add(temp);
            }

            HashSet<string> processedModels = new(); // Track processed models

            foreach (string model in devices)
            {
                // Check if the model is part of an associated device group
                string[]? matchingGroup = AssociatedDevices.FirstOrDefault(group => group.Contains(model));

                if (matchingGroup != null)
                {
                    // If none of the group has been processed, add it as a new Device
                    if (!processedModels.Overlaps(matchingGroup))
                    {
                        ReturnList.Add(new Device(matchingGroup[0], matchingGroup.ToList(), 0)); // 0 as a placeholder for counter
                        processedModels.UnionWith(matchingGroup); // Mark all in the group as processed
                    }
                }
                else if (!processedModels.Contains(model))
                {
                    // If the model isn't in an associated group, add it individually with only itself as an alternative name
                    ReturnList.Add(new Device(model, new List<string> { model }, 0)); // 0 as a placeholder for counter
                    processedModels.Add(model);
                }
            }

            return ReturnList;
        }
        public void DevicePlusCounter(string DeviceName)
        {
            // Find the device in the list if its there.
            Device? device = DeviceList.FirstOrDefault(d => d.Name == DeviceName || d.AlternativeName.Contains(DeviceName));

            if (device != null)
            {
                device.Counter++; // add to its count.
            }
        }
        public override string ToString()
        {
            return (string.Join("\n", this.DeviceList.Select(d => d.Counter)));
        }

        public void FormatDeviceTitles()
        {
            // Probably on the safer side to make this in settings but I will build a version now.
            foreach (Device dev in DeviceList)
            {
                if (dev.Name == "CGM4331COM")
                {
                    dev.Name = "XB7";
                }
            }
        }
        public string DevicesToPaste()
        {
            return string.Join("\n", this.DeviceList.Select(d => $"{d.Counter}"));
        }



    }
}
