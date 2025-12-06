using NotePad_MVP.Model;
using System.Threading.Tasks;

namespace NotePad_MVP.Service
{
    /// <summary>
    /// Service interface for weather data operations
    /// </summary>
    public interface IWeatherService
    {
        /// <summary>
        /// Gets weather data for given coordinates
        /// </summary>
        /// <param name="latitude">Latitude coordinate</param>
        /// <param name="longitude">Longitude coordinate</param>
        /// <returns>WeatherData containing current and forecast information</returns>
        Task<WheatherData> GetWeatherDataAsync(double latitude, double longitude);
    }
}
