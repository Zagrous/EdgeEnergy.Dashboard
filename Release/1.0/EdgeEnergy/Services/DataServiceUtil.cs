using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;

namespace EdgeEnergy.Services
{
    public partial class DataService 
    {
        #region Constants
        private readonly List<string> _startErrors = new List<string>
            {
                "No error",
                "Motor still changing",
                "Drive failed",
                "Speed too fast",
                "Speed too slow",
                "Direction change",
                "Supply low",
                "Already stopped",
                "",
                "",
                "Unknown"
            };

        private const int StartFault = 4;
        private const int StopFault = 8;



        private readonly Dictionary<string, int> _codes = new Dictionary<string, int>
            {
                {"DIRECTION", 1},
                {"SPEED_LOCKED", 2},
                {"START_FAULT", 4},
                {"STOP_FAULT", 8},
                {"RUNNING", 0x10},
                {"ERROR", 0x80}
            };


        private readonly List<string> _stopErrors = new List<string>
            {
                "No error",
                "Failed in start",
                "Lost lock",
                "Drive fail",
                "Speed too slow",
                "Low start current",
                "No rotation",
                "Current overload",
                "Overvoltage",
                "Winding fail",
                "High start power",
                "Stopped OK" //Make sure this stays as the last one!
            }; 
        #endregion

        #region Private Dump Methods
        public string RowToString(DataRow row)
        {
            var sb = new StringBuilder();

            foreach (DataColumn column in row.Table.Columns)
                sb.AppendFormat("{0}, ", row[column.ColumnName]);

            return sb.ToString();
        }

        public void DumpRow(string title, DataRow row)
        {
            Log.InfoFormat("{0}: {1}", title, RowToString(row));
        }

        public void DumpTableHeader(string title, DataTable dt)
        {
            if (dt == null) return;

            Log.InfoFormat("{0} {1}", title, dt.Rows.Count);
            foreach (DataColumn column in dt.Columns)
                Log.InfoFormat("   {0} {1} {2}", column.Ordinal, column.ColumnName, column.DataType);

            foreach (DataColumn column in dt.PrimaryKey)
                Log.InfoFormat("   PrimaryKey: {0} {1}", column.ColumnName, column.DataType);

        }

        public void DumpTableData(string title, DataTable dt, int count = -1)
        {
            if (dt == null) return;

            Log.InfoFormat("*** {0} RowCount={1}", title, dt.Rows.Count);

            var sb = new StringBuilder();

            foreach (DataColumn column in dt.Columns)
                sb.AppendFormat("{0}, ", column.ColumnName);

            Log.InfoFormat("{0}", sb);

            foreach (DataRow row in dt.Rows)
            {
                sb.Length = 0;
                foreach (DataColumn column in dt.Columns)
                    sb.AppendFormat("{0}, ", row[column.ColumnName]);

                Log.InfoFormat("{0}", sb);
                if (count != -1 && count-- <= 0) break;

            }
        }

        public void SaveTable(string filename, DataTable dt)
        {

            if (string.IsNullOrEmpty(ResultFile)) return;


            try
            {
                if (dt == null) return;

                //if (!Show(ServerUtilityCategories.SaveTable)) return;

                var sb = new StringBuilder();
                using (var sw = new StreamWriter(filename))
                {
                    foreach (DataColumn column in dt.Columns)
                    {
                        if (sb.Length > 0)
                            sb.AppendFormat(",");

                        sb.AppendFormat("{0}", column.ColumnName);
                    }
                    sw.WriteLine(sb.ToString());

                    foreach (DataRow row in dt.Rows)
                    {
                        sb.Length = 0;
                        foreach (DataColumn column in dt.Columns)
                        {
                            if (sb.Length > 0)
                                sb.AppendFormat(",");

                            sb.AppendFormat("{0}", row[column.ColumnName]);
                        }
                        sw.WriteLine(sb.ToString());

                    }
                }
            }
            catch (Exception ex)
            {
                Log.ErrorFormat("Exception {0}", ex);
            }
        }


        #endregion

    }
}
