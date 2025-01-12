using RogersToolbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Toolbox_Class_Library
{
    public class Settings
    {
        public List<SerialNumber> Serials { get; set; }
        public string ContractorString {  get; set; }
        public int BlitzImportSpeed { get; set; }
        public int WMSImportSpeed { get; set; }
        public int FlexiImportSpeed { get; set; }
        public string BartenderNotepadPath { get; set; }
        public string Name { get; set; }
        public bool ReverseImport { get; set; }
        public int TypingSpeed { get; set; }
        public string FlexiProPixel {  get; set; }
        public string WMSPixel { get; set; }

    }
}
