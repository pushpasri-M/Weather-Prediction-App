using System;
using System.Threading.Tasks;

namespace NotePad_MVP.Presenter
{
    /// <summary>
    /// Presenter interface for weather-related operations
    /// </summary>
    public interface IWeatherPresenter
    {
        /// <summary>
        /// Searches for weather data for the specified city
        /// </summary>
        Task SearchWeatherAsync(string cityName);

        /// <summary>
        /// Shows the search view tab
        /// </summary>
        void ShowSearchView();

        /// <summary>
        /// Shows the forecast view tab
        /// </summary>
        void ShowForecastView();
    }
}
