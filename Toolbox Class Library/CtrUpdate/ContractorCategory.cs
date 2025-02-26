using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Toolbox_Class_Library.CtrUpdate
{
    public class ContractorCategory
    {
        public string Name { get; set; } // Category Name
        public List<string> DeviceNames { get; set; } = new List<string>(); // List of devices
        public List<string> CtrIDs { get; set; } = new();

    }

}
