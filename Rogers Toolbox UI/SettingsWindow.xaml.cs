using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using Toolbox_Class_Library.CtrUpdate;
using Toolbox_Class_Library.Properties;

namespace Rogers_Toolbox_UI
{
    public partial class SettingsWindow : Window
    {
        private MainWindow mainWindow;
        private List<ContractorCategory> contractorCategories = new();

        public SettingsWindow(MainWindow main)
        {
            InitializeComponent();
            this.DataContext = Settings.Default;
            mainWindow = main;
            LoadContractorData();
            InitializeThemeComboBox();
            InitializeWmsFailSettingComboBox();
            Console.WriteLine(Settings.Default.ContractorData);
        }

        private void InitializeThemeComboBox()
        {
            // Set the ComboBox to the current theme
            string currentTheme = Settings.Default.Theme;
            ThemeComboBox.SelectedItem = ThemeComboBox.Items
                .Cast<ComboBoxItem>()
                .FirstOrDefault(item => item.Content.ToString() == currentTheme);
        }

        private void InitializeWmsFailSettingComboBox()
        {
            // Set the ComboBox to the current theme
            string currentAutomation = Settings.Default.WmsFailAutomation;
            WmsFailSettingComboBox.SelectedItem = WmsFailSettingComboBox.Items
                .Cast<ComboBoxItem>()
                .FirstOrDefault(item => item.Content.ToString() == currentAutomation);
        }
        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            // Save all the settings to persistent storage
            Settings.Default.Save();
            mainWindow.ApplyTheme($"Themes/{Settings.Default.Theme}Theme.xaml");
            mainWindow.UpdateMessage($"Your settings have been successfully updated {Settings.Default.Username}!");
            this.Close();
        }

        private void ThemeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ThemeComboBox.SelectedItem is ComboBoxItem selectedItem)
            {
                string selectedTheme = selectedItem.Content.ToString();
                Settings.Default.Theme = selectedTheme; // Update the setting
                Settings.Default.Save(); // Save the setting

                // Apply the new theme immediately
                mainWindow.ApplyTheme($"Themes/{selectedTheme}Theme.xaml");
            }
        }
        private void WmsFailAutomationSetting_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (WmsFailSettingComboBox.SelectedItem is ComboBoxItem selectedItem)
            {
                string selectedFailAutomation = selectedItem.Content.ToString();
                Settings.Default.WmsFailAutomation = selectedFailAutomation;
                Settings.Default.Save();
            }
        }

        public List<ContractorCategory> LoadContractorData()
        {
            string jsonData = Settings.Default.ContractorData;

            if (string.IsNullOrWhiteSpace(jsonData))
                return new List<ContractorCategory>();

            contractorCategories = JsonSerializer.Deserialize<List<ContractorCategory>>(jsonData) ?? new List<ContractorCategory>();

            ContractorCategoryComboBox.ItemsSource = contractorCategories;
            return contractorCategories;
        }

        public void SaveContractorData(List<ContractorCategory> categories)
        {
            string jsonData = JsonSerializer.Serialize(categories);
            Settings.Default.ContractorData = jsonData;
            Settings.Default.Save();
        }

        private void AddCategory_Click(object sender, RoutedEventArgs e)
        {
            string categoryName = NewCategoryTextBox.Text.Trim();
            if (string.IsNullOrEmpty(categoryName) || contractorCategories.Any(c => c.Name == categoryName)) return;

            var newCategory = new ContractorCategory { Name = categoryName };
            contractorCategories.Add(newCategory);
            ContractorCategoryComboBox.Items.Refresh();
            SaveContractorData(contractorCategories);
        }

        private void ContractorCategoryComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ContractorCategoryComboBox.SelectedItem is ContractorCategory selectedCategory)
            {
                DevicesListBox.ItemsSource = selectedCategory.Devices;
                CtrIDListBox.ItemsSource = selectedCategory.CtrIDs;
            }
        }

        private void AddDevice_Click(object sender, RoutedEventArgs e)
        {
            if (ContractorCategoryComboBox.SelectedItem is ContractorCategory selectedCategory)
            {
                string deviceName = DeviceNameTextBox.Text.Trim();
                if (!string.IsNullOrEmpty(deviceName) && !selectedCategory.Devices.Contains(deviceName))
                {
                    selectedCategory.Devices.Add(deviceName);
                    DevicesListBox.Items.Refresh();
                    SaveContractorData(contractorCategories);
                }
            }
        }

        private void AddCtrID_Click(object sender, RoutedEventArgs e)
        {
            if (ContractorCategoryComboBox.SelectedItem is ContractorCategory selectedCategory)
            {
                string ctrID = CtrIDTextBox.Text.Trim();  // Get CTR ID as a string

                if (!string.IsNullOrEmpty(ctrID) && !selectedCategory.CtrIDs.Contains(ctrID))  // Check if it's not empty and not in the list
                {
                    selectedCategory.CtrIDs.Add(ctrID);  // Add as a string

                    // Force UI to update
                    CtrIDListBox.ItemsSource = null;
                    CtrIDListBox.ItemsSource = selectedCategory.CtrIDs;

                    SaveContractorData(contractorCategories);
                }
            }
        }

        private void DeleteCategory_Click(object sender, RoutedEventArgs e)
        {
            if (ContractorCategoryComboBox.SelectedItem is ContractorCategory selectedCategory)
            {
                contractorCategories.Remove(selectedCategory);
                SaveContractorData(contractorCategories);

                // Refresh UI
                ContractorCategoryComboBox.ItemsSource = null;
                ContractorCategoryComboBox.ItemsSource = contractorCategories;
                DevicesListBox.ItemsSource = null;
                CtrIDListBox.ItemsSource = null;
            }
        }

        private void RemoveDevice_Click(object sender, RoutedEventArgs e)
        {
            if (ContractorCategoryComboBox.SelectedItem is ContractorCategory selectedCategory &&
                DevicesListBox.SelectedItem is string selectedDevice)
            {
                selectedCategory.Devices.Remove(selectedDevice);
                DevicesListBox.ItemsSource = null;
                DevicesListBox.ItemsSource = selectedCategory.Devices;
                SaveContractorData(contractorCategories);
            }
        }

        private void RemoveCtrID_Click(object sender, RoutedEventArgs e)
        {
            if (ContractorCategoryComboBox.SelectedItem is ContractorCategory selectedCategory &&
                CtrIDListBox.SelectedItem is string selectedCtrID)  // Ensure it's a string
            {
                selectedCategory.CtrIDs.Remove(selectedCtrID);  // Remove string from the list
                CtrIDListBox.ItemsSource = null;
                CtrIDListBox.ItemsSource = selectedCategory.CtrIDs;  // Update UI
                SaveContractorData(contractorCategories);  // Save changes
            }
        }
    }

    public class ContractorCategory
    {
        public string Name { get; set; }
        public List<string> Devices { get; set; } = new();
        public List<string> CtrIDs { get; set; } = new();
    }
}