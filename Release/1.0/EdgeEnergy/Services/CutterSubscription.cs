using System;
using System.Collections.Generic;

namespace EdgeEnergy.Services
{

    public class CutterSubscription
	{
        public string Symbol { get; set; }

        public CutterDataHeader DataHeader { get; set; }

        public Action<string, IEnumerable<CutterData>> SnapshotHandler{ get; set; }
        public Action<string, CutterData> UpdateHandler { get; set; }

        public bool DeliverRealtime { get; set; } 
        public CutterData LastData { get; set; }
	}
}
