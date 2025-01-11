using System.Collections.Generic;

namespace RogersToolbox
{
    public class ActiveSerials
    {
        public List<SerialNumber> Serials { get; private set; }

        public ActiveSerials(string serialString) 
        {
            Serials = new List<SerialNumber>(); 
            string[] betweenList = serialString.Split("\n");

            foreach (string rawSerial in betweenList)
            {
                Serials.Add(new SerialNumber(rawSerial.Trim())); 
            }
        }
    }
}