using System.Windows;
using System.Windows.Controls;


namespace Rogers_Toolbox_UI
{
    /// <summary>
    /// Interaction logic for PassFailWindow.xaml
    /// </summary>
    public partial class PassFailWindow : Window
    {
        public PassFailWindow(List<string> passedList, List<string> failedList)
        {
            InitializeComponent();
            PassedListBox.ItemsSource = passedList; // Bind passed items
            FailedListBox.ItemsSource = failedList; // Bind failed items
        }

        private void FailedListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Check if an item is selected
            if (FailedListBox.SelectedItem != null)
            {
                // Get the selected item
                string selectedItem = FailedListBox.SelectedItem.ToString();


                // Convert selected item to string and set it to clipboard
                string clipboardText = selectedItem.ToString();

                // Check if clipboardText is not null or empty
                if (!string.IsNullOrEmpty(clipboardText))
                {
                    try
                    {
                        Clipboard.SetText(clipboardText);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Failed to copy text to clipboard: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                else
                {
                    MessageBox.Show("No valid text to copy.", "Clipboard", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
        }
        private void PassedListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Check if an item is selected
            if (PassedListBox.SelectedItem != null)
            {
                // Get the selected item
                string selectedItem = PassedListBox.SelectedItem.ToString();


                // Convert selected item to string and set it to clipboard
                string clipboardText = selectedItem.ToString();

                // Check if clipboardText is not null or empty
                if (!string.IsNullOrEmpty(clipboardText))
                {
                    try
                    {
                        Clipboard.SetText(clipboardText);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Failed to copy text to clipboard: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                else
                {
                    MessageBox.Show("No valid text to copy.", "Clipboard", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
        }
        private void CopyPassedButton_Click(object sender, RoutedEventArgs e)
        {
            CopyListBoxItemsToClipboard(PassedListBox);
        }

        private void CopyFailedButton_Click(object sender, RoutedEventArgs e)
        {
            CopyListBoxItemsToClipboard(FailedListBox);
        }

        private void CopyListBoxItemsToClipboard(ListBox listBox)
        {
            // Check if the ListBox has items
            if (listBox.Items.Count > 0)
            {
                // Create a string with all items separated by newlines
                var items = listBox.Items.Cast<object>().Select(item => item.ToString());
                string clipboardText = string.Join(Environment.NewLine, items);

                // Copy to clipboard
                Clipboard.SetText(clipboardText);


            }
        }
    }
}