using ClosedXML.Excel;
using DocumentFormat.OpenXml.Packaging;
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
            string excelPath = OpenExcel();  
            if (excelPath != "")
            {
                UpdateTechs(excelPath);
                PrintTechs();
            }
            else
            {
                Console.WriteLine("Error");
            }
        }
        public void UpdateTechs(string ExcelPath)
        {
            using var workbookInstance = new XLWorkbook(ExcelPath);
            var sheet = workbookInstance.Worksheet(1); // Process the first sheet
            
            string[] UpdateList = Settings.Default.TechIds.Split(", ");
            foreach (var row in sheet.RowsUsed())
            {
                var RowTechID = (row.Cell(8).GetValue<string>());
                

                if (UpdateList.Contains(RowTechID)) 
                {
                    var matchedTech = Techs.FirstOrDefault(ctr => (ctr.Name) == RowTechID);
                    if (matchedTech != null)
                    {

                            var RowDeviceCode = row.Cell(6).GetValue<string>();
                            matchedTech.DevicePlusCounter(RowDeviceCode);

                    }
                }

            }
        }
        public string OpenExcel()
        {
            {
                Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog
                {
                    Title = "Select Excel File for Tech Update",
                    Filter = "Excel files (*.xlsx)|*.xlsx|All files (*.*)|*.*"
                };

                if (openFileDialog.ShowDialog() == true)
                {
                    return openFileDialog.FileName;
                }
                else
                {
                    return "";
                }
            }
        }
        public void PrintTechs()
        {
            foreach (Tech tech in Techs)
            {
                Console.WriteLine(tech.ToString());
            }
        }








    }

}