using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace OutSync.UserControls
{
    /// <summary>
    /// Interaction logic for LogoHeaderUserControl.xaml
    /// </summary>

    public partial class LogoHeaderUserControl : System.Windows.Controls.UserControl
    {
        public LogoHeaderUserControl()
        {
            InitializeComponent();
        }

        private void about_Click(object sender, RoutedEventArgs e)
        {
            AboutWindow window = new AboutWindow();
            window.ShowDialog();
        }
    }
}