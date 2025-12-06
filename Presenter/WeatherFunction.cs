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
    /// <summary>
    /// Presenter for weather-related operations following MVP pattern
    /// </summary>
    public class WeatherFunction : IWeatherPresenter
    {
        private readonly IViewForm viewForm;
        private readonly ILocationService _locationService;
        private readonly IWeatherService _weatherService;

        public WeatherFunction(IViewForm viewForm) 
            : this(viewForm, new LocationAPI(), new WeatherAPI())
        {
        }

        public WeatherFunction(IViewForm viewForm, ILocationService locationService, IWeatherService weatherService)
        {
            this.viewForm = viewForm;
            this._locationService = locationService;
            this._weatherService = weatherService;
        }

        public async Task SearchWeatherAsync(string cityName)
        {
            try
            {
                viewForm.ShowLoading();
                
                var location = await _locationService.GetLocationAsync(cityName);
                var weatherInfo = await _weatherService.GetWeatherDataAsync(location.latitude, location.longitude);

                viewForm.SetWeatherInfo(weatherInfo);
                viewForm.HideLoading();
            }
            catch (Exception ex)
            {
                viewForm.HideLoading();
                viewForm.ShowError(ex.Message);
            }
        }

        public async Task FetchAndDisplayWeatherAsync()
        {
            var cityName = viewForm.cityName;
            await SearchWeatherAsync(cityName);
        }

        public void ShowSearchView()
        {
            // This method can be called by the view when user clicks Search tab
            // Presenter handles the business logic for view switching
        }

        public void ShowForecastView()
        {
            // This method can be called by the view when user clicks Forecast tab
            // Presenter handles the business logic for view switching
        }
    }
}
