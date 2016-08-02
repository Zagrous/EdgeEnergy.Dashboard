using System;
using System.Collections.Generic;
using System.IO;

namespace EdgeEnergy.Services
{
    public partial class DataService
    {
        private static void CheckFileLock(string filename)
        {
            if (string.IsNullOrEmpty(filename)) return;

            var file = new FileInfo(filename);
            FileStream stream = null;

            try
            {
                stream = file.Open(FileMode.Open, FileAccess.ReadWrite, FileShare.None);
            }
            catch (IOException ex)
            {
                Log.ErrorFormat("Exception {0} {1}", filename, ex);
                throw new Exception(string.Format("{0}\nPlease close the file in other programs such as excel and try again.", ex.Message));
            }
            finally
            {
                if (stream != null)
                    stream.Close();
            }
        }

        private void WriteFile(string filename, string text)
        {
            using (var sw = new StreamWriter(filename, _isFileTouched))
            {
                _isFileTouched = true;
                sw.WriteLine(text);
            }
        }


        private static IEnumerable<string> ReadFile(string filename, bool skipHeader = true)
        {
            //Log.InfoFormat("ReadFile: {0}", filename);
            using (var sr = new StreamReader(filename))
            {
                String reader;
                if (skipHeader) sr.ReadLine(); // Readoff the header
                while ((reader = sr.ReadLine()) != null)
                {
                    //Log.Debug(reader);
                    yield return reader;
                }
            }
        }
    }
}
