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
using System.Collections;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Diagnostics;
using System.IO;
using System.ComponentModel;
using System.Windows.Interop;

using Outlook = Microsoft.Office.Interop.Outlook;

using facebook;
using facebook.Components;
using System.Windows.Controls.Primitives;

namespace OutSync
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>

    public partial class MainWindow : System.Windows.Window
    {
        public OutlookHelper _outlookHelper = new OutlookHelper();
        public FacebookService _facebookService = new FacebookService();

        public ArrayList _normalizedContacts = new ArrayList();

        // the following 2 arrays will each contain a subset of _normalizedContacts
        public ArrayList _matchedContacts = new ArrayList();
        public ArrayList _unmatchedContacts = new ArrayList();

        public MainWindow()
        {
            InitializeComponent();

            _facebookService.ApplicationKey = Properties.Resources.FBAppKey;

            _facebookService.Secret = Properties.Resources.FBSecret;
            _facebookService.IsDesktopApplication = true;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
        #if (!DEBUG)
            if (Properties.Settings.Default.AutoCheckUpdate)
            {
                Utilities.CheckForUpdate(this, true);
            }        
        #endif

            try
            {
                // get these out of the way. without logging into Facebook
                // and a valid Outlook session, there is no point in running
                // this app.

                _outlookHelper.Initialize();
                _facebookService.ConnectToFacebook();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    this,
                    ex.Message,
                    Properties.Resources.ErrorDialogCaption,
                    MessageBoxButton.OK,
                    MessageBoxImage.Error,
                    MessageBoxResult.OK,
                    MessageBoxOptions.None);
                
                Application.Current.Shutdown();
                return;
            }

            // Kick off a thread to download FB friends and match them  
            // to Outlook Contacts 
            MatchingProgressWindow window = new MatchingProgressWindow();
            window.ShowDialog();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _outlookHelper.Uninitialize();

            rememberExcludedContacts();
            Properties.Settings.Default.Save();
        }

        public void RefreshListBoxes()
        {
            matchedContactsListBox.ShowContacts(true);
            matchedContactsLabel.Content = String.Format(
                Properties.Resources.MatchedContactsHeader, _matchedContacts.Count);

            unmatchedContactsListBox.ShowContacts(false);
            unmatchedContactsLabel.Content = String.Format(
                Properties.Resources.UnmatchedContactsHeader, _unmatchedContacts.Count);

            if (_matchedContacts.Count == 0)
            {
                MessageBox.Show(
                    this, 
                    Properties.Resources.NoMatchesMessage, 
                    Properties.Resources.NoMatchesCaption, 
                    MessageBoxButton.OK, 
                    MessageBoxImage.Information, 
                    MessageBoxResult.OK, 
                    MessageBoxOptions.None);
                   
                syncButton.IsEnabled = false;
            }
            else
            {
                syncButton.IsEnabled = true;
            }
        }

        public int NumberOfContactsToSync
        {
            get
            {
                int count = 0;

                foreach (Contact contact in _matchedContacts)
                {
                    if (contact.IsIncludedInSync) count++;
                }

                return count;
            }
        }

        private void syncButton_Click(object sender, RoutedEventArgs e)
        {
            if (NumberOfContactsToSync == 0)
            {
                MessageBox.Show(
                    Properties.Resources.NoContactsToSyncMessage,
                    Properties.Resources.SyncContactsCaption,
                    MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return;
            }

            if (MessageBox.Show(String.Format(
                Properties.Resources.SyncConfirmationMessage, NumberOfContactsToSync),
                Properties.Resources.SyncContactsCaption, MessageBoxButton.YesNo,
                MessageBoxImage.Question) == MessageBoxResult.No) return;

            UpdateProgressWindow window = new UpdateProgressWindow();

            try
            {
                if (window.ShowDialog() == true)
                {
                    MessageBox.Show(String.Format(
                        Properties.Resources.Updated, window.NumProcessed),
                        Properties.Resources.SyncContactsCaption,
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (InvalidOperationException)
            {
            }
        }

        private void exitButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void hyperlink_Click(object sender, RoutedEventArgs e)
        {
            foreach (Contact contact in _matchedContacts)
            {
                if (sender == checkAllHyperlink)
                {
                    contact.IsIncludedInSync = true;
                }

                if (sender == checkNoneHyperlink)
                {
                    contact.IsIncludedInSync = false;
                }
            }

            // HACK: I can't for the life of me figure out how to update the 
            // state of the checkboxes in the listbox after the above Binding 
            // source has been updated. 

            // The only way I can do it, for now, is to unbind the listbox and
            // then re-bind it, which refreshes the state of the checkboxes.

            // I probably need to fix the below code at some point when I 
            // understand databinding better.

            BindingOperations.ClearBinding(
                matchedContactsListBox.listBox1, ListBox.ItemsSourceProperty);

            matchedContactsListBox.ShowContacts(true);
        }

        private void rememberExcludedContacts()
        {
            foreach (Contact contact in _matchedContacts)
            {
                if (contact.IsIncludedInSync)
                {
                    Properties.Settings.Default.ExclusionList.Remove(contact.OutlookId);
                }
                else
                {
                    Properties.Settings.Default.ExclusionList.Add(contact.OutlookId);
                }
            }
        }

        #region Code for the Tools menu button
        private void toolsButton_Click(object sender, RoutedEventArgs e)
        {
            if (toolsButton.ContextMenu != null)
            {
                toolsButton.ContextMenu.PlacementTarget = toolsButton;
                toolsButton.ContextMenu.Placement = PlacementMode.Bottom;
                ContextMenuService.SetPlacement(toolsButton, PlacementMode.Bottom);
                toolsButton.ContextMenu.IsOpen = true;
            }
        }

        private void toolsButtonContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            toolsButton.ContextMenu.IsOpen = false;
            e.Handled = true;
        }

        private void settings_Click(object sender, RoutedEventArgs e)
        {
            SettingsWindow window = new SettingsWindow();
            window.ShowDialog();
        }

        private void removePictures_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show(Properties.Resources.RemovePicturesWarning,
                Properties.Resources.RemovePicturesCaption, MessageBoxButton.YesNo,
                MessageBoxImage.Stop, MessageBoxResult.No) == MessageBoxResult.No)
            {
                return;
            }

            Cursor = Cursors.Wait;
            _outlookHelper.RemoveAllContactPictures();
            Cursor = Cursors.Arrow;

            MessageBox.Show(Properties.Resources.RemovePicturesDoneMessage,
                Properties.Resources.RemovePicturesCaption,
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void checkForUpdate_Click(object sender, RoutedEventArgs e)
        {
            Utilities.CheckForUpdate(this, false);
        }

        #endregion
 
        #region Aeroglass stuff
        private const int WM_DWMCOMPOSITIONCHANGED = 0x031E;

        private void Window_SourceInitialized(object sender, EventArgs e)
        {
            Brush bkBrush = Resources["backgroundBrush"] as Brush;

            if (Properties.Settings.Default.UseAeroGlass && GlassHelper.IsAeroGlassEnabled)
            {
                contentGrid.Background = bkBrush;

                // This can't be done any earlier than the SourceInitialized event:
                recalcGlassArea();

                // Attach a window procedure in order to detect later enabling of desktop composition
                IntPtr hwnd = new WindowInteropHelper(this).Handle;
                HwndSource.FromHwnd(hwnd).AddHook(new HwndSourceHook(WndProc));
            }
            else
            {
                uberPanel.Background = bkBrush;
            }
        }

        private void recalcGlassArea()
        {
            GlassHelper.ExtendGlassFrame(this, new Thickness(0,
                LogoHeader.ActualHeight, 0, toolbarPanel.ActualHeight + 1));
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == WM_DWMCOMPOSITIONCHANGED)
            {
                // Reenable glass:
                GlassHelper.ExtendGlassFrame(this, new Thickness(-1));
                handled = true;
            }

            return IntPtr.Zero;
        }

        private void Grid_LayoutUpdated(object sender, EventArgs e)
        {
            recalcGlassArea();
        }
        #endregion
    }
}