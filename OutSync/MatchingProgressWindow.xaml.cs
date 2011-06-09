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
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Collections;

using facebook;
using Outlook = Microsoft.Office.Interop.Outlook;
using facebook.Schema;

namespace OutSync
{
    /// <summary>
    /// Interaction logic for MatchingProgressWindow.xaml
    /// </summary>

    public partial class MatchingProgressWindow : System.Windows.Window
    {
        MainWindow _mainWnd = null;
        BackgroundWorker _worker = new BackgroundWorker();

        public MatchingProgressWindow()
        {
            InitializeComponent();

            _worker.DoWork += OnDoWork;
            _worker.RunWorkerCompleted += OnCompleted;
            _worker.ProgressChanged += OnProgressChanged;
            _worker.WorkerSupportsCancellation = false;
            _worker.WorkerReportsProgress = true;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            _mainWnd = (MainWindow)App.Current.MainWindow;
            _worker.RunWorkerAsync();
        }

        private void OnDoWork(object sender, DoWorkEventArgs e)
        {
            int numProcessed = 0;
            
            _worker.ReportProgress(0, Properties.Resources.Progress_BuildingFriendsList);

            IList<user> facebookFriends = _mainWnd._facebookService.friends.getUserObjects();

            // add the currently logged in FB user to the friends list            
            facebookFriends.Add(_mainWnd._facebookService.users.getInfo());

            // First create a 'normalized' list of Contacts from the user's
            // Facebook friends list. 'Normalized' means converting Facebook
            // User objects to our own 'Contact' object with some additional
            // data I want to track.

            _mainWnd._normalizedContacts.Clear();

            foreach (user friend in facebookFriends)
            {
                Contact normalizedContact = new Contact();
                normalizedContact.SetFaceBookInfo(friend);
                _mainWnd._normalizedContacts.Add(normalizedContact);

                double percentDone = ((float)++numProcessed /
                    (float)facebookFriends.Count) * 100;

                _worker.ReportProgress(
                    (int)percentDone,
                    Properties.Resources.Progress_NormalizingContacts);
            }

            _mainWnd._normalizedContacts.Sort();
            numProcessed = 0;

            // Now that we have a sorted normalized list, match Contacts in the
            // list to Outlook contacts.

            Outlook.Items items = _mainWnd._outlookHelper.ContactsFolder.Items;
            Outlook.ContactItem currentContact;

            foreach (object obj in items)
            {
                try
                {
                    currentContact = (Outlook.ContactItem)obj;
                }
                catch
                {
                    // most likely we encountered a distribution list or other 
                    // non-ContactItem object in the Contacts folder. Skip it.
                    continue;
                }

                foreach (Contact contact in _mainWnd._normalizedContacts)
                {
                    if (String.Equals(contact.Name, currentContact.FullName,
                        StringComparison.InvariantCultureIgnoreCase))
                    {
                        contact.IsMatched = true;
                        contact.OutlookId = currentContact.EntryID;
                    }
                }

                double percentDone = ((float)++numProcessed / (float)items.Count) * 100;

                _worker.ReportProgress((int)percentDone, 
                    Properties.Resources.Progress_Matching);
            }

            numProcessed = 0;

            // now separate out matched and unmatched contacts into 
            // separate arrays.

            _mainWnd._matchedContacts.Clear();
            _mainWnd._unmatchedContacts.Clear();

            foreach (Contact contact in _mainWnd._normalizedContacts)
            {
                if (contact.IsMatched)
                {
                    if (Properties.Settings.Default.ExclusionList.Contains(contact.OutlookId))
                    {
                        contact.IsIncludedInSync = false;
                    }

                    _mainWnd._matchedContacts.Add(contact);
                }
                else
                {
                    _mainWnd._unmatchedContacts.Add(contact);
                }

                double percentDone = ((float)++numProcessed / 
                    (float)_mainWnd._normalizedContacts.Count) * 100;

                _worker.ReportProgress((int)percentDone, 
                    Properties.Resources.Progress_CreatingLists);
            }
        }

        private void OnCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            _mainWnd.RefreshListBoxes();
            DialogResult = true;
        }

        private void OnProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            progressBar1.Value = e.ProgressPercentage;
            statusLabel.Content = e.UserState as string;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (_worker.IsBusy) e.Cancel = true;            
        }
    }
}