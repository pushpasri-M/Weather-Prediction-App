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
    public class LocationAPI
    {
        private readonly HttpClient _httpClient;
        private const string BaseUrl = "https://geocoding-api.open-meteo.com/v1/";
        public LocationAPI()
        {
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri(BaseUrl);

        }
        public async Task<(double longitude, double latitude)> GetValueAsync(string cityName)
        {
            var response = await _httpClient.GetAsync($"search?name={cityName}");
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();

                // Deserialize properly into LocationResponse
                var data = JsonConvert.DeserializeObject<LocationResponse>(content);

                if (data != null && data.results != null && data.results.Count > 0)
                {
                    double latitude = data.results[0].latitude;
                    double longitude = data.results[0].longitude;
                    return (longitude, latitude);
                }
            }
            throw new Exception("City not found or API error");
        }


    }
}
