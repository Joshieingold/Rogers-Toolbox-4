using System.Security.Cryptography.X509Certificates;
using System.Windows;
using RogersToolbox;


namespace Rogers_Toolbox_UI
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            InitializeData();
        }
        public void InitializeData()
        {
            ActiveSerials CurrentSerials = new ActiveSerials("");
        }
    }
}