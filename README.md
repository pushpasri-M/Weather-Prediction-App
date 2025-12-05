# Weather Prediction App (MVP)

A modern, premium-styled Windows Forms application built with C# that provides real-time weather forecasts, hourly temperature trends, and daily rainfall probabilities.

![Weather App Screenshot](https://via.placeholder.com/800x450?text=Weather+App+Preview)
*(Note: Replace with actual screenshot)*

## Features

-   **Modern UI**: sleek, borderless window design with a premium dark blue theme (`#0F172A`).
-   **Real-time Weather**: Fetches current temperature, wind speed, wind direction, and weather conditions.
-   **Interactive Charts**:
    -   **Temperature Trend**: 24-hour hourly temperature spline chart.
    -   **Rainfall Probability**: 7-day bar chart for precipitation info.
-   **Smart Search**: Search for any city to get instant weather data.
-   **Robust Error Handling**: Custom-styled error messages that match the application theme.
-   **Responsive Layout**: Custom window controls (Minimize, Maximize, Close) and draggable title bar.

## Technologies Used

-   **Language**: C#
-   **Framework**: .NET Framework (Windows Forms)
-   **Libraries**:
    -   `Newtonsoft.Json` (for API data parsing)
    -   `System.Windows.Forms.DataVisualization` (for Charts)
-   **API**: [Open-Meteo](https://open-meteo.com/) (Free Weather API)

## Getting Started

### Prerequisites

-   Windows OS
-   .NET Framework 4.7.2 or later
-   Visual Studio 2019/2022

### Installation

1.  **Clone the repository**
    ```bash
    git clone https://github.com/yourusername/weather-prediction-app.git
    cd weather-prediction-app
    ```

2.  **Open in Visual Studio**
    -   Open `NotePad MVP.sln`

3.  **Restore Packages**
    -   Right-click on the solution in Solution Explorer.
    -   Select **Restore NuGet Packages**.

4.  **Build and Run**
    -   Press `F5` or click **Start** to run the application.

## Project Structure

-   `Form1.cs`: Main UI logic, custom styling, and event handlers.
-   `Presenter/`: Contains `WeatherFunction.cs` handling business logic and view updates.
-   `Service/`: API services (`WeatherAPI.cs`, `LocationAPI.cs`).
-   `Model/`: Data models (`WheatherData.cs`, `LocationResult.cs`).

## Contributing

1.  Fork the project.
2.  Create your feature branch (`git checkout -b feature/AmazingFeature`).
3.  Commit your changes (`git commit -m 'Add some AmazingFeature'`).
4.  Push to the branch (`git push origin feature/AmazingFeature`).
5.  Open a Pull Request.

## License

Distributed under the MIT License. See `LICENSE` for more information.
