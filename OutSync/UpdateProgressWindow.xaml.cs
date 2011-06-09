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
using System.ComponentModel;
using System.Collections;
using System.Diagnostics;
using System.IO;
using Outlook = Microsoft.Office.Interop.Outlook;
using System.Threading;

namespace OutSync
{
    /// <summary>
    /// Interaction logic for UpdateProgressWindow.xaml
    /// </summary>

    public partial class UpdateProgressWindow : System.Windows.Window
    {
        MainWindow _mainWnd = null;
        BackgroundWorker _worker = new BackgroundWorker();
        int _numProcessed;        

        public UpdateProgressWindow()
        {
            InitializeComponent();
            
            _numProcessed = 0;

            _worker.DoWork += OnDoWork;
            _worker.RunWorkerCompleted += OnCompleted;
            _worker.ProgressChanged += OnProgressChanged;            
            _worker.WorkerSupportsCancellation = true;
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
            int numContactsToSync = _mainWnd.NumberOfContactsToSync;
            Properties.Settings settings = Properties.Settings.Default;

            foreach (Contact contact in _mainWnd._matchedContacts)
            {
                if (_worker.CancellationPending)
                {
                    e.Cancel = true;
                    break;
                }

                if (!contact.IsIncludedInSync)
                {
                    continue;
                }

                Outlook.ContactItem outlookContact =
                    _mainWnd._outlookHelper.FindContactByEntryID(contact.OutlookId);

                if (outlookContact == null) continue;

                if (settings.UpdateBirthday)
                {
                }

                if (settings.UpdatePicture)
                {
                    string path = Path.Combine(Path.GetTempPath(),
                        Path.GetFileName(contact.PictureUrl.LocalPath));

                    if (File.Exists(path))
                    {
                        try
                        {
                            File.Delete(path);
                        }
                        catch
                        {
                        }
                    }

                    if (Utilities.FetchAndStoreImage(contact.PictureUrl, path))
                    {
                        outlookContact.AddPicture(path);
                    }

                    try
                    {
                        File.Delete(path);
                    }
                    catch
                    {
                    }
                }

                outlookContact.Save();
                Debug.WriteLine("Updated contact: " + outlookContact.FullName);

                double percentDone = ((float)++numProcessed / (float)numContactsToSync) * 100;
                _worker.ReportProgress((int)percentDone, contact);                
            }

            e.Result = numProcessed;
        }

        private void OnCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            _numProcessed = (int)e.Result;            
            DialogResult = e.Cancelled ? false : true;
        }

        private void OnProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            progressBar1.Value = e.ProgressPercentage;

            Contact contact = e.UserState as Contact;
            nameLabel.Content = Properties.Resources.Updating + contact.Name;
            image1.Source = new BitmapImage(contact.PictureUrl);
        }

        private void cancel_Click(object sender, RoutedEventArgs e)
        {
            cancelButton.IsEnabled = false;
            _worker.CancelAsync();
        }

        public int NumProcessed
        {
            get
            {
                return _numProcessed;
            }
        }
    }
}