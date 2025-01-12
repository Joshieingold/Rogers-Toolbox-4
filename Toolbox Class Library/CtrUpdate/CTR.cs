using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Toolbox_Class_Library.CtrUpdate
{
    public class CTR
    {
        public string Name { get; set; }
        public string Company { get; set; }
        public List<Device> Devices { get; set; }

        public CTR(string name, string company, List<Device> devices)
        {
            Name = name;
            Company = company;
            Devices = devices;
        }

        public override string ToString()
        {
            string deviceString = String.Join(", ", Devices);
            return $"CTR: {Name}, Associated with {Company}, Logging devices: {deviceString}";
        }
        public List<string> GetDeviceList()
        {
            return Devices.Select(device => device.Name).ToList();
        }
        public string GetDeviceTotalString()
        {
            List<string> TempList = new List<string>();
            foreach (Device device in Devices) 
            {
                TempList.Add((device.Counter).ToString());
            }
            return string.Join("\n", TempList);
        }
    }
}

