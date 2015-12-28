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
    public partial class Cancer_Page : System.Web.UI.Page
    {
        protected static IMongoClient _client;
        protected static IMongoDatabase _database;
        public int x = 0;

        protected void Page_Load(object sender, EventArgs e)
        {
            _client = new MongoClient();
            _database = _client.GetDatabase("DiseaseMonitor");

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

            var CancerCollection = _database.GetCollection<EventType>("CancerCount");
            List<EventType> cnt = CancerCollection.AsQueryable().Where(b => b.Cnt > 1).ToList();
            var total = 0;
            total = CancerCollection.AsQueryable().Sum(b => b.Cnt);

            this.Chart1.ChartAreas[0].BorderDashStyle = ChartDashStyle.Solid;
            this.Chart1.ChartAreas[0].BorderColor = Color.Black;
            this.Chart1.ChartAreas[0].BorderWidth = 1;
            this.Chart1.Titles.Add("Cancer Types Pie-Chart distribution - Total Count = " + total);        
            this.Chart1.Legends.Add("Legend1");
            this.Chart1.Legends[0].Enabled = true;
            this.Chart1.Legends[0].Docking = Docking.Bottom;            
            this.Chart1.Legends[0].Alignment = System.Drawing.StringAlignment.Center;
            this.Chart1.DataSource = cnt;
            Chart1.Series["CancerSeries"].XValueMember = "Type";
            Chart1.Series["CancerSeries"].YValueMembers = "Cnt";

             //Set the legend to display pie chart values as percentages
             //Again, the P2 indicates a precision of 2 decimals
            this.Chart1.Series[0].LegendText = "#PERCENT{P2}";
            TimeZoneInfo easternZone = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
            var RealtimeCancerCollection = _database.GetCollection<RealTimeEventType>("CancerRealTimeCount");
            List<RealTimeEventType> Tweetscnt = RealtimeCancerCollection.AsQueryable().Where(b => b.Time > DateTime.Now.AddMinutes(-10)).ToList();
            foreach (RealTimeEventType value in Tweetscnt)
            {
                value.TimeString = TimeZoneInfo.ConvertTimeFromUtc(value.Time, easternZone).ToString("MM/dd/yyyy HH:mm:ss");
            }


            this.Chart3.ChartAreas[0].BorderDashStyle = ChartDashStyle.Solid;
            this.Chart3.ChartAreas[0].BorderColor = Color.Black;
            this.Chart3.ChartAreas[0].BorderWidth = 1;
            this.Chart3.Titles.Add("Number of Cancer Tweets Vs. Current time");
            this.Chart3.DataSource = Tweetscnt;
            Chart3.Series[0].XValueMember = "TimeString";
            Chart3.Series[0].YValueMembers = "Count";

            //if (!IsPostBack)
            //{
            //    var RealtimeCancerCollection1 = _database.GetCollection<RealTimeEventType>("CancerRealTimeCount");
            //    List<RealTimeEventType> Tweetscnt1 = RealtimeCancerCollection1.AsQueryable().Where(b => b.Time > DateTime.Now.AddMinutes(-10)).ToList();
            //    foreach (RealTimeEventType value in Tweetscnt1)
            //    {
            //        value.TimeString = TimeZoneInfo.ConvertTimeFromUtc(value.Time, easternZone).ToString("MM/dd/yyyy HH:mm:ss");
            //    }
            //    //this.Chart3.ChartAreas[0].BorderDashStyle = ChartDashStyle.Solid;
            //    //this.Chart3.ChartAreas[0].BorderColor = Color.Black;
            //    //this.Chart3.ChartAreas[0].BorderWidth = 1;
            //    //this.Chart3.Titles.Add("Flu Symptoms");
            //    this.Chart3.DataSource = Tweetscnt1;
            //    Chart3.Series[0].XValueMember = "Count";
            //    Chart3.Series[0].YValueMembers = "TimeString";
            //}

        }

        protected void Button1_Click(object sender, EventArgs e)
        {
            Server.Transfer("FLU-HomePAge.aspx", true);
        }

        protected void UpdateTimer_Tick(object sender, EventArgs e)
        {
            //Random rnd = new Random();
            //this will add a random number to the chart everytime the Timer1_Tick event is triggered    
            //for (int i = 0; i < 500; i++)
            //{
            //    this.Chart3.Series[0].Points.AddXY(i, rnd.Next(5, 20));
            //}
        }
        protected string GetChartData()
        {

            _client = new MongoClient();
            _database = _client.GetDatabase("DiseaseMonitor");

            var collection = _database.GetCollection<EventType>("CancerLocationCount");
            List<EventType> Loc = collection.AsQueryable().Where(b => b.Cnt > 1).ToList();

            string ChartData = string.Empty;

            for (int i = 0; i < Loc.Count; i++)
            {
                ChartData += "['" + Loc[i].Type.ToString() + "', "
                            + Loc[i].Cnt.ToString().ToString() + "],";
            }


            return ChartData;
        }

    }
}