using DocumentFormat.OpenXml.Wordprocessing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Toolbox_Class_Library.CtrUpdate;
using Toolbox_Class_Library.Properties;
using Settings = Toolbox_Class_Library.Properties.Settings;

namespace Toolbox_Class_Library
{
    public class Tech
    {
        public string Name { get; set; }
        public List<Device> Devices { get; set; }
        public Tech(string name)
        {
            Name = name;
            Devices = GetTechDevices();
            
        }
        List<Device> GetTechDevices()
        {
            List<Device> returnList = new();
            string devicesString = Settings.Default.TechDevices;
            string altNamesString = Settings.Default.GroupedDevices;
            string[] deviceList = devicesString.Split(", ");
            string[] altNameList = altNamesString.Split(", ");
            List<string[]> associatedDevices = new();
            foreach (string associatedDevice in altNameList)
            {
                string[] temp = associatedDevice.Split("+");
                associatedDevices.Add(temp);
            }
            HashSet<string> processedModels = new(); // Track processed models

            foreach (string model in deviceList)
            {
                // Check if the model is part of an associated device group
                string[]? matchingGroup = associatedDevices.FirstOrDefault(group => group.Contains(model));

                if (matchingGroup != null)
                {
                    // If none of the group has been processed, add it as a new Device
                    if (!processedModels.Overlaps(matchingGroup))
                    {
                        returnList.Add(new Device(matchingGroup[0], matchingGroup.ToList(), 0)); // 0 as a placeholder for counter
                        processedModels.UnionWith(matchingGroup); // Mark all in the group as processed
                    }
                }
                else if (!processedModels.Contains(model))
                {
                    // If the model isn't in an associated group, add it individually with only itself as an alternative name
                    returnList.Add(new Device(model, new List<string> { model }, 0)); // 0 as a placeholder for counter
                    processedModels.Add(model);
                }
            }
            return returnList;
        }
        public void DevicePlusCounter(string DeviceName)
        {
            // Find the device in the list if its there.
            Device? device = Devices.FirstOrDefault(d => d.Name == DeviceName || d.AlternativeName.Contains(DeviceName));

            if (device != null)
            {
                device.Counter++; // add to its count.
            }
        }
        public override string ToString()
        {

            return  (string.Join("\n", this.Devices.Select(d => d.Counter)));
        }
    } 
}