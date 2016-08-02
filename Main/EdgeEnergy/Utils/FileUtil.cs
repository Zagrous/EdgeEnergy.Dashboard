using System;
using System.Configuration;
using System.IO;
using System.Reflection;

namespace EdgeEnergy.CutterDashboard.Utils
{
    public static class FileUtil
    {
        public static string GetFtpFile()
        {
            var filename = string.Format(@"{0}\{1}", GetDataPath(), ConfigurationManager.AppSettings.Get("FtpFilename"));

            //if( File.Exists(filename))
            //    File.Delete(filename);

            return filename ;
        }

        public static string GetTcpFile()
        {
            var filename = string.Format(@"{0}\{1}", GetDataPath(), ConfigurationManager.AppSettings.Get("TcpFilename"));

            //if (File.Exists(filename))
            //    File.Delete(filename);

            return filename;
        }

        public static string GetFtpUrl(string host)
        {
            var filename = ConfigurationManager.AppSettings.Get("FtpFilename");
            return string.Format("ftp://{0}/{1}", host, filename);
        }

        public static string GetDataPath()
        {
            return string.Format(@"{0}\EdgeEnergy\Data", Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) );
        }

        public static string GetCurrentPath()
        {
            return Directory.GetCurrentDirectory();
        }

        public static void SetCurrentPath()
        {
            if ( String.CompareOrdinal(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), GetDataPath()) == 0 )
                Directory.SetCurrentDirectory( GetDataPath());
        }

        public static string GetDataFile(string filename)
        {
            return string.Format(@"{0}\{1}", GetDataPath(), filename);
        }

        public static string GetConfigFile(string filename)
        {
            var headerfile = filename.Replace("Data.csv", "Header.csv");
            if (!File.Exists(headerfile) || String.Compare(filename, headerfile, StringComparison.OrdinalIgnoreCase) == 0)
            {
                //headerfile = string.Format( @"{0}\Config\HeaderConfig.csv", Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));
                headerfile = string.Format(@"{0}\HeaderConfig.csv", GetDataPath());
            }
            return headerfile;
        }

        public static string GetLogFile()
        {
            var dir = string.Format(@"{0}\EdgeEnergy\Log", Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments));

            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            var filename = string.Format(@"{0}\Dashboard.log", dir);

            if (!File.Exists(filename))
                File.Create(filename).Close();

            return filename;
        }

        public static string GetResultFile()
        {

            var verbose = bool.Parse(ConfigurationManager.AppSettings.Get("Verbose"));
            if (!verbose) return String.Empty;

            var dir = string.Format(@"{0}\EdgeEnergy\Log", Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments));

            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            var filename = string.Format(@"{0}\Result.csv", dir);

            if (!File.Exists(filename))
            {
                var file = File.Create(filename);
                file.Close();
            }

            return filename;
        }

        public static string OpenFile()
        {
            var filename = string.Empty;
            var openFileDialog = new Microsoft.Win32.OpenFileDialog
            {
                FileName = "Data",
                DefaultExt = ".csv",
                Filter = "CSV documents (.csv)|*.csv"
            };
            var result = openFileDialog.ShowDialog();
            if (result == true)
            {
                filename = openFileDialog.FileName;
            }

            return filename;
        }

        public static string SaveFile(string filename)
        {
            if (string.IsNullOrEmpty(filename)) return filename;

            //var info = new FileInfo(filename);
            //if (info.Length == 0) return filename;
            
            var saveFile = string.Format("{0}.csv", DateTime.Now.ToString("yyyyMMdd-HHmm"));
            var saveFileDialog = new Microsoft.Win32.SaveFileDialog
            { 
                Title = "Save Current data to File",
                FileName = saveFile,
                DefaultExt = ".csv",
                Filter = "CSV documents (.csv)|*.csv"
            };
            var result = saveFileDialog.ShowDialog();
            if (result == true)
            {

                if (File.Exists(saveFileDialog.FileName))
                    File.Delete(saveFileDialog.FileName);

                File.Copy(filename, saveFileDialog.FileName, true);

                return saveFileDialog.FileName;
            }

            return null;
        }


    }
}
