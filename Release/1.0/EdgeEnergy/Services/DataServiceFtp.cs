using System;
using System.IO;
using System.Net;

namespace EdgeEnergy.Services
{
    public partial class DataService
    {
        public void StartFtp()
        {
            ProcessFtp(FtpUrl, DataFile, FtpUser, FtpPassword, FtpUsePassive, FtpTimeout);
        }

        public void StopFtp()
        {
            
        }

        // ftp:\\<ip>\<filename>
        public void ProcessFtp(string downloadUrl, string downloadFile, string user, string password, bool usePassive, int timeout)
        {
            Log.WarnFormat("Download start {0} {1} ", downloadUrl, downloadFile);
            Log.WarnFormat("Download start {0} {1} {2}", user, password, usePassive);

            Stream responseStream = null;
            FileStream fileStream = null;
            try
            {
                var downloadRequest = (FtpWebRequest)WebRequest.Create(downloadUrl);
                downloadRequest.Method = WebRequestMethods.Ftp.DownloadFile;

                downloadRequest.UsePassive = usePassive;
                downloadRequest.Timeout = timeout;
                // This example assumes the FTP site uses anonymous logon.
                if(!string.IsNullOrEmpty(user))
                    downloadRequest.Credentials = new NetworkCredential(user, password);

                var downloadResponse = (FtpWebResponse)downloadRequest.GetResponse();
                responseStream = downloadResponse.GetResponseStream();

                if (responseStream == null)
                    throw new ApplicationException(string.Format("Failed to Connect to {0}", downloadUrl));


                fileStream = File.Create(downloadFile);
                var buffer = new byte[1024];
                while (true)
                {
                    int bytesRead = responseStream.Read(buffer, 0, buffer.Length);
                    if (bytesRead == 0)
                        break;
                    fileStream.Write(buffer, 0, bytesRead);
                    _isFileTouched = true;
                }

                Log.WarnFormat("Download complete {0} {1}", downloadUrl, downloadFile);
            }
            catch (Exception ex)
            {
                Log.ErrorFormat(ex.Message);
                throw;
            }
            finally
            {
                if (responseStream != null)
                    responseStream.Close();
                if (fileStream != null)
                    fileStream.Close();
            }
        }
    }
}
