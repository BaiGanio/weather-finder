using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Nancy.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using WeatherFinder.DB;
using WeatherFinder.Models;

namespace WeatherFinder.Services
{
    public interface IWeatherServiceManager
    {
        Task<IEnumerable<WeatherForecast>> GetAllForecastsAsync();
        Task<WeatherForecast> GetIndustryForecast(string city);
    }
    public class WeatherServiceManager : IWeatherServiceManager
    {
        private readonly string apikey = "";

        private readonly WeatherFinderDbContext _ctx;
        public WeatherServiceManager(IConfiguration config)
        {
            _ctx = new WeatherFinderDbContext();
            apikey = config["OpenWeatherMap:ApiKey"];
        }
        public async Task<IEnumerable<WeatherForecast>> GetAllForecastsAsync()
        {
            return await _ctx.Forecasts.ToListAsync();
        }

        public async Task<WeatherForecast> GetIndustryForecast(string city)
        {
            try
            {
                var url = $"http://api.openweathermap.org/data/2.5/weather?q={city}&units=metric&APPID={apikey}";

                var client = new WebClient();
                var content = client.DownloadString(url);
                var serializer = new JavaScriptSerializer();
                var jsonContent = serializer.Deserialize<dynamic>(content);

                var city1 = jsonContent["name"];
                var temp = jsonContent["main"]["temp"];
                var pressure = jsonContent["main"]["pressure"];
                var humidity = jsonContent["main"]["humidity"];
                var wind = jsonContent["wind"]["speed"];

                var forecast =
                    new WeatherForecast()
                    {
                        Id = Guid.NewGuid().ToString(),
                        TemperatureC = (int)temp,
                        Date = DateTime.Now,
                        Pressure = pressure,
                        Humidity = (int)humidity,
                        City = city,
                        Summary = "N/A",
                        Wind = (int)wind
                    };


                await LogForecast(forecast);

                return forecast;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }



        private async Task LogForecast(WeatherForecast weatherForecast)
        {
            await _ctx.Forecasts.AddAsync(weatherForecast);
            await _ctx.SaveChangesAsync();
        }
    }
}
