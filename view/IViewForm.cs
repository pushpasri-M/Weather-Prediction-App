using NotePad_MVP.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NotePad_MVP.view
{
    public interface IViewForm
    {
        string cityName { get; set; }
        void SetWeatherInfo(WheatherData weatherInfo);
        void ShowError(string message);
        void ShowLoading();
        void HideLoading();
    }
}
