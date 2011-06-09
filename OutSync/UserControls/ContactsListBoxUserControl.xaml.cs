using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Collections;
using System.Windows.Media;
using System.Windows.Input;
using Outlook = Microsoft.Office.Interop.Outlook;

namespace OutSync.UserControls
{
    /// <summary>
    /// Interaction logic for ContactsListBoxUserControl.xaml
    /// </summary>

    public partial class ContactsListBoxUserControl : System.Windows.Controls.UserControl
    {
        public ContactsListBoxUserControl()
        {
            InitializeComponent();
        }

        public void ShowContacts(bool matchedContacts)
        {
            MainWindow mainWnd = (MainWindow)App.Current.MainWindow;
            
            Binding binding = new Binding();

            binding.Source = matchedContacts ? 
                mainWnd._matchedContacts : mainWnd._unmatchedContacts;

            listBox1.SetBinding(ListBox.ItemsSourceProperty, binding);            
        }

        public Contact SelectedContact
        {
            get
            {
                if (listBox1.SelectedIndex == -1) return null;
                return listBox1.SelectedItem as Contact;
            }
        }

        private void listbox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            object item = getElementFromPoint(listBox1, e.GetPosition(listBox1));
            if (item == null)
            {
                // did not click on an item.
                return;
            }

            Contact contact = SelectedContact;
            if (contact == null) return;

            if (contact.IsMatched)
            {
                MainWindow mainWnd = (MainWindow)App.Current.MainWindow;     

                Outlook.ContactItem outlookContact =
                    mainWnd._outlookHelper.FindContactByEntryID(contact.OutlookId);

                if (outlookContact != null)
                {
                    try
                    {
                        outlookContact.Display(false);
                    }
                    catch
                    {
                    }
                }
            }
            else
            {
                //ContactDetailsWindow window = new ContactDetailsWindow();
                //window._contact = contact;
                //window.ShowDialog();
            }
        }

        private object getElementFromPoint(ListBox listbox, Point point)
        {
            UIElement element = (UIElement)listbox.InputHitTest(point);

            while (true)
            {
                if (element == listbox) return null;

                object item = listbox.ItemContainerGenerator.ItemFromContainer(element);
                bool itemFound = !(item.Equals(DependencyProperty.UnsetValue));

                if (itemFound) return item;

                element = (UIElement)VisualTreeHelper.GetParent(element);
            }
        }
    }
}