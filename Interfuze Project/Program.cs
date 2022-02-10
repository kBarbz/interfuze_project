using System.Globalization;
using System.Linq;
using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.Configuration.Attributes;

namespace Interfuze_Project
{
    public class Device
    {
        [Name("Device ID")]
        public int? deviceId { get; set; }
        [Name("Device Name")]
        public string name { get; set; }
        [Name("Location")]
        public string location { get; set; }
        public List<Data> rainfall_data = new List<Data>();
        public List<Data> previous_rainfall_data = new List<Data>();
        public bool is_red = false;

    }
    public class Data
    {
        [Name("Device ID")]
        public int? deviceId { get; set; }
        [Name("Time")]
        public DateTime time { get; set; }
        [Name("Rainfall")]
        public int? rainfall { get; set; }
        
    }
    class Program
    {
        static void Main(string[] args)
        {
            List<Device> device_records;
            List<Data> data_records;
            List<Data> more_data_records;
            
            var conf = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HeaderValidated = null,
                MissingFieldFound = null,
            };

            
            using (var reader = new StreamReader(@"csv/Devices.csv"))
            using (var csv = new CsvReader(reader, conf))
            {
                device_records = csv.GetRecords<Device>().ToList();
            }
            
            using (var reader = new StreamReader(@"csv/Data2.csv"))
            using (var csv = new CsvReader(reader, conf))
            {
                data_records = csv.GetRecords<Data>().ToList();
            }
            
            using (var reader = new StreamReader(@"csv/Data1.csv"))
            using (var csv = new CsvReader(reader, conf))
            {
                more_data_records = csv.GetRecords<Data>().ToList();
            }
            
            more_data_records.ForEach(item => data_records.Add(item));
            data_records.Sort((x, y) => x.time.CompareTo(y.time));
            DateTime now = data_records[data_records.Count - 1].time;
            DateTime four_hours_ago = now.AddHours(-4);
            var filtered_records = data_records.Where(o => o.time >= four_hours_ago).ToList();

            for (int i = 0; i < data_records.Count; i++)
            {
                for (int j = 0; j < device_records.Count; j++)
                {
                    if (device_records[j].deviceId == data_records[i].deviceId)
                    {
                        if (filtered_records.Contains(data_records[i]))
                        {
                            if (data_records[i].rainfall > 30)
                            {
                                device_records[j].is_red = true;
                            }
                            device_records[j].rainfall_data.Add(data_records[i]);
                        }
                        else
                        {
                            device_records[j].previous_rainfall_data.Add(data_records[i]);
                        }
                    }
                }

            }

            foreach (var dev in device_records)
            {
                if (dev != null && dev.deviceId != null)
                {
                    var current_avg = (double)dev.rainfall_data.Sum(i => i.rainfall) / dev.rainfall_data.Count;
                    var previous_average = (double)dev.previous_rainfall_data.Sum(i => i.rainfall)  / dev.previous_rainfall_data.Count;
                    var trend = (current_avg > previous_average) ? "Increase"  : "Decrease";
                    if (dev.is_red == true || current_avg >= 15.0)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                    }
                    else if (current_avg < 10)
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Yellow;
                    }
                    Console.WriteLine($"{dev.name} (in {dev.location}): Avg Rainfall - {Math.Round(current_avg, 2)}; Trend - {trend}; ");
                }
            }
        }
    }
}