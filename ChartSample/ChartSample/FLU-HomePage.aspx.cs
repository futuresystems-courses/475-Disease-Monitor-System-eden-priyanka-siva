using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.DataVisualization.Charting;
using System.Web.UI.WebControls;

namespace ChartSample
{
    public class EventType
    {
        public ObjectId Id { get; set; }   
        public string Type { get; set; }
        public int Cnt { get; set; }        
    }
    public class RealTimeEventType
    {
        public ObjectId Id { get; set; }
        public int Count { get; set; }
        public DateTime Time { get; set; }
        public String TimeString { get; set; }
    }
    public partial class WebForm1 : System.Web.UI.Page
    {
        protected static IMongoClient _client;
        protected static IMongoDatabase _database;
        public int x = 0;

        

        protected void Page_Load(object sender, EventArgs e)
        {
          
            _client = new MongoClient();
            _database = _client.GetDatabase("DiseaseMonitor");

            var collection = _database.GetCollection<EventType>("CancerCount");
            List<EventType> cnt = collection.AsQueryable().Where(b => b.Cnt > 1).ToList();

            var flucollection = _database.GetCollection<EventType>("FluSymptoms");
            
            List<EventType> Symptomscnt = flucollection.AsQueryable().Where(b => b.Cnt > 1).ToList();
            var total =0;
            total = flucollection.AsQueryable().Sum(b =>b.Cnt);
            

            
            this.Chart2.ChartAreas[0].BorderDashStyle = ChartDashStyle.Solid;
            this.Chart2.ChartAreas[0].BorderColor = Color.Black;
            this.Chart2.ChartAreas[0].BorderWidth = 1;
            this.Chart2.Titles.Add("Flu Symptoms Pie-Chart distribution - Total Count = " + total);
            this.Chart2.Legends.Add("Legend1");
            this.Chart2.Legends[0].Enabled = true;
            this.Chart2.Legends[0].Docking = Docking.Bottom;
            this.Chart2.Legends[0].Alignment = System.Drawing.StringAlignment.Center;
            this.Chart2.DataSource = Symptomscnt;
            Chart2.Series["FluSymptoms"].XValueMember = "Type";
            Chart2.Series["FluSymptoms"].YValueMembers = "Cnt";

            // Set the legend to display pie chart values as percentages
            // Again, the P2 indicates a precision of 2 decimals
            this.Chart2.Series[0].LegendText = "#PERCENT{P2}";


            string ChartData = GetChartData();
            Literal1.Text = @"<script type='text/javascript'>
            google.load('visualization', '1.0', { 'packages': ['geochart'] });
            google.setOnLoadCallback(drawChart);
            
            function drawChart() {

                // Create the data table.
                var data = new google.visualization.DataTable();
                data.addColumn('string', 'Country');
                data.addColumn('number', 'Views');
                data.addRows([ " + ChartData + @" ]);

                // Set chart options
                var options = { 
                    'title': 'Stats of my blog',
                    'width': 900,
                    'height': 600,
                    'colorAxis': {colors: ['orange', 'red']},
                    'legend':{textStyle: {color: 'navy', fontSize: 12}}
                };

                // Instantiate and draw our chart, passing in some options.
                var chart = new 
                google.visualization.GeoChart(document.getElementById('chart_container'));

                google.visualization.events.addListener(chart,
                'regionClick',function(eventOption){
                        alert('Region : ' + eventOption.region);
                      }); 
                chart.draw(data, options);
            }
        </script>";
            TimeZoneInfo easternZone = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
            var RealtimeFluCollection = _database.GetCollection<RealTimeEventType>("FluRealTimeCount");
            List<RealTimeEventType> Tweetscnt = RealtimeFluCollection.AsQueryable().Where(b => b.Time > DateTime.Now.AddMinutes(-10)).ToList();
            foreach(RealTimeEventType value in Tweetscnt)
            {
                //value.TimeString = value.Time.ToString("HH:mm:ss");
                value.TimeString = TimeZoneInfo.ConvertTimeFromUtc(value.Time, easternZone).ToString("MM/dd/yyyy HH:mm:ss");
            }
            

            this.Chart3.ChartAreas[0].BorderDashStyle = ChartDashStyle.Solid;
            this.Chart3.ChartAreas[0].BorderColor = Color.Black;
            this.Chart3.ChartAreas[0].BorderWidth = 1;
            this.Chart3.Titles.Add("Number of Flu tweets Vs. Current time");
            this.Chart3.DataSource = Tweetscnt;
            Chart3.Series[0].XValueMember = "TimeString";
            Chart3.Series[0].YValueMembers = "Count";

            //if (!IsPostBack)
            //{
            //    var RealtimeFluCollection1 = _database.GetCollection<RealTimeEventType>("FluRealTimeCount");
            //    List<RealTimeEventType> Tweetscnt1 = RealtimeFluCollection1.AsQueryable().Where(b => b.Time > DateTime.Now.AddMinutes(-10)).ToList();
            //    foreach (RealTimeEventType value in Tweetscnt1)
            //    {
            //        //value.TimeString = value.Time.ToString("MM/dd/yyyy HH:mm:ss");
            //        value.TimeString = TimeZoneInfo.ConvertTimeFromUtc(value.Time, easternZone).ToString("MM/dd/yyyy HH:mm:ss");
            //    }
               
            //    Chart3.Series[0].XValueMember = "Count";
            //    Chart3.Series[0].YValueMembers = "TimeString";
            //}
        }

        protected string GetChartData()
        {

            _client = new MongoClient();
            _database = _client.GetDatabase("DiseaseMonitor");

            var collection = _database.GetCollection<EventType>("FluLocationCount");
            List<EventType> Loc = collection.AsQueryable().Where(b => b.Cnt > 1).ToList();

            string ChartData = string.Empty;

            for (int i = 0; i < Loc.Count; i++)
            {
                ChartData += "['" + Loc[i].Type.ToString() + "', "
                            + Loc[i].Cnt.ToString().ToString() + "],";
            }

                
                return ChartData;
        }

        protected void UpdateTimer_Tick(object sender, EventArgs e)
        {
            //Random rnd = new Random();
            ////this will add a random number to the chart everytime the Timer1_Tick event is triggered    
            //for (int i = 0; i < 500; i++)
            //{
            //    this.Chart3.Series[0].Points.AddXY(i, rnd.Next(5, 20));
            //}
            
        }
        protected void Chart1_Load(object sender, EventArgs e)
        {

        }

        protected void Button1_Click(object sender, EventArgs e)
        {
            Server.Transfer("Cancer-Page.aspx", true);
        }
    }
}