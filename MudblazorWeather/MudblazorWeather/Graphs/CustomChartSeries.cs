using System;
using System.Collections.Generic;
using System.Globalization;
using MudblazorWeather.Backend.DataHandling;

namespace MudblazorWeather.Graphs
{
    public class CustomChartSeries
    {
        private string name;
        private double[] data;
        private DateTime[] timestamps;
        
        public CustomChartSeries(string name, SensorData[] data)
        {
            this.Name = name;

            List<double> dataList = new List<double>();
            List<DateTime> timeList = new List<DateTime>();
            
            foreach (SensorData point in data)
            {
                dataList.Add(point.value);
                timeList.Add(point.timestamp);
            }

            this.Data = dataList.ToArray();
            this.Timestamps = timeList.ToArray();
        }
        
        public string Name { get; set; }

        public double[] Data { get; set; }
        
        public DateTime[] Timestamps { get; set; }
    }
    public class CustomHeatSeries
    {
        public CustomHeatSeries(string name, SensorData[] data)
        {
            List<HeatData> dataList = new List<HeatData>();
            
            foreach (SensorData point in data)
            {
                dataList.Add(new HeatData( x: point.timestamp.ToString(), y:point.value));
            }

            this.name = name;
            this.data = dataList;
        }
        
        public List<HeatData> data { get; set; }
        public string name { get; set; }
    }
    public class HeatData
    {
        public HeatData( string x, double y)
        {
            this.x = x;
            this.y = y;
        }

        public string x { get; set; }

        public double y { get; set; }
		
    }
}