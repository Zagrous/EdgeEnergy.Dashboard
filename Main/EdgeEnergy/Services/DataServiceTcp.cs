using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace EdgeEnergy.Services
{
    public partial class DataService
    {
        private bool _processing;

        private TcpClient _tcpClient;
        private NetworkStream _tcpStream;
        private Thread _tcpThread;
        private bool _inConnectionRecovery;
        DateTime _lastHeatbeat = DateTime.MinValue;
        byte[] _heatbeatMessage;


        private void StopTcp()
        {
            Log.InfoFormat("Stopping..");
            _processing = false;

            if (_tcpThread != null)
                _tcpThread.Join();

            if (_tcpStream != null)
                _tcpStream.Close();

            if( _tcpClient != null)
                _tcpClient.Close();

            Log.InfoFormat("Stopped");
        }

        private void StartTcp()
        {
            _heatbeatMessage = Encoding.ASCII.GetBytes(TcpHeatbeatMessage);

            _tcpThread = new Thread( ProcessTcp );
            _processing = true;

            _tcpThread.Start();
        }

        private void TryToConnect()
        {
            try
            {

                if (!_inConnectionRecovery)
                {
                    //Log.FatalFormat("OnStatus 10");
                    OnStatus(DataStatus.SplashBegin);
                    OnStatus(DataStatus.SplashDisplay, "Connection is lost, Recovering...");
                    _inConnectionRecovery = true;
                }



                if (_tcpStream != null)
                    _tcpStream.Close();

                if (_tcpClient != null)
                    _tcpClient.Close();

                _tcpClient = new TcpClient(TcpHost, TcpPort);
                _tcpStream = _tcpClient.GetStream();

                if (_inConnectionRecovery)
                {
                    //Log.FatalFormat("OnStatus 11");
                    OnStatus(DataStatus.SplashEnd);
                    _inConnectionRecovery = false;
                }


            }
            catch (Exception ex)
            {
                Log.InfoFormat("Failed to connect {0}", ex.Message);
                Thread.Sleep(TcpHeatbeatInterval * 1000);
            }
        }


        private void Process()
        {
            while (_processing)
            {
                try
                {

                    if (_tcpClient.Available > 0)
                        ReceiveData();
                    else
                        SendHeartbeat();

                }
                catch (Exception e)
                {
                    //Log.ErrorFormat("Exception: {0}", e);
                    TryToConnect();
                }
            }
        }

        private void ProcessTcp()
        {
            try
            {
                _tcpClient = new TcpClient(TcpHost, TcpPort);

                _tcpStream = _tcpClient.GetStream();

                Process();
            }
            catch (ArgumentNullException e)
            {
                OnStatus(DataStatus.Exception, string.Format("Unable to establish connection to remote server.\n\n{0}",   e.Message));
                Log.ErrorFormat("ArgumentNullException: {0}", e);
            }
            catch (SocketException e)
            {
                OnStatus(DataStatus.Exception, string.Format("Unable to establish connection to remote server.\n\n{0}", e.Message));
                Log.ErrorFormat("SocketException: {0}", e);
            }
            catch (Exception e)
            {
                OnStatus(DataStatus.Exception, string.Format("Unable to establish connection to remote server.\n\n{0}", e.Message));
                Log.ErrorFormat("Exception: {0}", e);
            }

        }

        private void ReceiveData()
        {
            var data = new Byte[2048];

            var bytes = _tcpStream.Read(data, 0, data.Length);
            string received = Encoding.ASCII.GetString(data, 0, bytes);


            var records = received.Split(new[] {Environment.NewLine}, StringSplitOptions.None);

            if (Verbose) Log.InfoFormat("Received: bytes: {0} len: {1} {2}", bytes, records.Length, received);


            foreach (var msg in records)
            {
                var record = msg.Trim();

                if (string.IsNullOrEmpty(record)) continue;

                if (Verbose) Log.InfoFormat("Received: record: {0}", record);
                WriteFile(DataFile, record);

                AddData(record);
            }
        }

        private void SendHeartbeat()
        {
            var diff = (DateTime.Now - _lastHeatbeat).TotalSeconds;
            if (diff > TcpHeatbeatInterval)
            {
                _tcpStream.Write(_heatbeatMessage, 0, _heatbeatMessage.Length);

                if (Verbose) Log.InfoFormat("Sent: {0} {1}", DateTime.Now.ToLongTimeString(), TcpHeatbeatMessage);
                _lastHeatbeat = DateTime.Now;
            }
        }
    }
}
