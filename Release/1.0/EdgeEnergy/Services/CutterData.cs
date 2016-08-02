using System;
using System.Runtime.Serialization;
using System.Globalization;

namespace EdgeEnergy.Services
{
	[DataContract]
	public class CutterData
	{
	    private DateTime _date;

        //Workaround datetime deserialization
        [DataMember(Name = "Date")]
        public string DateString { get; set; }

        public DateTime Date
        {
            get
            {
                return _date;
            }
            set
            {
                _date = value;
                DateString = value.ToString("HH:mm:ss", CultureInfo.InvariantCulture);
            }
        }

		[DataMember(Name = "Value")]
		public double Value { get; set; }

		[DataMember(Name = "ValueString")]
        public string ValueString { get; set; }

        public override string ToString()
        {
            return string.Format("X-String: {0}, X-Value: {1}, Y-Value: {2}, Y-String: {3}", 
                DateString, Date, Value, ValueString);
        }
	}
}
