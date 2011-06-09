using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.IO;
using System.Diagnostics;
using System.Reflection;
using System.Windows;
using OutSync.CurrentVersionWebService;

namespace OutSync
{
    class Utilities
    {
        public static bool FetchAndStoreImage(Uri imageUrl, string localFilename)
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(imageUrl);
                request.Timeout = 30000; // 30 secs 
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                Stream webStream = response.GetResponseStream();

                System.Drawing.Image image = System.Drawing.Image.FromStream(webStream);
                Stream localStream = File.Create(localFilename);

                image.Save(localStream, System.Drawing.Imaging.ImageFormat.Jpeg);
                localStream.Close();

                webStream.Close();
                image.Dispose();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static void CheckForUpdate(Window parent, bool isSilent)
        {
            Version installedVersion = Assembly.GetExecutingAssembly().GetName().Version;
            CurrentVersion service = new CurrentVersion();

            string latestVersionString = String.Empty;

            try
            {
                latestVersionString = service.GetCurrentVersion(
                    installedVersion.ToString(4));
            }
            catch (System.Net.WebException)
            {
                if (!isSilent)
                {
                    MessageBox.Show(parent, Properties.Resources.UpdateFailedMessage,
                        Properties.Resources.CheckNewVersionCaption,
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }

                return;
            }

            Version availableVersion = new Version(latestVersionString);

            if (availableVersion > installedVersion)
            {
                if (MessageBox.Show(parent, Properties.Resources.NewVersionAvailableMessage,
                    Properties.Resources.CheckNewVersionCaption,
                    MessageBoxButton.YesNo, MessageBoxImage.Information) == MessageBoxResult.Yes)
                {
                    Process.Start(String.Format(
                        Properties.Resources.UpdateUrl, installedVersion.ToString(4)));
                }
            }
            else
            {
                if (!isSilent)
                {
                    MessageBox.Show(parent, Properties.Resources.NoNewVersionAvailableMessage,
                        Properties.Resources.CheckNewVersionCaption,
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }
    }
}