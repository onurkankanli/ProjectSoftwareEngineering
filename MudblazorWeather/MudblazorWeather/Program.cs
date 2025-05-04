using System;
using System.Net.Http;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MudBlazor.Services;
using MudblazorWeather.Backend.DataHandling;
using MudblazorWeather.Backend.Networking;

namespace MudblazorWeather
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            await HttpHandler.RequestData(TimeSpan.FromDays(1));

            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("#app");
            builder.Services.AddMudServices();

            builder.Services.AddScoped(
                sp => new HttpClient{BaseAddress = new Uri(builder.HostEnvironment.BaseAddress)});
            
            builder.Logging.SetMinimumLevel(LogLevel.Debug);
            
            await builder.Build().RunAsync();
        }
    }
}