using UnityEngine;
using UnityEngine.Networking;
using System.Threading.Tasks;
using System;
using Newtonsoft.Json;
using WeatherApp.Data;
using WeatherApp.Config;

namespace WeatherApp.Services
{
    /// <summary>
    /// Modern API client for fetching weather data
    /// </summary>
    public class WeatherApiClient : MonoBehaviour
    {
        [Header("API Configuration")]
        [SerializeField] private string baseUrl = "http://api.openweathermap.org/data/2.5/weather";

        /// <summary>
        /// Fetch weather data for a specific city using async/await pattern
        /// </summary>
        /// <param name="city">City name to get weather for</param>
        /// <returns>Parsed WeatherData object if successful; null otherwise</returns>
        public async Task<WeatherData> GetWeatherDataAsync(string city)
        {
            if (string.IsNullOrWhiteSpace(city))
            {
                Debug.LogError("City name cannot be empty");
                return null;
            }

            if (!ApiConfig.IsApiKeyConfigured())
            {
                Debug.LogError("API key not configured. Please set up your config.json file in StreamingAssets folder.");
                return null;
            }

            string url = $"{baseUrl}?q={city}&appid={ApiConfig.OpenWeatherMapApiKey}&units=metric";

            using (UnityWebRequest request = UnityWebRequest.Get(url))
            {
                await request.SendWebRequest();

                // Handle different error types specifically
                switch (request.result)
                {
                    case UnityWebRequest.Result.Success:
                        return ParseWeatherData(request.downloadHandler.text);

                    case UnityWebRequest.Result.ConnectionError:
                        Debug.LogError($"Network connection failed: {request.error}");
                        break;

                    case UnityWebRequest.Result.ProtocolError:
                        Debug.LogError($"HTTP Error {request.responseCode}: {request.error}");
                        break;

                    case UnityWebRequest.Result.DataProcessingError:
                        Debug.LogError($"Data processing failed: {request.error}");
                        break;
                }

                return null;
            }
        }

        private WeatherData ParseWeatherData(string jsonString)
        {
            try
            {
                return JsonConvert.DeserializeObject<WeatherData>(jsonString);
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to parse weather data: {e.Message}");
                return null;
            }
        }

        /// <summary>
        /// Example usage method - students can use this as reference
        /// </summary>
        private async void Start()
        {
            var weatherData = await GetWeatherDataAsync("London");

            if (weatherData != null && weatherData.IsValid)
            {
                Debug.Log($"Weather in {weatherData.CityName}: {weatherData.TemperatureInCelsius:F1}°C");
                Debug.Log($"Description: {weatherData.PrimaryDescription}");
            }
            else
            {
                Debug.LogError("Failed to get weather data");
            }
        }
    }
}
