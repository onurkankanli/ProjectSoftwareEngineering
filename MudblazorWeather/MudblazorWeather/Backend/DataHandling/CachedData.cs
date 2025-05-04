using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using MudblazorWeather.Backend.Networking;
using MudblazorWeather.Graphs;
using WeatherClient.Extensions.CGTK.Utilities.Singletons;

namespace MudblazorWeather.Backend.DataHandling
{
    using DataContainer = Dictionary<DataType, Dictionary<string, List<SensorData>>>;

    public delegate void OnNewDataReceived();
    public delegate void OnRefreshRequired();
    
    public class CachedData : EnsuredSingleton<CachedData>
    {
        public DateTime From { get; private set; }
        public DateTime To { get; private set; }
        public TimeSpan TimeSpan => To - From;
        public DateTime EarliestDate => DateTime.Parse("2021-12-10");

        public bool IsInitialized
        {
            get;
            private set;
        }
        public event OnNewDataReceived OnNewDataReceivedEvent;
        public event OnRefreshRequired OnRefreshRequiredEvent;

        public bool showStudentSensors = true;

        private List<string> providedSensors = new List<string>()
        {
            "py-wierden",
            "py-saxion",
            "lht-gronau",
            "lht-wierden"
        };

        public DataContainer ProcessedData { get; private set; }
        public Dictionary<DataType, List<string>> Sensors { get; private set; }

        public int requiredDatapointCount = 40;
        
        public void SetTimeSpan(DateTime to, TimeSpan timeSpan)
        {
            From = to - timeSpan;
            To = to + TimeSpan.FromSeconds(1);
            
            //Remove seconds. The server-side averaging algorithm averages with a maximum accuracy on minutes.
            //From = new DateTime(From.Year, From.Month, From.Day, From.Hour, From.Minute, 0);
            //To = new DateTime(To.Year, To.Month, To.Day, To.Hour, To.Minute, 0);
        }
        public void SetTimeSpan(DateTime from, DateTime to)
        {
            From = from;
            To = to;
            
            //Remove seconds. The server-side averaging algorithm averages with a maximum accuracy on minutes.
            //From = new DateTime(From.Year, From.Month, From.Day, From.Hour, From.Minute, 0);
            //To = new DateTime(To.Year, To.Month, To.Day, To.Hour, To.Minute, 0);
        }

        public void ToggleStudentSensors()
        {
            OnRefreshRequiredEvent?.Invoke();
        }

        public Task<List<CustomChartSeries>> GetChartSeries(DataType type)
        {
            List<CustomChartSeries> series = new List<CustomChartSeries>();

            for (int i = 0; i < Sensors[type].Count; i++)
            {
                string sensor = Sensors[type][i];
                
                //If user turned off the student sensors, only add the built-in ones.
                if (!showStudentSensors)
                {
                    if (providedSensors.Contains(sensor))
                    {
                        series.Add(new CustomChartSeries(sensor, ProcessedData[type][sensor].ToArray()));
                    }
                }
                else
                {
                    series.Add(new CustomChartSeries(sensor, ProcessedData[type][sensor].ToArray()));
                }
            }
            
            return Task.FromResult(series);
        }
        public Task<List<CustomHeatSeries>> GetHeatSeries(DataType type)
        {
            List<CustomHeatSeries> series = new List<CustomHeatSeries>();

            for (int i = 0; i < Sensors[type].Count; i++)
            {
                string sensor = Sensors[type][i];
                if (!showStudentSensors)
                {
                    if (providedSensors.Contains(sensor))
                    {
                        series.Add(new CustomHeatSeries(sensor, ProcessedData[type][sensor].ToArray()));
                    }
                }
                else
                {
                    series.Add(new CustomHeatSeries(sensor, ProcessedData[type][sensor].ToArray()));
                }
            }
            
            return Task.FromResult(series);
        }
        
        public async Task SetAllData(DataContainer data)
        {
            ProcessedData = await VerifyDataIntegrity(data, TimeSpan, requiredDatapointCount);
            Sensors = new Dictionary<DataType, List<string>>();

            foreach (DataType type in data.Keys)
            {
                Sensors.Add(type, data[type].Keys.ToList());
            }

            IsInitialized = true;
            
            OnNewDataReceivedEvent?.Invoke();
        }
        public async Task AppendData(DataContainer data, TimeSpan range, int newDatapointCount)
        {
            if (!IsInitialized) return;
            
            DataContainer newData = await VerifyDataIntegrity(data, range, newDatapointCount, true);
            System.TimeSpan rangeBetweenElements = range / newDatapointCount;

            //Remove the old data we no longer need.
            foreach (DataType type in ProcessedData.Keys)
            {
                foreach (string sensor in ProcessedData[type].Keys)
                {
                    ProcessedData[type][sensor].RemoveRange(0, newDatapointCount);
                    
                    if (!newData[type].ContainsKey(sensor))
                    {
                        int count = ProcessedData[type][sensor].Count;
                        for (int i = 0; i < newDatapointCount; i++)
                        {
                            ProcessedData[type][sensor].Add(new SensorData(ProcessedData[type][sensor][count++].timestamp + rangeBetweenElements, 0));
                        }
                    }
                }
            }

            //Add new data
            foreach (DataType type in newData.Keys)
            {
                foreach (string sensor in newData[type].Keys)
                {
                    if (ProcessedData[type].ContainsKey(sensor))
                    {
                        ProcessedData[type][sensor].AddRange(newData[type][sensor]);
                    }
                    else
                    {
                        for (int i = 0; i < requiredDatapointCount - newDatapointCount; i++)
                        {
                            ProcessedData[type][sensor].Add(new SensorData(From + rangeBetweenElements * i, 0));
                        }
                        
                        ProcessedData[type][sensor].AddRange(newData[type][sensor]);
                    }
                }
            }
            
            OnNewDataReceivedEvent?.Invoke();
        }
        private async Task<DataContainer> VerifyDataIntegrity(DataContainer data, TimeSpan range, int expectedDatapoints, bool appending = false)
        {
            DateTime maxDate = To;
            DateTime minDate = To - range;

            System.TimeSpan expectedRangeBetweenElements = range / expectedDatapoints;

            await Task.Run(async () =>
            {
                await Task.Yield();
                foreach (DataType type in data.Keys)
                {
                    foreach (string sensor in data[type].Keys)
                    {
                        if(data[type][sensor].Count == expectedDatapoints) continue;
                        
                        Console.WriteLine($"Sensor: |{type.ToString()}|{sensor}| has an invalid amount of datapoints: [{data[type][sensor].Count}]");
                        
                        if (data[type][sensor][0].timestamp - minDate > expectedRangeBetweenElements) //first element missing
                        {
                            if (appending && ProcessedData[type].ContainsKey(sensor))
                            {
                                data[type][sensor].Insert(0, new SensorData(minDate, ProcessedData[type][sensor][ProcessedData[type][sensor].Count-1].value));
                            }
                            else
                            {
                                data[type][sensor].Insert(0, new SensorData(minDate, 0));
                            }
                        }


                        for (int i = 0; i < data[type][sensor].Count - 1; i++)
                        {
                            TimeSpan timeBetweenElements = data[type][sensor][i + 1].timestamp - data[type][sensor][i].timestamp;
                            if (timeBetweenElements > expectedRangeBetweenElements) //Missing element
                            {
                                int missingElementCount = (int)Math.Ceiling(timeBetweenElements.TotalMinutes / expectedRangeBetweenElements.TotalMinutes)-1;

                                double startingValue = data[type][sensor][i].value;

                                if (startingValue == 0)
                                {
                                    for (int j = 1; j <= missingElementCount; j++)
                                    {
                                        data[type][sensor].Insert(i + j,
                                            new SensorData(data[type][sensor][i + (j-1)].timestamp + expectedRangeBetweenElements, 0));
                                    }
                                }
                                else
                                {
                                    double averageIncrease = (data[type][sensor][i + 1].value - data[type][sensor][i].value) / missingElementCount;
                                
                                    for (int j = 1; j <= missingElementCount; j++)
                                    {
                                        data[type][sensor].Insert(i + j,
                                            new SensorData(data[type][sensor][i + (j-1)].timestamp + expectedRangeBetweenElements, Math.Round(startingValue + (averageIncrease * j))));
                                    }
                                }
                            }
                        }

                        int lastElement = data[type][sensor].Count - 1;

                        if (lastElement != expectedDatapoints - 1)
                        {
                            for (int i = lastElement; i < expectedDatapoints-1; i++)
                            {
                                if (maxDate - data[type][sensor][i].timestamp > expectedRangeBetweenElements)
                                {
                                    data[type][sensor].Insert(i + 1,
                                        new SensorData(data[type][sensor][i].timestamp + expectedRangeBetweenElements, 0));
                                }
                            }
                        }

                        //Console.WriteLine($"Sensor: |{type.ToString()}|{sensor}|: {data[type][sensor].Count}");
                        if (data[type][sensor].Count != expectedDatapoints)
                        {
                            throw new ConstraintException(
                                $"Received and parsed data from sensor: |{type.ToString()}|{sensor}| " +
                                $"has an incorrect amount of data points: Expected: {expectedDatapoints}. Got: {data[type][sensor].Count}.");
                        }
                    }
                }
            });

            return data;
        }
    }
}

/*
 Console.WriteLine("-------------------------------------------------------");
            
if (data[type][sensor][0].timestamp != minDate)
                    {
                        Console.Write($"FROM: |{sensor}|{type.ToString()}| Got: {data[type][sensor][0].timestamp} Expected: {minDate}");
                        
                        if (data[type][sensor].Count < expectedDatapoints) //less than expected values
                        {
                            //figure out which 
                            Console.WriteLine($" Which misses {expectedDatapoints - data[type][sensor].Count} datapoints");

                            for (int i = 0; i < data[type][sensor].Count-1; i++)
                            {
                                if ((data[type][sensor][i + 1].timestamp - data[type][sensor][i].timestamp) !=
                                    rangeBetweenElements)
                                {
                                    Console.WriteLine($"The timerange between element [{i}] and [{i+1}] is incorrect");
                                }
                            }
                        }
                        else
                        {
                            Console.WriteLine("");
                        }
                    }
                    Console.WriteLine("-------------------------------------------------------");
            
*/