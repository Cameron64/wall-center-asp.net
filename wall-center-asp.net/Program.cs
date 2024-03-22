using Newtonsoft.Json;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddHttpClient();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}


// First, use default files middleware to serve the index.html as a default file.
app.UseDefaultFiles();
app.Use(async (context, next) =>
{
    if (context.Request.Path.Equals("/index.html", StringComparison.OrdinalIgnoreCase))
    {
        var webRootPath = app.Environment.WebRootPath;
        var filePath = Path.Combine(webRootPath, "index.html");

        if (File.Exists(filePath))
        {
            var content = await File.ReadAllTextAsync(filePath);

            WeatherForecast weatherForecast = new WeatherForecast();
            var response = await weatherForecast.GetWeatherHtmlAsync(app.Services.GetRequiredService<IHttpClientFactory>());
            dynamic weatherData = JsonConvert.DeserializeObject<dynamic>(response);
            decimal temp = weatherData.properties.temperature.value;
            temp = temp * 9 / 5 + 32; // Convert to Fahrenheit
            temp = Math.Round(temp, 0);
            // Dynamically modify the content
            var dynamicContent = content.Replace("<!--DynamicContent-->", $"{temp}");

            context.Response.ContentType = "text/html";
            await context.Response.WriteAsync(dynamicContent);
            return;
        }
    }

    await next();
});
// Then, use static files middleware to serve static files from wwwroot.
app.UseStaticFiles();

app.Run();



public class WeatherForecast
{

    public async Task<string> GetWeatherHtmlAsync(IHttpClientFactory clientFactory)
    {
        var httpClient = clientFactory.CreateClient();
        httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/123.0.0.0 Safari/537.36");
        httpClient.DefaultRequestHeaders.Accept.ParseAdd("application/json");
        var latLong = "30.3628,-97.6715"; // Example latitude and longitude
        //var url = $"https://api.weather.gov/points/{latLong}";
        var url = $"https://api.weather.gov/stations/KATT/observations/latest?require_qc=true";


        var forecastResponse = await httpClient.GetAsync(url);
        forecastResponse.EnsureSuccessStatusCode();

        var forecastContent = await forecastResponse.Content.ReadAsStringAsync();
        




        return forecastContent;
    }

}


