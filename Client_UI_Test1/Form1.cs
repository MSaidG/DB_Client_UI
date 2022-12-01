using System;
using System.Diagnostics;
using System.Threading;
using System.Linq;
using System.Threading.Tasks;
using InfluxDB.Client;
using InfluxDB.Client.Api.Domain;
using InfluxDB.Client.Core;
using InfluxDB.Client.Writes;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Timers;





namespace Client_UI_Test1
{
    public partial class Form1 : Form //test
    {

        private static System.Timers.Timer timer;

        public Form1()
        {
            InitializeComponent();
            Control.CheckForIllegalCrossThreadCalls = false;
        }

        private void Form1_Load_1(object sender, EventArgs e)
        {
            
        }

        private void button2_Click(object sender, EventArgs e)
        {
            var values = InitializeClient();
            InfluxDBClient client = values.Item1;
            string bucket = values.Item2;
            string org = values.Item3;

            WriteDataToDataBase(client, bucket, org);
            client = values.Item1;
            QueryDataAndPlotGraph(client, org);

        }

        private void formsPlot2_Load_1(object sender, EventArgs e)
        {

            var values = InitializeClient();
            InfluxDBClient client = values.Item1;
            string org = values.Item3;

            QueryDataAndPlotGraph(client, org);

        }

        
        private void button3_Click(object sender, EventArgs e)
        {
            timer = new System.Timers.Timer(2000);
            timer.Elapsed += new ElapsedEventHandler(PlotTheGraphLive);

            timer.Interval = 1000;
            timer.Enabled = true;
        }

        public void PlotTheGraphLive(object source, ElapsedEventArgs e)
        {
            var values = InitializeClient();
            InfluxDBClient client = values.Item1;
            string org = values.Item3;

            QueryDataAndPlotGraph(client, org);
        }
        


        (InfluxDBClient, string, string) InitializeClient()
        {
            //Initialize The Client
            string token_ = "INFLUX_TOKEN";
            Environment.SetEnvironmentVariable(token_, "aG21wdV8_IYfyNLh_MSUoRd6a03CWu0t-aH1sAiUdiDj3Qp3FtTEhDsD7hKZ8ndDqBUahlFtzLlA5rxO7djb5A==");

            var token = Environment.GetEnvironmentVariable(token_)!;
            const string bucket = "Database";
            const string org = "yildirimbeyazit2";

            var client = InfluxDBClientFactory.Create("http://localhost:8086", token);

            return (client, bucket, org);
        }


        private void WriteDataToDataBase(InfluxDBClient client_, string bucket_, string org_ )
        {
            //InitializeClient();

            List<double> valueList = new List<double>();
            List<DateTime> timeList_ = new List<DateTime>();

            var point = PointData
            .Measurement("mem")
            .Tag("host", "host1")
            .Field("used_percent", 24.43234543)
            .Timestamp(DateTime.UtcNow, WritePrecision.Ns);

            using (var writeApi = client_.GetWriteApi())
            {
                writeApi.WritePoint(point, bucket_, org_);
            }
        }

        private async void QueryDataAndPlotGraph(InfluxDBClient client, string org_)
        {

            List<double> valueList = new List<double>();
            List<DateTime> timeList_ = new List<DateTime>();
            //Flux Query
            var query = "from(bucket: \"Database\") |> range(start: -1h)";
            var tables = await client.GetQueryApi().QueryAsync(query, org_);

            double value_ = 0;
            string time_ = "";
            DateTime time_d = System.DateTime.Now;

            foreach (var record in tables.SelectMany(table => table.Records).ToList())
            {

                if (record.GetTimeInDateTime() != null)
                {
                    time_ = record.GetTimeInDateTime().ToString();
                }

                if (record.GetValue() is IConvertible)
                {
                    value_ = ((IConvertible)record.GetValue()).ToDouble(null);
                }

                if (time_ != null)
                {
                    time_d = DateTime.Parse(time_);
                }

                timeList_.Add(time_d);
                valueList.Add(value_);

            }

            double[] timeArray = timeList_.Select(x => x.ToOADate()).ToArray();
            double[] valueArray = valueList.ToArray();

            if (timeArray != null && valueArray != null)
            {
                formsPlot2.Plot.AddScatter(timeArray, valueArray);
                formsPlot2.Plot.XAxis.DateTimeFormat(true);
                formsPlot2.Render();
            }

            else
            {
                double[] dataX = new double[] { 1, 2, 3, 4, 5 };
                double[] dataY = new double[] { 1, 4, 9, 16, 25 };
                formsPlot2.Plot.AddScatter(dataX, dataY);
                formsPlot2.Render();

            }


        }

        private void panel3_Paint(object sender, PaintEventArgs e)
        {

        }

       
    }
}