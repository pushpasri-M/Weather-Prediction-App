using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NotePad_MVP.Model
{
    public class WheatherData
    {
        // Current weather
        public double temperature { get; set; }
        public double windspeed { get; set; }
        public double winddirection { get; set; }
        public string weathercode { get; set; }
        public string WeatherDescription { get; set; }
        public int is_day { get; set; }
        public string time { get; set; }

        // Hourly forecast (existing)
        public List<string> HourlyTime { get; set; }
        public List<double> HourlyTemperature { get; set; }

        // Enhanced hourly data
        public List<int> HourlyHumidity { get; set; }
        public List<double> HourlyFeelsLike { get; set; }
        public List<int> HourlyPrecipitationProbability { get; set; }
        public List<double> HourlyUVIndex { get; set; }

        // Daily forecast (7-day)
        public List<string> DailyTime { get; set; }
        public List<double> DailyTempMax { get; set; }
        public List<double> DailyTempMin { get; set; }
        public List<string> DailySunrise { get; set; }
        public List<string> DailySunset { get; set; }
        public List<int> DailyPrecipitationProbability { get; set; }
        public List<double> DailyUVIndex { get; set; }
        public List<int> DailyWeatherCode { get; set; }

    }
}
