using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace NotePad_MVP.Service
{
    public class WeatherAPI
    {
        private readonly HttpClient httpClient;
        private const string BaseUrl = "https://api.open-meteo.com/v1/";
        public WeatherAPI()
        {
            httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri(BaseUrl);
        }
        public async Task<string> GetWeatherAsync(double longitude, double latitude)
        {
            var url = $"forecast?latitude={latitude}&longitude={longitude}" +
                     $"&current_weather=true" +
                     $"&hourly=temperature_2m,relative_humidity_2m,apparent_temperature,precipitation_probability,uv_index" +
                     $"&daily=temperature_2m_max,temperature_2m_min,sunrise,sunset,precipitation_probability_max,uv_index_max,weather_code" +
                     $"&timezone=auto&forecast_days=7";
            
            var response = await httpClient.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                return content;
            }
            throw new Exception("Weather data not found or API error");
        }

    } 
}
