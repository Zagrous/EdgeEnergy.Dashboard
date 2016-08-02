using System;
using System.Collections.Generic;

namespace EdgeEnergy.Services
{
    [Flags]
    public enum DataServiceType
    {
        File = 0x0002,
        Ftp  = 0x0004,
        Tcp  = 0x0008
    }

    public enum DataStatus
    {
        SplashBegin,
        SplashDisplay,
        SplashEnd,
        Info,
        Error,
        Exception,
        DataLog
    }

    //class TickEventArgs : EventArgs
    //{
    //    string Symbol { get; set; }
    //    CutterData Record { get; set; }
    //}

    //class DataEventArgs : EventArgs
    //{
    //    string Symbol { get; set; }
    //    List<CutterData> Records { get; set; }
    //}

    public class StatusEventArgs : EventArgs
    {
        public DataStatus Status { get; set; }
        public string Message { get; set; }
    }

    public interface IDataService
    {
        event EventHandler<StatusEventArgs> StatusEvent;

        string HeaderFile { get; set; }
        string DataFile { get; set; }
        string ResultFile { get; set; }

        DataServiceType ServiceType { get; set; }

        string FtpUser { get; set; }
        string FtpPassword { get; set; }
        bool FtpUsePassive { get; set; }
        int FtpTimeout { get; set; }
        string FtpUrl { get; set; }

        string TcpHost { get; set; }
        int TcpPort { get; set; }
        string TcpHeatbeatMessage { get; set; }
        int TcpHeatbeatInterval { get; set; }
        
        bool Verbose { get; set; }
        string DateFormat { get; set; }

        int Error { get; set; }

        void Initialise();

        void Subscribe(string symbol, Action<string, IEnumerable<CutterData>> snapshotHandler, Action<string, CutterData> updateHandler);
        void Unsubscribe(string symbol);

        IEnumerable<string> GetAvailableSymbols();

        IEnumerable<string> GetDefaultLegends();

        string GetColour(string symbol);

        string GetTitle();

        bool IsFileTouched();
    }
}