using Newtonsoft.Json.Linq;
using NotePad_MVP.Model;
using NotePad_MVP.Service;
using NotePad_MVP.view;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NotePad_MVP.Presenter
{
    public class WeatherFunction
    {
        private readonly IViewForm viewForm;
        private readonly LocationAPI _location;
        private readonly WeatherAPI _weather;
        public WeatherFunction(IViewForm viewForm)
        {
            this.viewForm = viewForm;
            _location = new LocationAPI();
            _weather = new WeatherAPI();
        }
        public async Task FetchAndDisplayWeatherAsync()
        {
            try
            {
                viewForm.ShowLoading(); // Show loading indicator
                
                var cityName = viewForm.cityName;
                var location = await _location.GetValueAsync(cityName);
                var weatherInfo = await _weather.GetWeatherAsync(location.longitude, location.latitude);
                var json = JObject.Parse(weatherInfo);

                var currentWeather = json["current_weather"];
                var hourly = json["hourly"];
                var daily = json["daily"]; // Extract daily forecast data
                string weathercodeConversion = (string)currentWeather["weathercode"];
                string description = WeatherDescription(weathercodeConversion);
                var data = new WheatherData
                {
                    temperature = (double)currentWeather["temperature"],
                    windspeed = (double)currentWeather["windspeed"],
                    winddirection = (int)currentWeather["winddirection"],
                    is_day = ((int)currentWeather["is_day"]),
                    time = (string)currentWeather["time"],
                    weathercode = (string)currentWeather["weathercode"],
                    WeatherDescription = description,
                    
                    // Hourly data
                    HourlyTime = hourly["time"]?.ToObject<List<string>>() ?? new List<string>(),
                    HourlyTemperature = hourly["temperature_2m"]?.ToObject<List<double>>() ?? new List<double>(),
                    HourlyHumidity = hourly["relative_humidity_2m"]?.ToObject<List<int>>() ?? new List<int>(),
                    HourlyFeelsLike = hourly["apparent_temperature"]?.ToObject<List<double>>() ?? new List<double>(),
                    HourlyPrecipitationProbability = hourly["precipitation_probability"]?.ToObject<List<int>>() ?? new List<int>(),
                    HourlyUVIndex = hourly["uv_index"]?.ToObject<List<double>>() ?? new List<double>(),
                    
                    // Daily data
                    DailyTime = daily?["time"]?.ToObject<List<string>>() ?? new List<string>(),
                    DailyTempMax = daily?["temperature_2m_max"]?.ToObject<List<double>>() ?? new List<double>(),
                    DailyTempMin = daily?["temperature_2m_min"]?.ToObject<List<double>>() ?? new List<double>(),
                    DailySunrise = daily?["sunrise"]?.ToObject<List<string>>() ?? new List<string>(),
                    DailySunset = daily?["sunset"]?.ToObject<List<string>>() ?? new List<string>(),
                    DailyPrecipitationProbability = daily?["precipitation_probability_max"]?.ToObject<List<int>>() ?? new List<int>(),
                    DailyUVIndex = daily?["uv_index_max"]?.ToObject<List<double>>() ?? new List<double>(),
                    DailyWeatherCode = daily?["weather_code"]?.ToObject<List<int>>() ?? new List<int>()
                };

                viewForm.SetWeatherInfo(data);
                viewForm.HideLoading(); // Hide loading indicator
            }
            catch (Exception ex)
            {
                viewForm.HideLoading(); // Hide loading indicator on error
                viewForm.ShowError(ex.Message);
            }
        }
        public string WeatherDescription(string code)
        {
            // This method can be implemented to set weather description based on weather code
            switch (code)
            {
                case "0":
                    return "Clear sky";

                case "1":
                case "2":
                case "3":
                    return "Mainly clear, partly cloudy, or overcast";

                case "45":
                case "48":
                    return "Fog and depositing rime fog";

                case "51":
                case "53":
                case "55":
                    return "Drizzle: Light, moderate, or dense intensity";

                case "56":
                case "57":
                    return "Freezing Drizzle: Light or dense intensity";

                case "61":
                case "63":
                case "65":
                    return "Rain: Slight, moderate, or heavy intensity";

                case "66":
                case "67":
                    return "Freezing Rain: Light or heavy intensity";

                case "71":
                case "73":
                case "75":
                    return "Snow fall: Slight, moderate, or heavy intensity";

                case "77":
                    return "Snow grains";

                case "80":
                case "81":
                case "82":
                    return "Rain showers: Slight, moderate, or violent";

                case "85":
                case "86":
                    return "Snow showers: Slight or heavy";

                case "95":
                    return "Thunderstorm: Slight or moderate";

                case "96":
                case "99":
                    return "Thunderstorm with hail";

                default:
                    return "Unknown weather code";
            }

        }
    }
    } 
