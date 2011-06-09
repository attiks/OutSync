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
using System.Diagnostics;

namespace OutSync.UserControls
{
    public partial class ContactHeaderUserControl : System.Windows.Controls.UserControl
    {    
        public ContactHeaderUserControl()
        {
            InitializeComponent();
        }

        public void ShowCheckBox(bool show)
        {
            checkBox1.Visibility = show ? Visibility.Visible : Visibility.Collapsed;
        }

        void dataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            Contact contact = DataContext as Contact;

            if (!String.IsNullOrEmpty(contact.Status))
            {
                ToolTip = contact.Status;    
            }

            if (contact.IsMatched)
            {
                checkBox1.Visibility = Visibility.Visible;
            }
        }
    }
}