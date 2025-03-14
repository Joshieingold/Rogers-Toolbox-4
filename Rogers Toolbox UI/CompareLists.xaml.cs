using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Rogers_Toolbox_UI
{
    public partial class CompareLists : Window
    {
        public CompareLists()
        {
            InitializeComponent();
        }

        private void CompareButton_Click(object sender, RoutedEventArgs e)
        {
            // Split the text into lists and trim each serial number
            var listA = ListAText.Text
                .Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(serial => serial.Trim()) // Trim spaces from each serial
                .ToList();

            var listB = ListBText.Text
                .Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(serial => serial.Trim()) // Trim spaces from each serial
                .ToList();

            // Find items only in List A
            var onlyInA = listA.Except(listB).ToList();
            ListAComparedText1.Text = string.Join("\n", onlyInA);
            ListAComparedLabel1.Content = $"Only in List A ({onlyInA.Count} serials)";

            // Find items only in List B
            var onlyInB = listB.Except(listA).ToList();
            ListBComparedText.Text = string.Join("\n", onlyInB);
            ListBComparedLabel.Content = $"Only in List B ({onlyInB.Count} serials)";

            // Find items in both lists
            var inBoth = listA.Intersect(listB).ToList();
            InBothListsText.Text = string.Join("\n", inBoth);
            InBothListsLabel.Content = $"In Both Lists ({inBoth.Count} serials)";
        }
        private void ListAText_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            UpdateListALabel();
        }

        private void ListBText_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            UpdateListBLabel();
        }

        private void UpdateListALabel()
        {
            // Count the number of lines in ListAText
            var lineCount = ListAText.Text.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries).Length;
            ListALabel.Content = lineCount > 0 ? $"List A - {lineCount} serials loaded" : "List A - No Serials Loaded";
        }

        private void UpdateListBLabel()
        {
            // Count the number of lines in ListBText
            var lineCount = ListBText.Text.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries).Length;
            ListBLabel.Content = lineCount > 0 ? $"List B - {lineCount} serials loaded" : "List B - No Serials Loaded";
        }
    }
}