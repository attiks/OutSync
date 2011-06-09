using System;
using System.IO;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Navigation;
using System.Reflection;
using System.Windows.Documents;
using System.Diagnostics;

namespace OutSync
{
	public partial class AboutWindow
	{
		public AboutWindow()
		{
			this.InitializeComponent();			
		}

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            versionLabel.Content = String.Format(
                "{0} v{1}", AssemblyProduct, AssemblyVersion);
        }

        private void requestNavigate(object sender, RoutedEventArgs e)
        {
            Hyperlink link = sender as Hyperlink;
            Process.Start(link.NavigateUri.ToString());
            Close();
        }

        private void checkForUpdate(object sender, RoutedEventArgs e)
        {            
            Utilities.CheckForUpdate(this, false);
            Close();
        }

        public string AssemblyProduct
        {
            get
            {
                // Get all Product attributes on this assembly
                object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(
                    typeof(AssemblyProductAttribute), false);

                // If there aren't any Product attributes, return an empty string
                if (attributes.Length == 0) return String.Empty;
                
                // If there is a Product attribute, return its value
                return ((AssemblyProductAttribute)attributes[0]).Product;
            }
        }

        public string AssemblyVersion
        {
            get
            {
                Version version = Assembly.GetExecutingAssembly().GetName().Version;                
                return String.Format("{0}.{1} (build {2})", 
                    version.Major, version.Minor, version.Build);
            }
        }
	}
}