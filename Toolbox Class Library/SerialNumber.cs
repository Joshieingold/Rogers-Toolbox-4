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
            else
                return "TG4482A";
        }
        private void SetLocation(string NewLocation)
        {
            Location = NewLocation;
        }

    }
}
public async Task WmsImport()
//         {
//             var serialsToProcess = new List<SerialNumber>(Serials);
//             List<string> passList = [];
//             List<string> failList = [];
//             foreach (SerialNumber serial in serialsToProcess)
//             {
//                 if (serial.Serial == "*")
//                 {
//                     Serials.Remove(serial);
//                     break;
//                 }
//                 else
//                 {
//                     await SimulateTyping(serial.Serial);
//                     Serials.Remove(serial);
//                     SimulateTabKey();
//                     await Task.Delay(wmsImportSpeed);
//                     bool isPixelGood = CheckPixel("(250, 250, 250)", GetCurrentPixel(wmsCheckPixel));
//                     if (isPixelGood == false)
//                     {
//                         failedList.add(serial.Serial);
//                         // Add automation for when there is a failure.
//                     }
//                     else
//                     {
//                         passedList.add(serial.Serial);
//                     }


//                     await Task.Delay(wmsImportSpeed / 2);

//                     serialsUpdatedCallback?.Invoke(); // Notify UI to update
                
//             }

//         }
