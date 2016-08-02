using System;
using System.ComponentModel;
using System.Configuration;
using System.Windows;
using EdgeEnergy.CutterDashboard.Utils;
using EdgeEnergy.CutterDashboard.ViewModels;
using EdgeEnergy.CutterDashboard.Views;
using EdgeEnergy.Services;
using log4net;

namespace EdgeEnergy.CutterDashboard
{

    public partial class MainWindow
    {
        private static readonly ILog Log = LogManager.GetLogger("root");
        private DataService _service;

        public MainWindow()
        {
            InitializeComponent();

            Log.InfoFormat("Starting");

            FileOpenButton.Click += OnFileOpenButtonChecked;
            FileFtpButton.Click += OnFileFtpButtonChecked;
            FileTcpButton.Click += OnFileTcpButtonChecked;
            //FileTcpFileButton.Click += OnFileTcpFileButtonButtonChecked;

            //FileSaveButton.Click += OnFileSaveButtonChecked;
            AboutButton.Click += OnAboutButtonChecked;
        }




        protected override void OnClosing(CancelEventArgs e)
        {
            SaveFile();
            
            base.OnClosing(e);
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);

            Application.Current.Shutdown();
        }

        //private void StatusHandlerX(object sender, StatusEventArgs e)
        //{
        //    switch (e.Status)
        //    {
        //        case DataStatus.SplashBegin:
        //            Splash.BeginDisplay();
        //            break;

        //        case DataStatus.SplashDisplay:
        //            Splash.Loading(e.Message);
        //            break;

        //        case DataStatus.SplashEnd:
        //            Splash.EndDisplay();
        //            break;

        //        default:
        //            MessageBox.Show(e.Message, e.Status.ToString());
        //            break;
        //    }
        //}


        private Splash _splash;
        private void StatusHandler(object sender, StatusEventArgs e)
        {
            switch (e.Status)
            {
                case DataStatus.SplashBegin:

                     _splash = new Splash { Owner = this };
       
                    _splash.BeginDisplay();
                    break;

                case DataStatus.SplashDisplay:
                    if (_splash != null)
                        _splash.Loading(e.Message);
                    break;

                case DataStatus.SplashEnd:
                    if (_splash != null)
                        _splash.EndDisplay();
                    _splash = null;
                    break;

                case DataStatus.DataLog:
                    DataLogTextBlock.Text = e.Message;
                    break;

                default:
                    MessageBox.Show(e.Message, e.Status.ToString());
                    ManageButtons(true);
                    break;
            }
        }

        private void SaveFile()
        {
            if (_service == null) return;

            _service.Stop();    
            
            if ( _service.IsFileTouched() && ( _service.IsServiceType(DataServiceType.Ftp) || _service.IsServiceType(DataServiceType.Tcp)))
            {
                FileUtil.SaveFile(_service.DataFile);
            }

            DataLogTextBlock.Text = "";

            _service = null;
        }

        private void ProcessFile(DataService service)
        {
            try
            {
                StatusHandler( this, new StatusEventArgs { Status = DataStatus.SplashEnd} );

                DataLogTextBlock.Text = "";


                _service = service;

                _service.StatusEvent += StatusHandler;

                _service.Initialise();
                _service.Start();

                Title = _service.GetTitle();

                var view = new StocksView
                {
                    DataContext = new StockViewModel(_service),
                    KeepAlive = false
                };

                ContentFrame.Navigate(view);
            }
            catch (Exception ex)
            {
                Log.ErrorFormat("Exception {0}", ex);
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK);
                ManageButtons(true);

            }

        }

        private void OnFileOpenButtonChecked(object sender, RoutedEventArgs e)
        {
            if (SecUtil.ProtCheck() != 0)
                return;

            SaveFile();

            var filename = FileUtil.OpenFile();
            if (string.IsNullOrEmpty(filename)) return;


            var service = new DataService
            {
                ServiceType = DataServiceType.File,
                DataFile = filename,

                HeaderFile = FileUtil.GetConfigFile(filename),
                ResultFile = FileUtil.GetResultFile(),
            };
            
            ProcessFile(service);
        }
    
        private void OnFileFtpButtonChecked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (SecUtil.ProtCheck() != 0)
                    return;

                SaveFile();

                var dlg = new SettingsDialog
                    {
                        Owner = this,
                        FtpHost = Properties.Settings.Default.FtpHost,
                        Title = "FTP Server"
                    };

                if (dlg.ShowDialog() != true) return;

                var host = dlg.FtpHost.Trim();
                Properties.Settings.Default.FtpHost = host;
                Properties.Settings.Default.Save();

                var filename = FileUtil.GetFtpFile();
                var service = new DataService
                {
                    ServiceType = DataServiceType.Ftp,

                    DataFile = filename,
                    HeaderFile = FileUtil.GetConfigFile(filename),
                    ResultFile = FileUtil.GetResultFile(),


                    FtpUrl = FileUtil.GetFtpUrl(host),

                    FtpUser = ConfigurationManager.AppSettings.Get("FtpUser"),
                    FtpPassword = ConfigurationManager.AppSettings.Get("FtpPassword"),
                    FtpUsePassive = bool.Parse(ConfigurationManager.AppSettings.Get("FtpUsePassive")),
                    FtpTimeout = int.Parse(ConfigurationManager.AppSettings.Get("FtpTimeout"))

                };

                ProcessFile(service);

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK);
                
            }


        }

        private bool _inTcp;

        private void ManageButtons(bool enabled)
        {
            FileOpenButton.IsEnabled = enabled;
            FileFtpButton.IsEnabled = enabled;

            FileTcpButton.Content = enabled ? "Realtime" : "Stop";


            _inTcp = !enabled;
        }

        private void OnFileTcpButtonChecked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (SecUtil.ProtCheck() != 0)
                    return;

                if (_inTcp)
                {
                    SaveFile();
                    ManageButtons(true);
                    return;
                }
                else
                {
                    ManageButtons(false);
                }

                var dlg = new SettingsDialog
                {
                    Owner = this,
                    FtpHost = Properties.Settings.Default.FtpHost,
                    Title = "Realtime Server"
                };

                if (dlg.ShowDialog() != true) return;

                var host = dlg.FtpHost.Trim();
                Properties.Settings.Default.FtpHost = host;
                Properties.Settings.Default.Save();


                var filename = FileUtil.GetTcpFile();
                var service = new DataService
                {
                    ServiceType = DataServiceType.Tcp,

                    DataFile = filename,
                    HeaderFile = FileUtil.GetConfigFile(filename),
                    ResultFile = FileUtil.GetResultFile(),

                    TcpHost = host,
                    TcpPort = int.Parse(ConfigurationManager.AppSettings.Get("TcpPort")),
                    TcpHeatbeatMessage = ConfigurationManager.AppSettings.Get("TcpHeatbeatMessage"),
                    TcpHeatbeatInterval = int.Parse(ConfigurationManager.AppSettings.Get("TcpHeatbeatInterval"))

                };

                ProcessFile(service);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK);
                ManageButtons(true);

            }
        }

        //private void OnFileTcpFileButtonButtonChecked(object sender, RoutedEventArgs e)
        //{
        //    try
        //    {
        //        if (SecUtil.ProtCheck() != 0)
        //            return;

        //        SaveFile();

        //        var filename = FileUtil.OpenFile();
        //        if (string.IsNullOrEmpty(filename)) return;


        //        var dlg = new SettingsDialog
        //        {
        //            Owner = this,
        //            FtpHost = Properties.Settings.Default.FtpHost,
        //            Title = "Realtime Server"
        //        };

        //        if (dlg.ShowDialog() != true) return;

        //        var host = dlg.FtpHost.Trim();
        //        Properties.Settings.Default.FtpHost = host;
        //        Properties.Settings.Default.Save();

        //        var service = new DataService
        //        {
        //            ServiceType = DataServiceType.Tcp | DataServiceType.File,

        //            DataFile = filename,
        //            HeaderFile = FileUtil.GetConfigFile(filename),
        //            ResultFile = FileUtil.GetResultFile(),

        //            TcpHost = host,
        //            TcpPort = int.Parse(ConfigurationManager.AppSettings.Get("TcpPort")),
        //            TcpHeatbeatMessage = ConfigurationManager.AppSettings.Get("TcpHeatbeatMessage"),
        //            TcpHeatbeatInterval = int.Parse(ConfigurationManager.AppSettings.Get("TcpHeatbeatInterval"))

        //        };

        //        ProcessFile(service);
        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK);

        //    }
        //}

        private void OnAboutButtonChecked(object sender, RoutedEventArgs e)
        {
            var dwin = new About { Owner = this };
            dwin.ShowDialog();
        }

    }
}
