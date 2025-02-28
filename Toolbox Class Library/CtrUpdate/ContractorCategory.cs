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
        public List<string> Devices { get; set; } = new List<string>(); // Devices field
        public List<string> CtrIDs { get; set; } = new List<string>(); // CtrIDs list
    }


}
