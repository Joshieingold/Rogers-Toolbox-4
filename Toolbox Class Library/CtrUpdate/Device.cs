using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Toolbox_Class_Library.CtrUpdate
{
    public class Device
    {
        public string Name { get; set; }
        public List<string> AlternativeName { get; set; }
        public int Counter { get; set; }

        public Device(string name, List<string> alternativeName, int counter)
        {
            Name = name;
            AlternativeName = alternativeName;
            Counter = counter;

        }

        public override string ToString()
        {
            return $"{Name}: {Counter})";
        }
    }
}
