using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Toolbox_Class_Library.Properties;

namespace Toolbox_Class_Library.CtrUpdate
{
    public class CtrUpdate
    {
        public List<CTR> AllCtrs { get; set; } = new();

        public CtrUpdate()
        {
            // Deserialize contractor data from JSON
            string jsonData = Settings.Default.ContractorData;
            List<ContractorCategory> categories = string.IsNullOrWhiteSpace(jsonData)
                ? new List<ContractorCategory>()
                : JsonSerializer.Deserialize<List<ContractorCategory>>(jsonData) ?? new List<ContractorCategory>();

            // Split the stored CTR order into a list of IDs
            string[] allCtrStringList = Settings.Default.CtrOrder.Split(", ");

            foreach (string ctrString in allCtrStringList)
            {
                var matchingCategory = categories.FirstOrDefault(category => category.CtrIDs.Contains(ctrString));

                if (matchingCategory != null)
                {
                    

                    CTR newCtr = new CTR(ctrString, matchingCategory.Devices, string.Join(", ", Settings.Default.GroupedDevices));
                    AllCtrs.Add(newCtr);
                }
            }
        }
        public void Test()
        {
            foreach (CTR ctr in AllCtrs)
            {
                Console.Write(ctr.ToString());

            }
            
        }
    }
}