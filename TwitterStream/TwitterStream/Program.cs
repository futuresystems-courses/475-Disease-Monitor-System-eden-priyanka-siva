﻿using Microsoft.ComplexEventProcessing;
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
            Query _results, _results2;
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

                    var query1 = from e in input
                                 select new { text = e.Text, createdAt = e.CreatedAt, location=e.Location, lang=e.Lang, tweetid=e.TweetID, followercnt=e.FollowersCount, friendscnt=e.FriendsCount, hash = e.HashTag, userName = e.UserName, timeZone=e.TimeZone, lat = e.Latitude, lon = e.Longitude };

                    _results = query1.ToQuery(_application, "TweetsData", "Data Query", typeof(OutputFactory),
                                          _stopSignalName,
                                          EventShape.Point,
                                          StreamEventOrder.FullyOrdered);
                    _adapterStopSignal.Reset();                    
                    //Start the Query                
                    _results.Start();                    
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    Console.ReadLine();
                }
        }
    }
}
