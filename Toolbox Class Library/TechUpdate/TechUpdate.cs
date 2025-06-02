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
            if (DateText == "")
            {
                SetDateText();
            }
        }
        public void InitializeData()
        {
            string excelPath = OpenExcel();
            if (excelPath != "")
            {
                UpdateTechs(excelPath);
            }
            else
            {
                Console.WriteLine("Error");
            }

        }
        private void SetDateText()
        {
            DateText = $"Qty - {DateTime.Now:MMMM d}";
        }
        public async Task TechAutomation(string ThisTech)
        {

            var tech = Techs.FirstOrDefault(c => c.Name == ThisTech);
            if (tech != null)
            {
                Console.WriteLine($"\nProcessing {ThisTech}");
                if (RunTechAutomation)
                {
                    System.Windows.Clipboard.SetText(DateText);
                    var sim = new InputSimulator();
                    sim.Keyboard.ModifiedKeyStroke(WindowsInput.Native.VirtualKeyCode.CONTROL, WindowsInput.Native.VirtualKeyCode.VK_V);
                    await Task.Delay(250);
                    sim.Keyboard.KeyPress(WindowsInput.Native.VirtualKeyCode.DOWN);
                    await Task.Delay(500);
                    System.Windows.Clipboard.SetText(tech.ToString());
                    sim.Keyboard.ModifiedKeyStroke(WindowsInput.Native.VirtualKeyCode.CONTROL, WindowsInput.Native.VirtualKeyCode.VK_V);
                    for (int i = 0; i < NumDown; i++)
                    {
                        await Task.Delay(250);
                        sim.Keyboard.KeyPress(WindowsInput.Native.VirtualKeyCode.DOWN);
                    }
                }
                DatabaseConnection db_Tech = new DatabaseConnection();
                await db_Tech.PushTechData(tech.Name, tech.Devices);
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
                    var matchedTech = Techs.FirstOrDefault(tech =>
                        tech.Name == RowTechID ||
                        string.Equals(tech.Name, RowTechID, StringComparison.OrdinalIgnoreCase)
                    );
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