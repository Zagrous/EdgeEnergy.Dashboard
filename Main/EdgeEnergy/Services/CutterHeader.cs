using System;
using System.Data;

namespace EdgeEnergy.Services
{
    public class CutterDataHeader
    {
        public int Index { get; private set; }
        public string Name { get; private set; }
        public string Type { get; private set; }
        public string Legend { get; private set; }
        public string Colour { get; private set; }
        public bool Available { get; private set; }
        public bool Default { get; private set; }

        public CutterDataHeader(string[] parts, DataTable dt)
        {
            Index = int.Parse(parts[0].Trim());
            Name = parts[1].Trim();
            Type = parts[2].Trim();
            Legend = parts[3].Trim();
            Colour = parts[4].Trim();
            Available = (parts[5].Trim() == "1");
            Default = (parts[6].Trim() == "1");


            Type type;
            switch (parts[2])
            {
                case "Date":
                    type = typeof(DateTime);
                    break;
                case "Time":
                    type = typeof(DateTime);
                    break;
                default:
                    type = typeof(string);
                    break;
            }

            dt.Columns.Add(Legend, type);

        }

        public override string ToString()
        {
            return string.Format("Index: {0}, Name: {1}, Type: {2}, Legend: {3}, Colour: {4}, Available: {5}, Default: {6}", Index, Name, Type, Legend, Colour, Available, Default);
        }
    }
    
}
