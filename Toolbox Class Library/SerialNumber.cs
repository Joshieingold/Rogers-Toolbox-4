namespace RogersToolbox
{
    public class SerialNumber
    {
        public string Serial { get; set; }
        public string Device { get; set; }
        public string Location { get; set; }

        public SerialNumber(string serial) 
        {
            Serial = serial;
            Device = DetermineDevice(Serial);
            Location = "Unknown";
        }
        private string DetermineDevice(string Serial)
        {
            if (Serial.StartsWith("TM"))
                return "IPTVTCXI6HD";
            else if (Serial.StartsWith("M"))
                return "IPTVARXI6HD";
            else if (Serial.StartsWith("409"))
                return "CGM4981COM";
            else if (Serial.StartsWith("XI1"))
                return "SCXI11BEI";
            else if (Serial.StartsWith("336"))
                return "CGM4331COM";
            else if (Serial.StartsWith("AS"))
                return "XE2SGROG1";
            else if (Serial.StartsWith("B60"))
                return "CODA5810";
            else if (Serial.StartsWith("ALC"))
                return "XS010XQ";
            else if (Serial.StartsWith("B1SC"))
                return "SCHB1AEW";
            else if (Serial.StartsWith("C3SC"))
                return "SCHC3AE0";
            else if (Serial.StartsWith("C22"))
                return "SCHC2AEW";
            else if (Serial.Contains("-"))
                return "MR36HW";
            else if (Serial.StartsWith("D2LD"))
                return "LDHD2AZW";
            else
                return "TG4482A";
        }
        private void SetLocation(string NewLocation)
        {
            Location = NewLocation;
        }

    }
}

