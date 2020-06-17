using System;

namespace CEC.RoutingSample.Data
{
    public class WeatherForecast
    {
        public DateTime Date { get; set; } = DateTime.Now;

        public int TemperatureC { get; set; }

        public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);

        public string Summary { get; set; } = string.Empty;

        public WeatherForecast Copy()
        {
            return new WeatherForecast() {
                Date = this.Date,
                TemperatureC = this.TemperatureC,
                Summary = this.Summary
            };
        }
    }
}
