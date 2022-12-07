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
        public bool plotTimerStart = false;
        public bool sinTimerStart = false;
        public bool sinTimerStartForAnotherPCStart = false;
        public bool plotTimerOfAnotherPCStart = false;

        public Form1()
        {
            InitializeComponent();
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


        private void button4_Click(object sender, EventArgs e)
        {
            plotTimerStart = false;
        }

        private void button9_Click(object sender, EventArgs e)
        {
            plotTimerOfAnotherPCStart = false;
        }

        private async void button8_Click(object sender, EventArgs e)
        {
            plotTimerOfAnotherPCStart = true;

            while (plotTimerStart)
            {
                await Task.Delay(3000);
                var valuess = Task.Run(async () => await QueryDataOfAnotherPC());
                (double[] x, double[] y) = valuess.Result;


                formsPlot2.Plot.AddScatter(x, y);
                formsPlot2.Plot.XAxis.DateTimeFormat(true);
                //formsPlot2.Render(skipIfCurrentlyRendering: true);
                formsPlot2.Refresh();
            }
        }

        private async void button3_Click(object sender, EventArgs e)
        {
      
            plotTimerStart = true;

            while (plotTimerStart)
            {
                await Task.Delay(3000);
                var valuess = Task.Run(async () => await QueryData());
                (double[] x, double[] y) = valuess.Result;


                formsPlot2.Plot.AddScatter(x, y);
                formsPlot2.Plot.XAxis.DateTimeFormat(true);
                //formsPlot2.Render(skipIfCurrentlyRendering: true);
                formsPlot2.Refresh();
            }

        }

        private async void button5_Click(object sender, EventArgs e)
        {
            sinTimerStart = true;
            float increment = 0;
            while (sinTimerStart)
            {
                await Task.Delay(1000);
                await Task.Run(() => WriteData(increment));
                increment += 0.2f;

                if(increment == 360)
                {
                    increment = 0;
                }
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            sinTimerStart= false;
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

        (InfluxDBClient, string, string) InitializeClientForAnotherPc()
        {
            //Initialize The Client
            string token_ = "INFLUX_TOKEN";
            Environment.SetEnvironmentVariable(token_, "Ljtx7Tje95y6yglhSV0uxXo_5ehFbZ_pbwkJgHxLokDwSi8SBVlYv8ovP7N6bXoVv4OAulsS7iIWEpQk1Bw9ww==");

            var token = Environment.GetEnvironmentVariable(token_)!;
            const string bucket = "Database";
            const string org = "Atilim";

            var client = InfluxDBClientFactory.Create("http://25.20.87.75:8086", token);

            return (client, bucket, org);
        }

        private void WriteSinDataToDataBase(InfluxDBClient client_, string bucket_, string org_, double t)
        {

            var point = PointData
            .Measurement("mem_sin")
            .Tag("host_sin", "host1_sin")
            .Field("used_percent_sin", Math.Sin(t))
            .Timestamp(DateTime.UtcNow, WritePrecision.Ns);

            using (var writeApi = client_.GetWriteApi())
            {
                writeApi.WritePoint(point, bucket_, org_);
            }
        }

        private async void button7_Click(object sender, EventArgs e)
        {
            sinTimerStartForAnotherPCStart = true;
            float increment = 0;
            while (sinTimerStart)
            {
                await Task.Delay(1000);
                await Task.Run(() => WriteDataToAnotherPC(increment));
                increment += 0.2f;

                if (increment == 360)
                {
                    increment = 0;
                }
            }

            
        }

        private void button10_Click(object sender, EventArgs e)
        {
            sinTimerStartForAnotherPCStart = false;
        }


        private void WriteDataToDataBase(InfluxDBClient client_, string bucket_, string org_ )
        {
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
            //var query = "from(bucket: \"Database\") |> range(start: 2019-08-28T22:00:00Z) |> filter(fn: (r) => r.measurement == \"mem\" and r.field == \"used_percent\" and r.host ==  \"host1\")";
            var query = " from(bucket: \"Database\") |> range(start: 2019-08-28T22:00:00Z) |> filter(fn: (r) => r._measurement == \"mem\")";
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

        private async Task<(double[], double[])> QueryData()
        {
            string token_ = "INFLUX_TOKEN";
            Environment.SetEnvironmentVariable(token_, "aG21wdV8_IYfyNLh_MSUoRd6a03CWu0t-aH1sAiUdiDj3Qp3FtTEhDsD7hKZ8ndDqBUahlFtzLlA5rxO7djb5A==");

            var token = Environment.GetEnvironmentVariable(token_)!;
            const string org = "yildirimbeyazit2";

            var client = InfluxDBClientFactory.Create("http://localhost:8086", token);

            /////////////////////////////////////////////////////////

            List<double> valueList = new List<double>();
            List<DateTime> timeList_ = new List<DateTime>();
            //Flux Query
            //var query = " from(bucket: \"Database\") |> range(start: 2019-08-28T22:00:00Z) |> filter(fn: (r) => r._measurement == \"mem\")";
            var query = " from(bucket: \"Database\") |> range(start: 2019-08-28T22:00:00Z) |> filter(fn: (r) => r._measurement == \"mem_sin\")";
            var tables = await client.GetQueryApi().QueryAsync(query, org);

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

            return (timeArray, valueArray);
        
        }

        private async Task<(double[], double[])> QueryDataOfAnotherPC()
        {
            string token_ = "INFLUX_TOKEN";
            Environment.SetEnvironmentVariable(token_, "Ljtx7Tje95y6yglhSV0uxXo_5ehFbZ_pbwkJgHxLokDwSi8SBVlYv8ovP7N6bXoVv4OAulsS7iIWEpQk1Bw9ww==");

            var token = Environment.GetEnvironmentVariable(token_)!;
            const string org = "Atilim";

            var client = InfluxDBClientFactory.Create("http://25.20.87.75:8086", token);

            /////////////////////////////////////////////////////////

            List<double> valueList = new List<double>();
            List<DateTime> timeList_ = new List<DateTime>();
            //Flux Query
            //var query = " from(bucket: \"Database\") |> range(start: 2019-08-28T22:00:00Z) |> filter(fn: (r) => r._measurement == \"mem\")";
            var query = " from(bucket: \"Database\") |> range(start: 2019-08-28T22:00:00Z) |> filter(fn: (r) => r._measurement == \"mem_sin\")";
            var tables = await client.GetQueryApi().QueryAsync(query, org);

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

            return (timeArray, valueArray);

        }

        private void WriteData(float t)
        {

            var values = InitializeClient();
            InfluxDBClient client_ = values.Item1;
            string bucket_ = values.Item2;
            string org_ = values.Item3;

            /////////////////////////////////////////////////////////
            //Write Data

            var point = PointData
            .Measurement("mem_sin")
            .Tag("host_sin", "host1_sin")
            .Field("used_percent_sin", Math.Sin(t))
            .Timestamp(DateTime.UtcNow, WritePrecision.Ns);

            using (var writeApi = client_.GetWriteApi())
            {
                writeApi.WritePoint(point, bucket_, org_);
            }


        }

        private void WriteDataToAnotherPC(float t)
        {

            var values = InitializeClientForAnotherPc();
            InfluxDBClient client_ = values.Item1;
            string bucket_ = values.Item2;
            string org_ = values.Item3;

            /////////////////////////////////////////////////////////
            //Write Data

            var point = PointData
            .Measurement("mem_sin")
            .Tag("host_sin", "host1_sin")
            .Field("used_percent_sin", Math.Sin(t))
            .Timestamp(DateTime.UtcNow, WritePrecision.Ns);

            using (var writeApi = client_.GetWriteApi())
            {
                writeApi.WritePoint(point, bucket_, org_);
            }

            

        }

        private void panel3_Paint(object sender, PaintEventArgs e)
        {

        }

        
    }
}