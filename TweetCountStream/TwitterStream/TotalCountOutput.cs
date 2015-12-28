using Microsoft.ComplexEventProcessing;
using Microsoft.ComplexEventProcessing.Adapters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Net;
using System.IO;
using System.Xml.Linq;
using System.Globalization;
using System.Xml;

namespace TwitterStream
{   
    public class TotalCountOutput : PointOutputAdapter
    {
        private EventWaitHandle _adapterStopSignal;
        private CepEventType _bindtimeEventType;
        protected static IMongoClient _client;
        protected static IMongoDatabase _database;

        public TotalCountOutput(string StopSignalName, CepEventType EventType)
        {
            _bindtimeEventType = EventType;
            _adapterStopSignal = EventWaitHandle.OpenExisting(StopSignalName);
        }

        public override void Resume()
        {
            ConsumeEvents();
        }

        public override void Start()
        {
            ConsumeEvents();
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }

        private void ConsumeEvents()
        {
            PointEvent currEvent = default(PointEvent);
            DequeueOperationResult result;

            try
            {
                while (true)
                {
                    if (AdapterState.Stopping == AdapterState)
                    {

                        result = Dequeue(out currEvent);
                        PrepareToStop(currEvent, result);
                        Stopped();
                        _adapterStopSignal.Set();
                        return;
                    }
                    result = Dequeue(out currEvent);
                    if (DequeueOperationResult.Empty == result)
                    {
                        PrepareToResume();
                        Ready();
                        return;
                    }
                    else
                    {
                        if (currEvent.EventKind == EventKind.Insert)
                        {
                            _client = new MongoClient();
                            _database = _client.GetDatabase("DiseaseMonitor");

                            if (currEvent.GetField(0).ToString() == "flu")
                            {

                                var document = new BsonDocument
                                {                                
                                    {"Count",currEvent.GetField(1).ToString()},
                                    {"Time",System.DateTime.Now}                                
                                };
                                var collection = _database.GetCollection<BsonDocument>("FluRealTimeCount");
                                collection.InsertOneAsync(document);
                            }
                            else
                            {
                                var document = new BsonDocument
                                {                                
                                    {"Count",currEvent.GetField(1).ToString()},
                                    {"Time",System.DateTime.Now}                                
                                };
                                var collection = _database.GetCollection<BsonDocument>("CancerRealTimeCount");
                                collection.InsertOneAsync(document);
                            }
                            string s = CreateString(currEvent);
                            Console.WriteLine(s);
                        }
                        ReleaseEvent(ref currEvent);
                    }
                }
            }
            catch (AdapterException e)
            {
                Console.WriteLine("ConsumeEvents - " + e.Message + e.StackTrace);

            }
        }

        private void PrepareToStop(PointEvent currEvent, DequeueOperationResult result)
        {
            if (DequeueOperationResult.Success == result)
            {
                ReleaseEvent(ref currEvent);
            }
        }

        private void PrepareToResume()
        {
        }

        public string CreateString(PointEvent currEvent)
        {

            if (EventKind.Cti == currEvent.EventKind)
            {
                return currEvent.StartTime.ToString();
            }
            else
            {
                return currEvent.GetField(1).ToString();
            }

        }
    }
}
