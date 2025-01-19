using System;
using System.Collections.Generic;
using System.Globalization;
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
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        public SettingsWindow()
        {
            InitializeComponent();
            this.DataContext = Toolbox_Class_Library.Properties.Settings.Default;
        }
        
        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            // Save all the settings to persistent storage
            Toolbox_Class_Library.Properties.Settings.Default.Save();

            // Close the settings window (optional)
            this.Close();
        }
    }
}
