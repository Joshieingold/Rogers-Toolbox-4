using System.Diagnostics;
using System.Reactive.Concurrency;
using System.Windows;
using RogersToolbox;
using System;
using System.Windows.Controls;
using Toolbox_Class_Library.CtrUpdate;
using Toolbox_Class_Library;

namespace Rogers_Toolbox_UI
{
    public partial class MainWindow : Window
    {
        private ActiveSerials CurrentSerials; // Initialize our current serials class 
        public string StartupText { get; set; } = $"Welcome to the Rogers Toolbox 4.0 {Toolbox_Class_Library.Properties.Settings.Default.Username}";
        private bool IsOnline = true; // Keeps track of if the service is online
        private DatabaseConnection dbConnection; // Handles connections between the service and the database

        public MainWindow()
        {
            InitializeComponent(); // Initialize Data
            dbConnection = new DatabaseConnection(); // Creates fresh connection to database
            InitializeDataAsync(); // Call the async methods.
            LoadTheme(); // Changes the application theme on start-up
            DataContext = this;
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
                case "Club":
                    ApplyTheme("Themes/ClubTheme.xaml");
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
                    ctrUpdate.Run();
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

                default: // Just in case :)
                    UpdateMessage("Didn't read anything :(");
                    break;
                    
            }
        } // Handles what happens when a button is clicked

    }
}

// To Do:
// 1. Make settings allow user to choose the column of the imported excel.
