using System;
using System.Linq;
using log4net;

namespace EdgeEnergy.TestConsole
{
    class Program
    {
        private static readonly ILog Log = LogManager.GetLogger("root");

        static void Main()
        {
            //XmlConfigurator.ConfigureAndWatch(new FileInfo("app.config"));


            try
            {
                //TestProvider();
                //TestFtp();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception {0}", ex);
                Log.ErrorFormat("Exception {0}", ex);
            }

            Console.WriteLine("Finished");
            Console.ReadKey();

        }

        //private static void TestFtp()
        //{
        //    // CutterDataFtp.Download();
        //    //CutterDataFtp.Download("ftp://188.121.45.1/data.csv", "data.csv", "ones1177", "Gregory@64");    
        //    //CutterDataFtp.Download("ftp://188.121.45.1/junk.html", "ones1177", "Gregory@64");    
        //    //CutterDataFtp.Download("ftp://188.121.45.1/junk.html", "ones1177", "asd@64");    
        //}

        //private static void TestProvider()
        //{
        //    var dataProvider =
        //        new CutterDataProvider(@"D:\Development\OneSource\EdgeEnergy\EdgeEnergy\TestData\HeaderConfig.csv",
        //                               @"D:\Development\OneSource\EdgeEnergy\EdgeEnergy\TestData\Data01.csv",
        //                               @"D:\Development\OneSource\EdgeEnergy\EdgeEnergy\TestData\Result.01.csv");


        //    foreach (var available in dataProvider.GetAvailables())
        //    {
        //        var data = dataProvider.GetData(available);
        //        var list = data.Item2.ToList();
        //        Log.InfoFormat("---- {0}  Count={1} ----", data.Item1, list.Count);

        //        foreach (var cutterData in list)
        //        {
        //            Log.InfoFormat("{0}", cutterData);
        //        }
        //    }
        //}
    }
}
