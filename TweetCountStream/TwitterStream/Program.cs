using Microsoft.ComplexEventProcessing;
using Microsoft.ComplexEventProcessing.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TwitterStream
{
    class Program
    {
        static void Main(string[] args)
        {
            Server _server = null;
            Application _application = null;            
            Query _results;
            //InputAdapter Config 
                InputConfig _inputConfig = new InputConfig();


                string _stopSignalName = "TimeToStop";                
                EventWaitHandle _adapterStopSignal = new EventWaitHandle(false, EventResetMode.ManualReset, _stopSignalName);                

                try
                {

                    // Creating the server and application
                    _server = Server.Create("localhost");
                    _application = _server.CreateApplication("TwitterStream");                    
                    var input = CepStream<TweetsType>.Create("TwitterInputStream", typeof(InputFactory), _inputConfig, EventShape.Point);

                    var query1 = from s in input
                                 group s by s.HashTag into grp
                                 from win in grp.TumblingWindow(TimeSpan.FromSeconds(10), HoppingWindowOutputPolicy.ClipToWindowEnd)
                                 select new
                                 {
                                     HashTag = grp.Key,
                                     cnt = win.Count()
                                 };
                    _results = query1.ToQuery(_application, "TweetsCount", "Data Query2", typeof(TotalCountOutputFactory),
                                          _stopSignalName,
                                          EventShape.Point,
                                          StreamEventOrder.FullyOrdered);
                    _adapterStopSignal.Reset();                    
                    //Start the Query                
                    _results.Start();
                    //_results2.Start();                    
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    Console.ReadLine();
                }
        }
    }
}
