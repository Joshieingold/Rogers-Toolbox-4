using ClosedXML.Excel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Text.Json;
using System.Windows;
using Toolbox_Class_Library.Properties;
using WindowsInput;


namespace Toolbox_Class_Library.TechUpdate
{


    // Needs
    // 1.
    // List of all Techs generated from the settings as a class.
    // To know if it should run the automation or not, and to grab ctr import speed.
    // The number to go down. and an option to have a custom header if not it will default to Qty - Date.
    // 2.
    // allow user to select an excel file to process.
    // 3.
    // Process the file and update the Techs.
    // 4.
    // Run the automation if it is true and push to database.
    public class TechUpdate
    {

        public List<Tech> Techs { get; set; } = new();
        public bool RunTechAutomation = Settings.Default.RunTechAutomation;
        public int NumDown = Settings.Default.TechNumDown;
        public string DateText = Settings.Default.TechHeaderText;
        
        public TechUpdate()
        {
            string[] allTechStringList = Settings.Default.TechIds.Split(", ");
            foreach (string tech in allTechStringList)
            {
                Techs.Add(new Tech(tech));
            }
        }
        public void TechAutomation()
        {

        }
        public void UpdateTechs(IXLWorksheet sheet)
        {
            string[] UpdateList = Settings.Default.TechIds.Split(", ");
            foreach (var row in sheet.RowsUsed())
            {
                var RowTechID = row.Cell(7).GetValue<string>();
                var RowInventoryType = row.Cell(10).GetValue<string>();
                var RowWarehouseCtrID = row.Cell(2).GetValue<string>();

                if (UpdateList.Contains(RowCtrID) || UpdateList.Contains(RowWarehouseCtrID))
                {
                    var matchedCtr = this.Techs.FirstOrDefault(ctr => ctr.Name == RowCtrID || ctr.Name == RowWarehouseCtrID);
                    if (matchedCtr != null)
                    {
                        if (RowInventoryType.StartsWith("CTR.Subready."))
                        {
                            var RowDeviceCode = row.Cell(6).GetValue<string>();
                            matchedCtr.DevicePlusCounter(RowDeviceCode);
                        }
                        if (UpdateList.Contains(RowWarehouseCtrID))
                        {
                            var RowDeviceCode = row.Cell(6).GetValue<string>();
                            matchedCtr.DevicePlusCounter(RowDeviceCode);
                        }

                    }
                }

            }
        }
        public void OpenExcel()
        {

        }








    }

}