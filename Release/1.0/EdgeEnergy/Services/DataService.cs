using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using log4net;

namespace EdgeEnergy.Services
{
    public partial class DataService : IDataService
    {
        #region Private Properties
        private static readonly ILog Log = LogManager.GetLogger("root");

        readonly Dictionary<string, CutterDataHeader> _dataHeaders = new Dictionary<string, CutterDataHeader>();
        
        private readonly object _tableSync = new object();
        readonly DataTable _table = new DataTable();

        private DateTime _lastDate = DateTime.MinValue;
        private DateTime _lastTime = DateTime.MinValue;

        private DateTime _firstDate = DateTime.MinValue;
        private DateTime _firstTime = DateTime.MinValue;

        private readonly object _subscriptionSync = new object();
        private readonly List<CutterSubscription> _subscriptions = new List<CutterSubscription>();
        private SynchronizationContext _uiContext;
        private bool _isFileTouched;

        private string _dataLog ;

        //private bool _processing;
        //private Thread _tcpThread;
        #endregion

        private void OnStatus(DataStatus status, string message = "")
        {

            if (Verbose) Log.FatalFormat("OnStatus {0} {1}", status, message);

           // Application.Current.Dispatcher.Invoke(new Action(() => { /* Your code here */ }));

            //if (StatusEvent != null) StatusEvent(this, new StatusEventArgs {Status = status, Message = message});
            if (StatusEvent == null) return;

            var args = new StatusEventArgs { Status = status, Message = message };


            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
            {
                if (Verbose) Log.FatalFormat("OnStatus 2");
                StatusEvent(this, args);
            }));

        }

        public DataService()
        {
            TcpHeatbeatMessage = "HB";
            TcpHeatbeatInterval = 2;
            Verbose = bool.Parse(ConfigurationManager.AppSettings.Get("Verbose"));
            DateFormat = ConfigurationManager.AppSettings.Get("DateFormat");

            Log.InfoFormat("Thread.CurrentThread.CurrentCulture.Name={0}", Thread.CurrentThread.CurrentCulture.Name);
        }

        public void Subscribe(string symbol, Action<string, IEnumerable<CutterData>> snapshotHandler, Action<string, CutterData> updateHandler)
        {
            CutterDataHeader dataHeader;
            if (!_dataHeaders.TryGetValue(symbol, out dataHeader))
                throw new ApplicationException(string.Format("Invalid Symbol {0}", symbol));

            var subscription = new CutterSubscription
                {
                    Symbol = symbol,
                    DataHeader = dataHeader,
                    SnapshotHandler = snapshotHandler,
                    UpdateHandler = updateHandler
                };

            Log.InfoFormat("$$$$ Subscribe {0} ", symbol);

            lock (_subscriptionSync)
            {
                //OnStatus(DataStatus.SplashBegin);
                //OnStatus(DataStatus.SplashDisplay, string.Format("Loading {0} ...", symbol));

                PublishSnapshot(subscription);

                _subscriptions.Add(subscription);

                //OnStatus(DataStatus.SplashEnd);
            }
        }

        public void Unsubscribe(string symbol)
        {
            lock (_subscriptionSync)
            {
                var subscription = _subscriptions.Find(s => s.Symbol == symbol);
                if (subscription != null)
                 _subscriptions.Remove(subscription);
            } 
        }

        public string GetColour(string symbol)
        {
            CutterDataHeader dataHeader;
            if (!_dataHeaders.TryGetValue(symbol, out dataHeader))
                throw new ApplicationException(string.Format("Invalid Symbol {0}", symbol));

            return dataHeader.Colour;
        }

        public string GetTitle()
        {
            //return _lastDate.ToString("dd MMM yyyy");

            var date = _lastDate;
            if (date == DateTime.MinValue)
                date = DateTime.Now;

            return string.Format("Edge Energy Dashboard - {0} : {1}", date.ToString("dd MMM yyyy"), DataFile);

        }

        public IEnumerable<string> GetDefaultLegends()
        {
            return from header in _dataHeaders
                   select header.Value into dataHeader
                   where dataHeader.Default
                   select dataHeader.Legend;
        }

        public IEnumerable<string> GetAvailableSymbols()
        {
            return from header in _dataHeaders
                   select header.Value into dataHeader
                   where dataHeader.Available
                   select dataHeader.Legend;
        }

        public bool IsServiceType(DataServiceType serviceType)
        {
            return ((ServiceType & serviceType) == serviceType);
        }

        public bool IsFileTouched()
        {
            return _isFileTouched;
        }

        public void Initialise()
        {
            Log.InfoFormat("ServiceType:    {0}", ServiceType);

            Log.InfoFormat("HeaderFile:     {0}", HeaderFile);
            Log.InfoFormat("DataFile:       {0}", DataFile);
            Log.InfoFormat("ResultFile:     {0}", ResultFile);

            Log.InfoFormat("FtpUser:        {0}", FtpUser);
            Log.InfoFormat("FtpPassword:    {0}", FtpPassword);
            Log.InfoFormat("FtpUsePassive:  {0}", FtpUsePassive);
            Log.InfoFormat("FtpTimeout:     {0}", FtpTimeout);
            Log.InfoFormat("FtpUrl:         {0}", FtpUrl);

            Log.InfoFormat("TcpHost:        {0}", TcpHost);
            Log.InfoFormat("TcpPort:        {0}", TcpPort);
            Log.InfoFormat("TcpHBMessage:   {0}", TcpHeatbeatMessage);
            Log.InfoFormat("TcpHBInterval:  {0}", TcpHeatbeatInterval);

            Log.InfoFormat("Verbose:        {0}", Verbose);
            Log.InfoFormat("DateFormat:     {0}", DateFormat);

            CheckFileLock(HeaderFile);
            CheckFileLock(DataFile);
            CheckFileLock(ResultFile);

            _uiContext = SynchronizationContext.Current;

            OnStatus(DataStatus.SplashBegin);
            
            OnStatus(DataStatus.SplashDisplay, "Loading header...");
            LoadHeader();


            if ( IsServiceType( DataServiceType.Ftp) )
            {
                OnStatus(DataStatus.SplashDisplay, "Downloading file...");
                StartFtp();

                ServiceType |= DataServiceType.File;
            }

            if  (!string.IsNullOrEmpty(DataFile) && IsServiceType( DataServiceType.File) )
            {
                OnStatus(DataStatus.SplashDisplay, "Loading data...");
                LoadData();
            }

            if (IsServiceType(DataServiceType.Tcp))
            {
                OnStatus(DataStatus.SplashDisplay, "Connecting...");
                StartTcp();
            }

            OnStatus(DataStatus.SplashEnd);
        }

        public void Start()
        {

      
        }

        public void Stop()
        {
            if (ServiceType == DataServiceType.Ftp)
                StopFtp();

            if (ServiceType == DataServiceType.Tcp)
                StopTcp();
        }

        public event EventHandler<StatusEventArgs> StatusEvent;

        public string HeaderFile { get; set; }
        public string DataFile { get; set; }
        public string ResultFile { get; set; }

        public DataServiceType ServiceType { get; set; }

        public string FtpUser { get; set; }
        public string FtpPassword { get; set; }
        public bool FtpUsePassive { get; set; }
        public int FtpTimeout { get; set; }
        public string FtpUrl { get; set; }

        public string TcpHost { get; set; }
        public int TcpPort { get; set; }
        public string TcpHeatbeatMessage { get; set; }
        public int TcpHeatbeatInterval { get; set; }

        public bool Verbose { get; set; }
        public string DateFormat { get; set; }

        public int Error { get; set; }
    }
}
