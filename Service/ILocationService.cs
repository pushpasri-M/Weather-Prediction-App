using NotePad_MVP.Model;
using System.Threading.Tasks;

namespace NotePad_MVP.Service
{
    /// <summary>
    /// Service interface for location/geocoding operations
    /// </summary>
    public interface ILocationService
    {
        /// <summary>
        /// Gets location coordinates for a given city name
        /// </summary>
        /// <param name="cityName">Name of the city to search for</param>
        /// <returns>LocationResult containing coordinates and city information</returns>
        Task<LocationResult> GetLocationAsync(string cityName);
    }
}
