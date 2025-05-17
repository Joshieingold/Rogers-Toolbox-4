using System.Diagnostics;
using System.Reactive.Concurrency;
using System.Windows;
using RogersToolbox;
using System;
using System.Windows.Controls;
using Toolbox_Class_Library.CtrUpdate;
using Toolbox_Class_Library;
using System.Windows.Input;

namespace Rogers_Toolbox_UI
{
    public partial class MainWindow : Window
    {
        private ActiveSerials CurrentSerials; // Initialize our current serials class 
        private DatabaseConnection dbConnection; // Handles connections between the service and the database
        public string StartupText { get; set; } = $"Welcome to the Rogers Toolbox 4.1 {Toolbox_Class_Library.Properties.Settings.Default.Username}";
        private bool IsOnline = true; // Keeps track of if the service is online
        private string lastSelectedPrinter = "Custom Purolator"; // Default printer




        public MainWindow()
        {
            InitializeComponent(); // Initialize Data
            dbConnection = new DatabaseConnection(); // Creates fresh connection to database
            InitializeDataAsync(); // Call the async methods.
            LoadTheme(); // Changes the application theme on start-up
            DataContext = this;
            
            // Refresh to default settings.
            // Toolbox_Class_Library.Properties.Settings.Default.Reset();
            // Toolbox_Class_Library.Properties.Settings.Default.Save(); // Save changes if needed

        }

        // Initialization Helper Functions
        private async void InitializeDataAsync() // Initialize Async Data
        {
            try
            {
                CurrentSerials = new ActiveSerials(""); // Initialize CurrentSerials With an empty Parameter
                IsOnline = await dbConnection.CheckIsOnline(); // Checks if the service is online and updates the application accordingly.
                
                if (!IsOnline)
                {
                    Console.WriteLine($"Rogers Toolbox is Offline. Sorry {Toolbox_Class_Library.Properties.Settings.Default.Username}..");
                    this.Close();
                }
                else
                {
                    Console.WriteLine($"Rogers Toolbox is Online! Welcome {Toolbox_Class_Library.Properties.Settings.Default.Username}!");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred during initialization: {ex.Message}");
            }
        } // Initialize Async Data.
        private void LoadTheme() // Load the appropriate theme based on the setting
        {
            string selectedTheme = Toolbox_Class_Library.Properties.Settings.Default.Theme; 
            switch (selectedTheme)
            {
                case "Dark":
                    ApplyTheme("Themes/DarkTheme.xaml");
                    break;
                case "Rogers":
                    ApplyTheme("Themes/RogersTheme.xaml");
                    break;
                case "Purple":
                    ApplyTheme("Themes/PurpleTheme.xaml");
                    break;
                case "Coffee":
                    ApplyTheme("Themes/CoffeeTheme.xaml");
                    break;
                case "Fade":
                    ApplyTheme("Themes/FadeTheme.xaml");
                    break;
                case "Neon":
                    ApplyTheme("Themes/NeonTheme.xaml");
                    break;
                case "Ice":
                    ApplyTheme("Themes/IceTheme.xaml");
                    break;
                default:
                    ApplyTheme("Themes/DarkTheme.xaml"); 
                    break;
            }
        } // Themes
        public void ApplyTheme(string themePath) // Handles Changing the XAML based on theme.
        {
            ResourceDictionary newTheme = (ResourceDictionary)Application.LoadComponent(new Uri(themePath, UriKind.Relative));
     
            Application.Current.Resources.MergedDictionaries.Clear();
            Application.Current.Resources.MergedDictionaries.Add(newTheme);
        }

        // UI Helper Functions

        public void UpdateMessage(string text) // Updates the Textbox that communicates to the user. 
        {
            InfoBox.Content = (text);
        }  
        private void UpdateSerialsDisplay() // Specifically Updates the Textbox to communicate with the user how many serials are currently loaded.
        {
            TextBox.Clear();
            TextBox.Text = CurrentSerials.GetRemainingSerials(); // Get remaining serials from CurrentSerials
            int remainingSerials = CurrentSerials.Serials.Count; // Update remaining serials count
            InfoBox.Content = ($"{remainingSerials} Serials Loaded");
        } 
        private void TextBox_TextChanged(object sender, TextChangedEventArgs e) // Detects when the input box has changed and updates the user textbox with how many there are in real time.
        {
            // Split the text of the TextBox into lines based on newline characters
            var lines = TextBox.Text.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

            // Create the line number text (e.g., 1, 2, 3, ...)
            var lineNumbers = string.Join("\n", lines.Select((line, index) => $"{index + 1}:"));

            // Update the line number label with the new line numbers
            LineNumberLabel.Text = lineNumbers;
            UpdateMessage($"{lines.Count()} Serials Loaded");


        } 
        private ActiveSerials GetTextboxText() // Transforms Serials into an ActiveSerials instance.
        {
            string[] lines = TextBox.Text.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

            string output = string.Join("\n", lines);
            ActiveSerials tempList = new ActiveSerials(output);

            return tempList;
        }
        private void ShowPrintMenu(object sender, MouseButtonEventArgs e)
        {
            Button btn = sender as Button;
            ContextMenu menu = new ContextMenu();

            string[] printers = { "Purolator", "Barcodes", "Lot Sheets", "Custom Purolator" };

            foreach (string printer in printers)
            {
                MenuItem item = new MenuItem { Header = printer };
                item.Click += (s, args) => SetPrinter(printer);
                menu.Items.Add(item);
            }

            btn.ContextMenu = menu;
            menu.IsOpen = true;
        }// Open menu on right-click (or long-press for touchscreen)
        private void SetPrinter(string printer)
        {
            lastSelectedPrinter = printer;
            if (printer == "Purolator")
            {
                PrintButton.Tag = "pack://application:,,,/Icons/PurolatorIcon.png";
            }
            else if (printer == "Barcodes")
            {
                PrintButton.Tag = "pack://application:,,,/Icons/BarcodeIcon.png";
            }
            else if (printer == "Lot Sheets")
            {
                PrintButton.Tag = "pack://application:,,,/Icons/LotSheetIcon.png";
            }
            else if (printer == "Custom Purolator")
            {
                PrintButton.Tag = "pack://application:,,,/Icons/PrintIcon.png";
            }
            else
            {
                PrintButton.Tag = "pack://application:,,,/Icons/PrinterIcon.png";
            }
        }// Set the new default printer

        // Main Functionality
        private async void Button_Click(object sender, RoutedEventArgs e) // Handles Actions based on Button Clicked
        {
            var button = sender as System.Windows.Controls.Button; // Set for ease of use.

            CurrentSerials = GetTextboxText(); // Get a fresh case of ActiveSerials from the textbox.
            // Checking if the service is still online.
            IsOnline = await dbConnection.CheckIsOnline();
            if (!IsOnline)
            {
                this.Close();
            }
                // Finds Button that was clicked and preforms the appropriate action.
                switch (button.Name)
                {
                    case "BlitzButton":
                        // Initialization
                        Stopwatch stopwatch = new Stopwatch();
                        UpdateMessage("Starting Blitz Import, Please click target Location");
                        await Task.Delay(6000); // Gives user 6 seconds to select Import Location.
                        stopwatch.Start();

                        // Create ActiveSerials instance with a callback for updates to the textbox.
                        CurrentSerials = new ActiveSerials(TextBox.Text, UpdateSerialsDisplay);
                        await CurrentSerials.BlitzImport(); // Runs and updates display while processing
                        stopwatch.Stop();

                        // Formats the time the import took
                        TimeSpan ts = stopwatch.Elapsed;
                        string elapsedTime = String.Format("{0:00}h : {1:00}m : {2:00}s : {3:00} ms",
                        ts.Hours, ts.Minutes, ts.Seconds,
                        ts.Milliseconds / 10);
                        UpdateMessage($"Import Completed in {elapsedTime}");

                        break; // Done!
                    case "FlexiButton":
                        Stopwatch flexiStopwatch = new Stopwatch();
                        UpdateMessage("Starting FlexiPro Import, Please click target Location");
                        await Task.Delay(6000); // Gives user 6 seconds to select Import Location.
                        flexiStopwatch.Start();

                        // Create ActiveSerials instance with a callback for updates to the textbox.
                        CurrentSerials = new ActiveSerials(TextBox.Text, UpdateSerialsDisplay);
                        await CurrentSerials.FlexiProImport(); // Runs and updates display while processing
                        flexiStopwatch.Stop();

                        // Formats the time the import took
                        TimeSpan Flexts = flexiStopwatch.Elapsed;
                        string FlexieElapsedTime = String.Format("{0:00}h : {1:00}m : {2:00}s : {3:00} ms",
                        Flexts.Hours, Flexts.Minutes, Flexts.Seconds,
                        Flexts.Milliseconds / 10);
                        UpdateMessage($"Import Completed in {FlexieElapsedTime}");

                        break; // Done !!
                    case "WMSButton":
                        Stopwatch wmsStopwatch = new Stopwatch();
                        UpdateMessage("Starting WMS Import, Please click target Location");
                        await Task.Delay(6000); // Gives user 6 seconds to select Import Location.
                        wmsStopwatch.Start();
                        // Create ActiveSerials instance with a callback for updates to the textbox.
                        CurrentSerials = new ActiveSerials(TextBox.Text, UpdateSerialsDisplay);
                        await CurrentSerials.WmsImport(); // Runs and updates display while processing
                        wmsStopwatch.Stop();
                        // Formats the time the import took
                        TimeSpan WmsTS = wmsStopwatch.Elapsed;
                        string WmsElapsedTime = String.Format("{0:00}h : {1:00}m : {2:00}s : {3:00} ms",
                        WmsTS.Hours, WmsTS.Minutes, WmsTS.Seconds,
                        WmsTS.Milliseconds / 10);
                        UpdateMessage($"Import Completed in {WmsElapsedTime}");
                        PassFailWindow passFailWindow = new PassFailWindow(CurrentSerials.passList, CurrentSerials.failList);
                        passFailWindow.Show();

                        break; // Done !!
                    case "OpenExcelButton": // Retrieves Serials from the First Column of the Excel given.

                        // Creates new instance of ActiveSerials and sets them
                        ActiveSerials TempList = new ActiveSerials(CurrentSerials.OpenExcel());
                        CurrentSerials = TempList;
                        UpdateSerialsDisplay(); // Updates display with the currently loaded serials.
                        break; // Done!
                    case "SettingsButton": // Creates a new instance of settings window with "this" as reference.

                        SettingsWindow settingsWindow = new SettingsWindow(this);
                        settingsWindow.ShowDialog();
                        break; // Done!
                    case "CTRButton": // Creates a new instance of CtrUpdate and runs it.

                        CtrUpdate ctrUpdate = new CtrUpdate();                     
                        ctrUpdate.InitializeData(); // Ensure this is awaited
                        await Task.Delay(7000); // Initial delay if needed
                        string[] ctrList = (Toolbox_Class_Library.Properties.Settings.Default.CtrOrder).Split(", ");
                        
                        foreach (string ctr in ctrList)
                        {
                        UpdateMessage($"Updating CTR {ctr}");
                        await ctrUpdate.RunAutomation(ctr);
                        

                    }
                        UpdateMessage("CTR Update Completed!");
                        break;
                    case "FormatSerialsButton": // Opens the Format Serials window and uses the serials in the textbox to be used.
                        string serialsText = TextBox.Text;
                        FormatWindow formatWindow = new FormatWindow(serialsText);
                        formatWindow.Owner = this; // Set MainWindow as the owner
                        formatWindow.ShowDialog(); // Show as a modal dialog

                        // After closing, update TextBox if FormattedSerials is set
                        if (!string.IsNullOrEmpty(formatWindow.FormattedSerials))
                        {
                            TextBox.Text = formatWindow.FormattedSerials;
                        }

                        break;
                    case "PrintButton": // Opens the Print Menu
                            PrinterConnection printer = new PrinterConnection(CurrentSerials);
                        if (lastSelectedPrinter == "Purolator")
                        {
                            printer.DefaultPrintPurolator();
                        }
                        else if (lastSelectedPrinter == "Barcodes")
                            printer.PrintBarcodes();
                        else if (lastSelectedPrinter == "Lot Sheets")
                        {
                            printer.PrintLotSheets();
                        }
                        else if (lastSelectedPrinter == "Custom Purolator")
                        {
                            CustomPrintWindow window = new CustomPrintWindow();
                            bool? result = window.ShowDialog();

                            if (result == true)
                            {
                                int formatBy = window.FormatBy;
                                string selectedDevice = window.SelectedDevice;

                                // Create a new PrinterConnection instance with the selected settings
                                PrinterConnection printerConnection = new PrinterConnection(CurrentSerials);

                                // Set target device using reflection to access private field
                                typeof(PrinterConnection)
                                    .GetField("targetDevice", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                                    ?.SetValue(printerConnection, selectedDevice);

                                try
                                {
                                    printerConnection.CustomPrintPurolator(formatBy, selectedDevice); // Run the print job
                                }
                                catch (Exception ex)
                                {
                                    MessageBox.Show($"Error during printing: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                                }
                            }
                        }
                        break;
                    case "GraphButton":
                        // Opens the Stats Window
                        DatabaseConnection databaseConnection = new DatabaseConnection();
                        StatsWindow statsWindow = new StatsWindow();
                        statsWindow.Show();
                        break;
                    case "CompareListButton":
                        CompareLists compareLists = new CompareLists();
                        compareLists.Show();
                        break;
                default: // Just in case :)
                        UpdateMessage("Didn't read anything :(");
                        break;

                }
        } // Handles what happens when a button is clicked

    }
}

// To Do:

// 2. I want there to be an easy way for users to send each other serials across toolboxs.