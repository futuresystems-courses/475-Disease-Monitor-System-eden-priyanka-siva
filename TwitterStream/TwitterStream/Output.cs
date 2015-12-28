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
    public class EventType
    {
        public ObjectId Id { get; set; }
        public string Type { get; set; }
        public int Cnt { get; set; }
    }
    public class Output : PointOutputAdapter
    {
        private EventWaitHandle _adapterStopSignal;
        private CepEventType _bindtimeEventType;
        protected static IMongoClient _client;
        protected static IMongoDatabase _database;

        public Output(string StopSignalName, CepEventType EventType)
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
                            Double latitude = Convert.ToDouble(currEvent.GetField(5)), longitude = Convert.ToDouble(currEvent.GetField(7));                         
                            if (latitude !=0  && longitude != 0)
                            {
                                XmlDocument doc = new XmlDocument();
                                doc.Load("http://maps.googleapis.com/maps/api/geocode/xml?latlng=" + latitude + "," + longitude + "&sensor=false");
                                XmlNode element = doc.SelectSingleNode("//GeocodeResponse/status");
                                string Address_country = String.Empty;
                                if (element.InnerText != "ZERO_RESULTS")
                                {

                                    element = doc.SelectSingleNode("//GeocodeResponse/result/formatted_address");

                                    string longname = "";
                                    string shortname = "";
                                    string typename = "";

                                    XmlNodeList xnList = doc.SelectNodes("//GeocodeResponse/result/address_component");
                                    foreach (XmlNode xn in xnList)
                                    {
                                        longname = xn["long_name"].InnerText;
                                        shortname = xn["short_name"].InnerText;
                                        typename = xn["type"].InnerText;
                                        switch (typename)
                                        {
                                            case "country":
                                                {
                                                    Address_country = longname;
                                                    break;
                                                }
                                        }
                                    }
                                }

                                if (Address_country != String.Empty && currEvent.GetField(3).ToString() == "flu")
                                {
                                    _client = new MongoClient();
                                    _database = _client.GetDatabase("DiseaseMonitor");
                                    int count = 0;
                                    var col = _database.GetCollection<EventType>("LocationCount");
                                    List<EventType> cnt = col.AsQueryable().Where(b => b.Type == Address_country).ToList();
                                    if (cnt.Count == 0)
                                    {
                                        count = 1;
                                        var newdoc = new BsonDocument
                                        {
                                         {"Type",Address_country},
                                         {"Cnt",count},                               
                                        };
                                        var col1 = _database.GetCollection<BsonDocument>("LocationCount");
                                        col1.InsertOneAsync(newdoc);
                                    }
                                    else
                                    {
                                        count = cnt.FirstOrDefault().Cnt + 1;
                                       
                                        var filter = Builders<BsonDocument>.Filter.Eq("Type", Address_country);
                                        var update = Builders<BsonDocument>.Update.Set("Cnt", count);
                                        var col1 = _database.GetCollection<BsonDocument>("LocationCount");
                                        var res = col1.UpdateOneAsync(filter, update);                                        
                                    }
                                }
                                if (Address_country != String.Empty && currEvent.GetField(3).ToString() == "cancer")
                                {
                                    _client = new MongoClient();
                                    _database = _client.GetDatabase("DiseaseMonitor");
                                    int count = 0;
                                    var col = _database.GetCollection<EventType>("CancerLocationCount");
                                    List<EventType> cnt = col.AsQueryable().Where(b => b.Type == Address_country).ToList();
                                    if (cnt.Count == 0)
                                    {
                                        count = 1;
                                        var newdoc = new BsonDocument
                                        {
                                         {"Type",Address_country},
                                         {"Cnt",count},                               
                                        };
                                        var col1 = _database.GetCollection<BsonDocument>("CancerLocationCount");
                                        col1.InsertOneAsync(newdoc);
                                    }
                                    else
                                    {
                                        count = cnt.FirstOrDefault().Cnt + 1;

                                        var filter = Builders<BsonDocument>.Filter.Eq("Type", Address_country);
                                        var update = Builders<BsonDocument>.Update.Set("Cnt", count);
                                        var col1 = _database.GetCollection<BsonDocument>("CancerLocationCount");
                                        var res = col1.UpdateOneAsync(filter, update);
                                    }
                                }
                            }
                            _client = new MongoClient();
                            _database = _client.GetDatabase("DiseaseMonitor");

                            var document = new BsonDocument
                            {                                
                                {"FollowersCount",currEvent.GetField(2).ToString()},
                                {"Location",currEvent.GetField(6).ToString()},
                                {"Text",currEvent.GetField(8).ToString()},
                                {"CreatedAt",Convert.ToDateTime(currEvent.GetField(0))},
                                {"UserName",currEvent.GetField(11).ToString()},
                                {"FriendsCount",currEvent.GetField(1).ToString()},
                                {"TweetID",currEvent.GetField(10).ToString()},
                                {"Lang",currEvent.GetField(4).ToString()},
                                {"HashTag",currEvent.GetField(3).ToString()},
                                {"TimeZone",currEvent.GetField(9).ToString()},
                                {"X",currEvent.GetField(5).ToString()},
                                {"Y",currEvent.GetField(7).ToString()},
                            };
                            var collection = _database.GetCollection<BsonDocument>("tweets");
                            collection.InsertOneAsync(document);

                            if (currEvent.GetField(3).ToString() == "flu")
                            {
                                var FScollection = _database.GetCollection<BsonDocument>("FluSymptoms");
                                
                                if(currEvent.GetField(8).ToString().Contains("headache"))
                                {
                                    var filter = Builders<BsonDocument>.Filter.Eq("Type", "Headache");
                                    var update = Builders<BsonDocument>.Update.Inc("Cnt", 1);
                                    var resultFS = FScollection.UpdateOneAsync(filter,update);
                                }
                                if (currEvent.GetField(8).ToString().Contains("cough"))
                                {
                                    var filter = Builders<BsonDocument>.Filter.Eq("Type", "Cough");
                                    var update = Builders<BsonDocument>.Update.Inc("Cnt", 1);
                                    var resultFS = FScollection.UpdateOneAsync(filter, update);
                                }
                                if (currEvent.GetField(8).ToString().Contains("sore throat"))
                                {
                                    var filter = Builders<BsonDocument>.Filter.Eq("Type", "Sore throat");
                                    var update = Builders<BsonDocument>.Update.Inc("Cnt", 1);
                                    var resultFS = FScollection.UpdateOneAsync(filter, update);
                                }
                                if (currEvent.GetField(8).ToString().Contains("running nose"))
                                {
                                    var filter = Builders<BsonDocument>.Filter.Eq("Type", "Running nose");
                                    var update = Builders<BsonDocument>.Update.Inc("Cnt", 1);
                                    var resultFS = FScollection.UpdateOneAsync(filter, update);
                                }
                                if (currEvent.GetField(8).ToString().Contains("muscle pain"))
                                {
                                    var filter = Builders<BsonDocument>.Filter.Eq("Type", "Muscle pain");
                                    var update = Builders<BsonDocument>.Update.Inc("Cnt", 1);
                                    var resultFS = FScollection.UpdateOneAsync(filter, update);
                                }
                                if (currEvent.GetField(8).ToString().Contains("dizziness"))
                                {
                                    var filter = Builders<BsonDocument>.Filter.Eq("Type", "Dizziness");
                                    var update = Builders<BsonDocument>.Update.Inc("Cnt", 1);
                                    var resultFS = FScollection.UpdateOneAsync(filter, update);
                                }
                                if (currEvent.GetField(8).ToString().Contains("vomiting"))
                                {
                                    var filter = Builders<BsonDocument>.Filter.Eq("Type", "Vomiting");
                                    var update = Builders<BsonDocument>.Update.Inc("Cnt", 1);
                                    var resultFS = FScollection.UpdateOneAsync(filter, update);
                                }
                            }

                            if (currEvent.GetField(3).ToString() == "cancer")
                            {
                                var Cancercollection = _database.GetCollection<BsonDocument>("CancerCount");

                                if (currEvent.GetField(8).ToString().Contains("breast"))
                                {
                                    var filter = Builders<BsonDocument>.Filter.Eq("Type", "Breast");
                                    var update = Builders<BsonDocument>.Update.Inc("Cnt", 1);
                                    var resultFS = Cancercollection.UpdateOneAsync(filter, update);
                                }
                                if (currEvent.GetField(8).ToString().Contains("kidney"))
                                {
                                    var filter = Builders<BsonDocument>.Filter.Eq("Type", "Kidney");
                                    var update = Builders<BsonDocument>.Update.Inc("Cnt", 1);
                                    var resultFS = Cancercollection.UpdateOneAsync(filter, update);
                                }
                                if (currEvent.GetField(8).ToString().Contains("skin"))
                                {
                                    var filter = Builders<BsonDocument>.Filter.Eq("Type", "Skin");
                                    var update = Builders<BsonDocument>.Update.Inc("Cnt", 1);
                                    var resultFS = Cancercollection.UpdateOneAsync(filter, update);
                                }
                                if (currEvent.GetField(8).ToString().Contains("lung"))
                                {
                                    var filter = Builders<BsonDocument>.Filter.Eq("Type", "Lung");
                                    var update = Builders<BsonDocument>.Update.Inc("Cnt", 1);
                                    var resultFS = Cancercollection.UpdateOneAsync(filter, update);
                                }
                                if (currEvent.GetField(8).ToString().Contains("liver"))
                                {
                                    var filter = Builders<BsonDocument>.Filter.Eq("Type", "Liver");
                                    var update = Builders<BsonDocument>.Update.Inc("Cnt", 1);
                                    var resultFS = Cancercollection.UpdateOneAsync(filter, update);
                                }
                                if (currEvent.GetField(8).ToString().Contains("thyroid"))
                                {
                                    var filter = Builders<BsonDocument>.Filter.Eq("Type", "Thyroid");
                                    var update = Builders<BsonDocument>.Update.Inc("Cnt", 1);
                                    var resultFS = Cancercollection.UpdateOneAsync(filter, update);
                                }
                                if (currEvent.GetField(8).ToString().Contains("brain"))
                                {
                                    var filter = Builders<BsonDocument>.Filter.Eq("Type", "Brain");
                                    var update = Builders<BsonDocument>.Update.Inc("Cnt", 1);
                                    var resultFS = Cancercollection.UpdateOneAsync(filter, update);
                                }
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
                return currEvent.GetField(10).ToString();
            }
            
        }
    }
}
