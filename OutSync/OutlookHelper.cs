using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Reflection;
using Outlook = Microsoft.Office.Interop.Outlook;
using System.Diagnostics;

namespace OutSync
{
    public class OutlookHelper
    {
        Outlook.Application _olApp;
        Outlook.NameSpace _olNS;
        Outlook.MAPIFolder _olContactsFolder;
        bool _isInitialized;

        public OutlookHelper()
        {
            _isInitialized = false;
            _olApp = new Outlook.Application();            
        }

        ~OutlookHelper()
        {
            Uninitialize();
        }

        /// <summary>
        /// Initalizes and logs into an Outlook session. Must call this 
        /// before using any of the other methods of this class.
        /// </summary>
        public void Initialize()
        {
            if (_isInitialized)
            {
                // Already initialized. Return silently.
                return;
            }

            _olNS = _olApp.GetNamespace("MAPI");

            _olNS.Logon(Missing.Value, Missing.Value, Missing.Value, true);

            _olContactsFolder = _olNS.GetDefaultFolder(
                Outlook.OlDefaultFolders.olFolderContacts);

            _isInitialized = true;
        }

        public void Uninitialize()
        {
            if (_isInitialized)
            {
                try
                {
                    _olNS.Logoff();
                }
                catch
                {
                }

                _olContactsFolder = null;
                _olNS = null;
                _olApp = null;
            }
        }

        public Outlook.MAPIFolder ContactsFolder
        {
            get
            {
                return _olContactsFolder;
            }
        }

        /// <summary>
        /// Returns a ContactItem that matches the given entryID.
        /// </summary>
        /// <param name="entryID"></param>
        /// <returns></returns>
        public Outlook.ContactItem FindContactByEntryID(string entryID)
        {
            if (!_isInitialized)
            {
                throw new Exception("Outlook has not been initialized.");
            }

            if (String.IsNullOrEmpty(entryID)) return null;

            object result = _olNS.GetItemFromID(entryID, _olContactsFolder.StoreID);
            if (result != null)
            {
                return (Outlook.ContactItem)result;
            }
            else return null;
        }

        /// <summary>
        /// Removes the picture from all Outlook contacts that have one. This is for
        /// debugging and testing only.
        /// </summary>
        public int RemoveAllContactPictures()
        {
            if (!_isInitialized)
            {
                throw new Exception("Outlook has not been initialized.");
            }

            int count = 0;

            foreach (Outlook.ContactItem contact in _olContactsFolder.Items)
            {
                if (contact.HasPicture)
                {
                    try
                    {
                        contact.RemovePicture();
                        contact.Save();
                        Debug.WriteLine("Removed picture from contact: " + contact.FullName);
                        count++;
                    }
                    catch
                    {
                    }
                }
            }

            return count;
        } 
    }
}