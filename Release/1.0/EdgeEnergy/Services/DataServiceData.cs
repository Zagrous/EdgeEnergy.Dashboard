using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Threading;

namespace EdgeEnergy.Services
{
    public partial class DataService 
    {
        #region Set Methods

        private static void SetString(string[] parts, CutterDataHeader dataHeader, DataRow row)
        {
            row[dataHeader.Index] = parts[dataHeader.Index].Trim();
        }

        private void SetTime(string[] parts, CutterDataHeader dataHeader, DataRow row)
        {
            var data = parts[dataHeader.Index].Trim();
            if (!string.IsNullOrEmpty(data))
            {
                _lastTime = new DateTime(TimeSpan.Parse(data).Ticks);
                if (_firstTime == DateTime.MinValue)
                    _firstTime = _lastTime;
            }

            row[dataHeader.Index] = _lastTime;
        }

        private void SetDate(string[] parts, CutterDataHeader dataHeader, DataRow row)
        {
            var data = parts[dataHeader.Index].Trim();
            if (!string.IsNullOrEmpty(data))
            {
                DateTime date;
                if (DateTime.TryParseExact(data, DateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out date) ||
                    DateTime.TryParseExact(data, "yyyy/MM/dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out date) ||
                    DateTime.TryParseExact(data, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out date))
                {
                    _lastDate = date;
                    if (_firstDate == DateTime.MinValue)
                        _firstDate = _lastDate;
                }
            }
            row[dataHeader.Index] = _lastDate;
        }

        private DateTime _lastRealtimeDate = DateTime.MinValue;
        private DateTime _lastRealtimeTime = DateTime.MinValue;

        private void SetTimeRealtime(string[] parts, CutterDataHeader dataHeader, DataRow row)
        {
            var data = parts[dataHeader.Index].Trim();
            if (!string.IsNullOrEmpty(data))
            {
                _lastRealtimeTime = new DateTime(TimeSpan.Parse(data).Ticks);
                row[dataHeader.Index] =_lastRealtimeTime;
                
                return;
            }

            if (_lastRealtimeTime == DateTime.MinValue)
                throw new IndexOutOfRangeException("Invalid Last Time");

            row[dataHeader.Index] = _lastRealtimeTime;
        }

        private void SetDateRealtime(string[] parts, CutterDataHeader dataHeader, DataRow row)
        {
            var data = parts[dataHeader.Index].Trim();
            if (!string.IsNullOrEmpty(data))
            {
                DateTime date;
                if (DateTime.TryParseExact(data, DateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out date) ||
                    DateTime.TryParseExact(data, "yyyy/MM/dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out date) ||
                    DateTime.TryParseExact(data, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out date))
                {
                    _lastRealtimeDate = date;
                    row[dataHeader.Index] = date;
                    return;
                }
            }

            if( _lastRealtimeDate == DateTime.MinValue )
                throw new IndexOutOfRangeException("Invalid Last Date");

            row[dataHeader.Index] = _lastRealtimeDate;
        }


        private CutterData SetDouble(CutterDataHeader dataHeader, DataRow row, CutterData lastData)
        {
            var valueString = row.Field<string>(dataHeader.Legend);
            
            if (string.IsNullOrEmpty(valueString))
            {
                if (lastData == null) return null;

                valueString = lastData.ValueString;
            }

            return new CutterData
            {
                Date = row.Field<DateTime>("Date"),
                Value = Double.Parse(valueString),
                ValueString = valueString
            };
        }

        private CutterData SetString(CutterDataHeader dataHeader, DataRow row)
        {
            var valueString = row.Field<string>(dataHeader.Legend);

            if (string.IsNullOrEmpty(valueString)) return null;

            var data = new CutterData
            {
                Date = row.Field<DateTime>("Date"),
                Value = 0.0,
                ValueString = valueString
            };

            return data;
        }

        private CutterData SetError(CutterDataHeader dataHeader, DataRow row)
        {
            var valueString = row.Field<string>(dataHeader.Legend);

            if (string.IsNullOrEmpty(valueString)) return null;

            int value = int.Parse(valueString);
            if (value == 0) return null;

            // Status index is one less than the Error Index
            CutterDataHeader statusHeader = null;
            foreach (var header in _dataHeaders)
            {
                if (header.Value.Index == dataHeader.Index - 1)
                {
                    statusHeader = header.Value;
                    break;

                }
            }

            if (statusHeader == null) return null;

            var statusString = row.Field<string>(statusHeader.Legend);
            if (string.IsNullOrEmpty(statusString)) return null;
            int status = int.Parse(statusString);
            if (status == 0) return null;

            var error = "Unknown";
            if ((status & StartFault) == StartFault)
                error = _startErrors[value];
            else if ((status & StopFault) == StopFault)
                error = _stopErrors[value];

            try
            {
                var data = new CutterData
                {
                    Date = row.Field<DateTime>("Date"),
                    Value = Double.Parse(valueString),
                    ValueString = error
                };

                return data;
            }
            catch (Exception)
            {
                Log.ErrorFormat("Invalid Error code for {0} = {1}", dataHeader.Legend, value);
            }

            return null;
        }

        private CutterData SetStatus(CutterDataHeader dataHeader, DataRow row)
        {
            var valueString = row.Field<string>(dataHeader.Legend);

            if (string.IsNullOrEmpty(valueString)) return null;

            int value = int.Parse(valueString);
            if (value == 0) return null;

            var list = new List<string>();
            foreach (var code in _codes)
            {
                if ((value & code.Value) == code.Value)
                    list.Add(code.Key);
            }

            if (list.Count > 0)
            {
                return new CutterData
                {
                    Date = row.Field<DateTime>("Date"),
                    Value = value,
                    ValueString = string.Join(", ", list)
                };

            }

            return null;
        }

        #endregion


        private void SetDataLog(string data)
        {
            if (!string.IsNullOrEmpty(_dataLog)) return;

            var parts = data.Split(',');
            if (parts.Length < 4) return;

            _dataLog = string.Format("{0}  {1}  {2}", parts[1], parts[2], parts[3]);

            OnStatus(DataStatus.DataLog, _dataLog);
        }

        private DataRow AddRow(string data, bool realtime = false)
        {
            if (string.IsNullOrEmpty(data)) return null;

            if (data.IndexOf("Time", StringComparison.OrdinalIgnoreCase) >= 0 &&
                data.IndexOf("Commands", StringComparison.OrdinalIgnoreCase) >= 0) return null;

            if (data.IndexOf("Datalog", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                SetDataLog(data);
                return null;
            }

            CutterDataHeader dataHeader = null;
            try
            {
                //Log.InfoFormat("AddRow Data: {0}", data);

                var row = _table.NewRow();

                var parts = data.Split(',');

                foreach (var header in _dataHeaders)
                {
                    dataHeader = header.Value;
                    if (realtime)
                        SetDataRealtime(dataHeader, parts, row);
                    else
                        SetData(dataHeader, parts, row);
                }

                if (realtime)
                    AdjustRow(row);

                if (Verbose) DumpRow("AddRow", row);

                lock (_tableSync)
                {
                    _table.Rows.Add(row);
                }

                return row;
            }
            catch (IndexOutOfRangeException)
            {
                Log.WarnFormat("Missing Last Date or Time: {0}", data);
            }
            catch (Exception)
            {
                Error++;
                if (dataHeader != null)
                    Log.WarnFormat("Exception Column: {0}", dataHeader);
                Log.WarnFormat("Exception Data: {0}", data);
                //throw;
            }

            return null;
        }

        private void SetData(CutterDataHeader dataHeader, string[] parts, DataRow row)
        {
            switch (dataHeader.Type)
            {
                case "Date":
                    SetDate(parts, dataHeader, row);
                    break;
                case "Time":
                    SetTime(parts, dataHeader, row);
                    break;
                default:
                    SetString(parts, dataHeader, row);
                    break;
            }
        }

        private void SetDataRealtime(CutterDataHeader dataHeader, string[] parts, DataRow row)
        {
            switch (dataHeader.Type)
            {
                case "Date":
                    SetDateRealtime(parts, dataHeader, row);
                    break;
                case "Time":
                    SetTimeRealtime(parts, dataHeader, row);
                    break;
                default:
                    SetString(parts, dataHeader, row);
                    break;
            }
        }


        private void AdjustRow(DataRow row)
        {
            var date = row.Field<DateTime>(0);
            var time = row.Field<DateTime>(1);
            var datetime = new DateTime(date.Year, date.Month, date.Day, time.Hour, time.Minute, time.Second);//.AddDays(count++);
            row[0] = datetime;
            row[1] = datetime;
        }

        private void LoadHeader()
        {
            try
            {
                foreach (string data in ReadFile(HeaderFile))
                {
                    if (string.IsNullOrEmpty(data)) continue;
                    Log.Info(data);

                    var parts = data.Split(',');
                    if (parts.Length < 7)
                        throw new ApplicationException("Incorrect Header file");

                    if (string.IsNullOrEmpty(parts[0])) continue;

                    var header = new CutterDataHeader(parts, _table);
                    _dataHeaders.Add(header.Legend, header);
                }

                DumpTableHeader("Header", _table);
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("{0}\nPlease contact help desk.", ex.Message));
            }
        }


        private void AdjustRows()
        {

            if (_firstDate == DateTime.MinValue)
                _firstDate = DateTime.Now.Date;

            foreach (DataRow row in _table.Rows)
            {
                var date = row.Field<DateTime>(0);
                if (date == DateTime.MinValue)
                    date = _firstDate;

                var time = row.Field<DateTime>(1);
                if (time == DateTime.MinValue)
                    time = _firstTime;

                var datetime = new DateTime(date.Year, date.Month, date.Day, time.Hour, time.Minute, time.Second);//.AddDays(count++);
                row[0] = datetime;
                row[1] = datetime;
            }
        } 

        private void LoadData()
        {
            try
            {
                if (string.IsNullOrEmpty(DataFile)) return;

                foreach (string data in ReadFile(DataFile))
                {
                    if (Verbose) Log.InfoFormat("Load: {0}", data);
                    AddRow(data);
                }

                AdjustRows();

                SaveTable(ResultFile, _table);
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("Incorrect Data file.\nPlease contact help desk.\n\n{0}", ex.Message));
            }
        }

        private void AddData(string data)
        {
            var row = AddRow(data, true);
            if (row == null)
                return;

            lock (_subscriptionSync)
            {
                foreach (var subscription in _subscriptions)
                {
                    PublishUpdate(subscription, row);
                }
            }
        }
        
        private CutterData ProcessRow( CutterSubscription subscription  , DataRow row)
        {
            var dataHeader = subscription.DataHeader;
            CutterData data = null;
            try
            {
                switch (dataHeader.Type)
                {
                    case "Double":
                        data = SetDouble(dataHeader, row, subscription.LastData);
                        break;
                    case "Error":
                        data = SetError(dataHeader, row);
                        break;
                    case "Status":
                        data = SetStatus(dataHeader, row);
                        break;
                    case "String":
                        data = SetString(dataHeader, row);
                        break;
                }

                subscription.LastData = data;

                if (Verbose) Log.InfoFormat("ProcessRow: {0}", data);

            }
            catch (Exception ex)
            {
                Error++;
                if (dataHeader != null)
                    Log.WarnFormat("Exception Column: {0}", dataHeader);
                DumpRow("Exception", row);
                Log.WarnFormat("Exception Data: {0}", ex);
            }
            return data;
        }

        private void PublishSnapshot(CutterSubscription subscription)
        {
            Log.InfoFormat("$$$$ PublishSnapshot {0} ", subscription.Symbol);
            if (_uiContext == null) return;
            
            var series = new List<CutterData>();
            lock (_tableSync)
            {
                foreach (DataRow row in _table.Rows)
                {
                    var cutterData = ProcessRow(subscription, row);
                    if (cutterData == null) continue;


                    //var time = cutterData.Date.Second;
                    //cutterData.Date = DateTime.Now.AddSeconds(time);

                    series.Add(cutterData);
                }  
            }

            Log.InfoFormat("$$$$ PublishSnapshot {0}  Count={1}", subscription.Symbol, series.Count);

            //subscription.SnapshotHandler(subscription.Symbol, series);
            
            // SynchronizationContext.Current.Post(x => subscription.SnapshotHandler(subscription.Symbol, (IEnumerable<CutterData>)x), series);
            _uiContext.Post(x => subscription.SnapshotHandler(subscription.Symbol, (IEnumerable<CutterData>)x), series);

            subscription.DeliverRealtime = true;

        }

        private void PublishUpdate(CutterSubscription subscription, DataRow row)
        {
            if (_uiContext == null) return;
            if (!subscription.DeliverRealtime) return;

            var cutterData = ProcessRow(subscription, row);
            if (cutterData == null)
                return;

            //subscription.UpdateHandler(subscription.Symbol, cutterData);
            //SynchronizationContext.Current.Post(x => subscription.UpdateHandler(subscription.Symbol, (CutterData)x), cutterData);
            _uiContext.Post(x => subscription.UpdateHandler(subscription.Symbol, (CutterData)x), cutterData);
        }
    }

}
