using Newtonsoft.Json;
using NotePad_MVP.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace NotePad_MVP.Service
{
    /// <summary>
    /// Implementation of location service using Open-Meteo Geocoding API
    /// </summary>
    public class LocationAPI : ILocationService
    {
        private readonly HttpClient _httpClient;
        private const string BaseUrl = "https://geocoding-api.open-meteo.com/v1/";
        
        public LocationAPI() : this(new HttpClient())
        {
        }

        public LocationAPI(HttpClient httpClient)
        {
            _httpClient = httpClient;
            if (_httpClient.BaseAddress == null)
            {
                _httpClient.BaseAddress = new Uri(BaseUrl);
            }
        }

        public async Task<LocationResult> GetLocationAsync(string cityName)
        {
            var response = await _httpClient.GetAsync($"search?name={cityName}");
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();

                // Deserialize properly into LocationResponse
                var data = JsonConvert.DeserializeObject<LocationResponse>(content);

                if (data != null && data.results != null && data.results.Count > 0)
                {
                    return data.results[0];
                }
            }
            throw new Exception("City not found or API error");
        }
    }
}
