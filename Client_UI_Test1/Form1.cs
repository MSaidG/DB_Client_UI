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

/*
    References
    https://learn.microsoft.com/en-us/dotnet/standard/asynchronous-programming-patterns/task-based-asynchronous-pattern-tap
    https://docs.influxdata.com/influxdb/v2.5/
    https://stackoverflow.com/
*/

//To see the data closer, while pressing down mouse rigt click, drag mouse right

namespace Client_UI_Test1
{

    public partial class Form1 : Form //test
    {
        //To Check Timers For Continuous Jobs 
        public bool IsPlotTimerOn = false;
        public bool IsSinTimerOn = false;
        public bool IsSinTimerForRemotePCOn = false;
        public bool IsPlotTimerOfRemotePCOn = false;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load_1(object sender, EventArgs e)
        {
            
        }

        private void QueryAndWrite_Button(object sender, EventArgs e)
        {
            var values = InitializeClient();
            InfluxDBClient client = values.Item1;
            string bucket = values.Item2;
            string org = values.Item3;

            WriteDataToDataBase(client, bucket, org);
            client = values.Item1;
            QueryDataAndPlotGraph(client, org);

        }

        private void QueryDataOfRemotePC_Button(object sender, EventArgs e)
        {
            var values = InitializeClientForRemotePc();
            InfluxDBClient client = values.Item1;
            string org = values.Item3;

            QueryDataAndPlotGraph(client, org);
        }


        //Run on another thread using Task 
        //Task Delay Wait approximately 3 sec for task to complete
        //Especially this fucntion should wait 3 sec because remote accessing is slower than local accessing
        private async void StartPlottingLiveDataOfRemotePC_Button(object sender, EventArgs e)
        {
            IsPlotTimerOfRemotePCOn = true;

            while (IsPlotTimerOfRemotePCOn)
            {
                var valuesRemote = Task.Run(async () => await QueryDataOfRemotePC_Task());
                await Task.Delay(3000);
                (double[] a, double[] b) = valuesRemote.Result;

                formsPlot2.Plot.AddScatter(a, b);
                formsPlot2.Plot.XAxis.DateTimeFormat(true);
                formsPlot2.Render();
               
            }
        }


        //Run on another thread using Task 
        //Task Delay Wait approximately 1 sec for task to complete
        //Task Delay Could be reduced to decrease refreshing time of data
        private async void StartPlottingLiveData_Button(object sender, EventArgs e)
        {

            IsPlotTimerOn = true;

            while (IsPlotTimerOn)
            {
                
                var valuesLocal = Task.Run(async () => await QueryData_Task());
                await Task.Delay(1000);
                (double[] x, double[] y) = valuesLocal.Result;


                formsPlot2.Plot.AddScatter(x, y);
                formsPlot2.Plot.XAxis.DateTimeFormat(true);
                formsPlot2.Render();
                
            }

        }

        //Sending Sin Data every 1 sec and increase value of sin 0.2 every sec
        private async void StartSendingSinData_Button(object sender, EventArgs e)
        {
            IsSinTimerOn = true;
            float increment = 0;
            while (IsSinTimerOn)
            {
                
                await Task.Run(() => WriteData(increment));
                await Task.Delay(1000);
                increment += 0.2f;

                if (increment == 360)
                {
                    increment = 0;
                }
            }
        }

        //Sending Sin Data every 1 sec and increase value of sin 0.2 every sec
        private async void StartSendingSinDataToRemotePC_Button(object sender, EventArgs e)
        {
            IsSinTimerForRemotePCOn = true;
            float increment = 0;
            while (IsSinTimerForRemotePCOn)
            {
                
                await Task.Run(() => WriteDataToRemotePC(increment));
                await Task.Delay(1000);
                increment += 0.2f;

                if (increment == 360)
                {
                    increment = 0;
                }
            }


        }

        //While loading the UI plot the local data
        private void formsPlot2_Load_1(object sender, EventArgs e)
        {

            var values = InitializeClient();
            InfluxDBClient client = values.Item1;
            string org = values.Item3;

            QueryDataAndPlotGraph(client, org);

        }


        //Stop continuous functions
        private void StopPlottingLiveData_Button(object sender, EventArgs e)
        {
            IsPlotTimerOn = false;
        }

        private void StopPlottingLiveDataOfRemotePC_Button(object sender, EventArgs e)
        {
            IsPlotTimerOfRemotePCOn = false;
        }
       
        private void StopSendingSinData_Button(object sender, EventArgs e)
        {
            IsSinTimerOn= false;
        }

        private void StopSendingSinDataToRemotePC_Button(object sender, EventArgs e)
        {
            IsSinTimerForRemotePCOn = false;
        }


        //Initialize local client and return necessary values for Query and Writing operations
        (InfluxDBClient, string, string) InitializeClient()
        {
            //Initialize The Client
            string token_ = "INFLUX_TOKEN";
            //Token ID (you get it when you first create a token)
            Environment.SetEnvironmentVariable(token_, "aG21wdV8_IYfyNLh_MSUoRd6a03CWu0t-aH1sAiUdiDj3Qp3FtTEhDsD7hKZ8ndDqBUahlFtzLlA5rxO7djb5A==");

            var token = Environment.GetEnvironmentVariable(token_)!;
            const string bucket = "Database";
            const string org = "yildirimbeyazit2";
            //Client: "http://(PUBLIC IP):(PORT)" 
            var client = new InfluxDBClient("http://localhost:8086", token);

            return (client, bucket, org);
        }

        //Initialize remote pc client and return necessary values for Query and Writing operations
        (InfluxDBClient, string, string) InitializeClientForRemotePc()
        {
            //Initialize The Client
            string token_ = "INFLUX_TOKEN";
            //Token ID (you get it when you first create a token)
            Environment.SetEnvironmentVariable(token_, "O7cVcjsN0suvWsd2xJZClEvWFibCag6Ti3eUaVnhKoWu5sOD-1ptTH4KgYQnuGo6B9mE9Cu8uq317Wdr-_LBPQ==");

            var token = Environment.GetEnvironmentVariable(token_)!;
            const string bucket = "Database";
            const string org = "Atilim";
            //Client: "http://(PUBLIC IP):(PORT)"
            var client = new InfluxDBClient("http://25.65.174.239:8086?timeout=900000&logLevel=BASIC", token);

            return (client, bucket, org);
        }     

        //Write Data to bucket(Database)
        
        private void WriteDataToDataBase(InfluxDBClient client_, string bucket_, string org_ )
        {
            //measurement is table in sql
            //tags are indexed columns in sql
            //fields are unindexed columns in sql
            //points are rows in sql
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
            //from(bucket: "(BUCKET_NAME)") |> range(start: (TIME)) |> filter(fn: (r) => r._measurement == "(MEASUREMENT NAME)") ";

            List<double> valueList = new List<double>();
            List<DateTime> timeList_ = new List<DateTime>();

            //  var client_ = new InfluxDBClient("http://25.65.174.239:8086?timeout=900000&logLevel=BASIC");
            client.EnableGzip();
            //Flux Query
            //var query = "from(bucket: \"Database\") |> range(start: 2019-08-28T22:00:00Z) |> filter(fn: (r) => r.measurement == \"mem\" and r.field == \"used_percent\" and r.host ==  \"host1\")";
            var query = " from(bucket: \"Database\") |> range(start: 2019-08-28T22:00:00Z) |> filter(fn: (r) => r._measurement == \"mem_sin\") ";
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

            formsPlot2.Plot.AddScatter(timeArray, valueArray);
            formsPlot2.Plot.XAxis.DateTimeFormat(true);
            formsPlot2.Render();


        }

        


        //To be able to return a value from asynchronous operation Task used
        private async Task<(double[], double[])> QueryData_Task()
        {
            string token_ = "INFLUX_TOKEN";
            Environment.SetEnvironmentVariable(token_, "aG21wdV8_IYfyNLh_MSUoRd6a03CWu0t-aH1sAiUdiDj3Qp3FtTEhDsD7hKZ8ndDqBUahlFtzLlA5rxO7djb5A==");

            var token = Environment.GetEnvironmentVariable(token_)!;
            const string org = "yildirimbeyazit2";

            var client = new InfluxDBClient("http://localhost:8086", token);

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


        //To be able to return a value from asynchronous operation Task used
        private async Task<(double[], double[])> QueryDataOfRemotePC_Task()
        {
            string token_ = "INFLUX_TOKEN";
            Environment.SetEnvironmentVariable(token_, "O7cVcjsN0suvWsd2xJZClEvWFibCag6Ti3eUaVnhKoWu5sOD-1ptTH4KgYQnuGo6B9mE9Cu8uq317Wdr-_LBPQ==");

            var token = Environment.GetEnvironmentVariable(token_)!;
            const string org = "Atilim";

            var client = new InfluxDBClient("http://25.65.174.239:8086?timeout=90000", token);

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

        private void WriteDataToRemotePC(float t)
        {

            var values = InitializeClientForRemotePc();
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

        
    }
}