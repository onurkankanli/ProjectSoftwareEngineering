using System;
using System.Collections.Generic;
using MudblazorWeather.Backend.DataHandling;
using WeatherClient.Extensions.CGTK.Utilities.Singletons;

namespace MudblazorWeather.Backend.DataHandling
{
	public class ToggleData : EnsuredSingleton<ToggleData>
	{
		public Dictionary<string, bool> data = new Dictionary<string, bool>()
		{
			{"lht_gronau", true},
			{"lht_wierden", true},
			{"py_saxion", true},
			{"py_wierden", true}
		};

		public bool IsToggled(string sensor) => data[sensor];

		public bool IsReadOnly(string sensor)
		{
			if (EnabledCount() < 5) return false;

			return !data[sensor];
		}

		private int EnabledCount()
		{
			int count = 0;
			foreach (bool val in data.Values)
			{
				if (val) count++;
			}

			return count;
		}
	}
}