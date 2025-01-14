using System.Collections.Generic;
using System.Windows.Shapes;
using WindowsInput;
namespace RogersToolbox
{
    public class ActiveSerials
    {
        // Initialization
        public List<SerialNumber> Serials { get; set; }
        private InputSimulator inputSimulator = new InputSimulator();
        private int typingSpeed { get; set;}
        private int blitzImportSpeed { get; set; }

        public ActiveSerials(string serialString) 
        {
            Serials = new List<SerialNumber>(); 
            string[] betweenList = serialString.Split("\n");

            foreach (string rawSerial in betweenList)
            {
                Serials.Add(new SerialNumber(rawSerial.Trim())); 
            }
            typingSpeed = Toolbox_Class_Library.Properties.Settings.Default.TypingSpeed;
            blitzImportSpeed = Toolbox_Class_Library.Properties.Settings.Default.BlitzImportSpeed;
        }
        //  Helper Functions
        private async Task SimulateTyping(string text)
        {

            foreach (char c in text)
            {
                inputSimulator.Keyboard.TextEntry(c);  
                await Task.Delay(typingSpeed);  
            }
        }
        private void SimulateTabKey()
        {
            inputSimulator.Keyboard.KeyPress(WindowsInput.Native.VirtualKeyCode.RETURN);
        }
        public string GetRemainingSerials()
        {
            return string.Join(Environment.NewLine, Serials.Select(s => s.Serial));
        }



        // Button Click Functions
        public async Task BlitzImport()
        {
            

            // Create a copy of the list to avoid modifying the collection while iterating
            var serialsToProcess = new List<SerialNumber>(Serials);

            foreach (SerialNumber serial in serialsToProcess)
            {
                if (serial.Serial == "*")
                {
                    Serials.Remove(serial);
                    break;
                }
                else
                {
                    await SimulateTyping(serial.Serial);

                    // Remove the serial from the original list after processing
                    Serials.Remove(serial);

                    SimulateTabKey(); // Simulate pressing the Tab key or any other key as needed
                    await Task.Delay(blitzImportSpeed); // Delay between serials
                }
                
            }
        }

    }
}