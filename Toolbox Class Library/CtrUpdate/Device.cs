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
        public int Counter { get; set; }

        public Device(string name, int counter)
        {
            Name = name;
            Counter = counter;
        }

        public override string ToString()
        {
            return $"{Name}: {Counter})";
        }
    }
}
