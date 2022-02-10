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
            List<Device> device_records = new List<Device>();
            List<Data> data_records = new List<Data>();
            string device_folder = "";
            string data_folder = "";
            
            var conf = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HeaderValidated = null,
                MissingFieldFound = null,
            };
            
            /* Get folder where device csv's are saved from user */
            bool device_folder_exists = false;
            
            while (!device_folder_exists)
            {
                Console.Write("Please enter path to Device folder:");
                device_folder = Console.ReadLine();
                if (Directory.Exists(device_folder))
                {
                    device_folder_exists = true;
                }
            }
            string[] device_files = 
                Directory.GetFiles(device_folder, "*.csv", SearchOption.AllDirectories);
            
            /* Get folder where data csv's are saved from user */
            bool data_folder_exists = false;
            
            while (!data_folder_exists)
            {
                Console.Write("Please enter path to Data folder:");
                data_folder = Console.ReadLine();
                if (Directory.Exists(data_folder))
                {
                    data_folder_exists = true;
                }
            }
            string[] data_files = 
                Directory.GetFiles(data_folder, "*.csv", SearchOption.AllDirectories);
            
            /* Read device information from csv's */
            for (int i = 0; i < device_files.Length; i++)
            {
                using (var reader = new StreamReader($@"{device_files[i]}"))
                using (var csv = new CsvReader(reader, conf))
                {
                    device_records = device_records.Concat(csv.GetRecords<Device>().ToList()).ToList();
                }
            }
            
            /* Read rainfall data information from csv's */
            for (int i = 0; i < data_files.Length; i++)
            {
                using (var reader = new StreamReader($@"{data_files[i]}"))
                using (var csv = new CsvReader(reader, conf))
                {
                    data_records = data_records.Concat(csv.GetRecords<Data>().ToList()).ToList();
                }
            }
            
            /* Sort rainfall data by if it's been more than 4 hours since data was collected or not */
            data_records.Sort((x, y) => x.time.CompareTo(y.time));
            DateTime now = data_records[data_records.Count - 1].time;
            DateTime four_hours_ago = now.AddHours(-4);
            var filtered_records = data_records.Where(o => o.time >= four_hours_ago).ToList();
            
            /* Save rainfall data onto Device objects */
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
            
            /* Display rainfall information */
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
                    Console.WriteLine($"{dev.name} (in {dev.location}): Avg Rainfall - {Math.Round(current_avg, 2)}mm; Trend - {trend}; ");
                }
            }
        }
    }
}