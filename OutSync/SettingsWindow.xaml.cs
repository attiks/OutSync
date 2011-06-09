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
using System.Windows.Shapes;

namespace OutSync
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>

    public partial class SettingsWindow : System.Windows.Window
    {
        public SettingsWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            aeroGlassCheckBox.IsEnabled = Environment.OSVersion.Version.Major >= 6;
            settingsPanel.DataContext = Properties.Settings.Default;
        }

        private void ok_Click(object sender, RoutedEventArgs e)
        {
            BindingExpression be;
            
            be = aeroGlassCheckBox.GetBindingExpression(CheckBox.IsCheckedProperty);
            be.UpdateSource();

            be = autoUpdateCheckBox.GetBindingExpression(CheckBox.IsCheckedProperty);
            be.UpdateSource();

            be = updatePictureCheckBox.GetBindingExpression(CheckBox.IsCheckedProperty);
            be.UpdateSource();

            be = updateBirthdayCheckBox.GetBindingExpression(CheckBox.IsCheckedProperty);
            be.UpdateSource();

            Properties.Settings.Default.Save();
            DialogResult = true;
        }
    }
}