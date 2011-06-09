using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using facebook.Schema;
using System.Diagnostics;

namespace OutSync
{
    public class Contact : IComparable
    {
    #region Private members
        string _name;
        string _networks;
        string _status;

        Uri _pictureUrl;
        bool _isMatched;
        bool _isIncludedInSync;

        //string _facebookId;
        string _outlookId;
        #endregion

        public Contact()
        {
            _isMatched = false;
            _isIncludedInSync = true;
        }

    #region Facebook properties
        //public string FacebookId
        //{
        //    get
        //    {
        //        return _facebookId;
        //    }
        //}

        public string Name
        {
            get
            {
                return _name;
            }
        }

        public string Networks
        {
            get
            {
                return _networks;
            }
        }

        public string Status
        {
            get
            {
                return _status;
            }
        }

        public Uri PictureUrl
        {
            get
            {
                return _pictureUrl;
            }
        }
    #endregion

    #region General properties
        public bool IsMatched
        {
            get
            {
                return _isMatched;
            }
            set
            {
                _isMatched = value;
            }
        }

        public bool IsIncludedInSync
        {
            get
            {
                return _isIncludedInSync;
            }
            set
            {
                _isIncludedInSync = value;
            }
        }
    #endregion

    #region Outlook properties
        public string OutlookId
        {
            get
            {
                return _outlookId;
            }
            set
            {
                _outlookId = value;
            }
        }
    #endregion

        public int CompareTo(object obj)
        {
            Contact contact = (Contact)obj;
            return string.Compare(_name, contact.Name);
        }

        /// <summary>
        /// Populates internal fields such as name, status, networks etc. 
        /// with information about the given Facebook user.
        /// </summary>
        /// <param name="user"></param>
        public void SetFaceBookInfo(user user)
        {
            //_facebookId = user.uid;
            _name = user.name;
            _status = user.status.message;            
            _networks = String.Empty;

            foreach (facebook.Schema.affiliation aff in user.affiliations.affiliation)
            {
                _networks += Environment.NewLine + aff.name;
            }

            _networks = _networks.Trim();    
    
            try
            {
                _pictureUrl = new Uri(user.pic_big);                
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }            
        }
    }
}