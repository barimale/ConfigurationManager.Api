using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ConfigurationManager.Api;

namespace ConfigurationManager.Examples.AspNetCore
{
    /// <summary>
    /// Example: ASP.NET Core WebAPI with Consul Configuration
    /// 
    /// This example demonstrates how to:
    /// 1. Configure IHostBuilder with Consul configuration provider
    /// 2. Use IConfiguration in Startup or Program.cs
    /// 3. Inject IConfiguration into controllers and services
    /// 4. Reload configuration from Consul
    /// </summary>
    public class AspNetCoreWebApiExample
    {
        // ========== LEGACY STARTUP APPROACH (ASP.NET Core 3.1 - 5.x) ==========

        public class Startup
        {
            public IConfiguration Configuration { get; }

            public Startup(IConfiguration configuration)
            {
                Configuration = configuration;
            }

            // This method gets called by the runtime. Use this method to add services to the container.
            public void ConfigureServices(IServiceCollection services)
            {
                // Register IConfiguration in DI
                services.AddSingleton(Configuration);

                // Add controllers
                services.AddControllers();

                // Register application services
                services.AddScoped<IWeatherService, WeatherService>();
                services.AddScoped<IConsulConfigService, ConsulConfigService>();

                // Add API versioning, CORS, etc.
                services.AddCors(options =>
                {
                    options.AddPolicy("AllowAll", builder =>
                    {
                        builder.AllowAnyOrigin()
                               .AllowAnyMethod()
                               .AllowAnyHeader();
                    });
                });
            }

            // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
            public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
            {
                if (env.IsDevelopment())
                {
                    app.UseDeveloperExceptionPage();
                }

                app.UseHttpsRedirection();
                app.UseRouting();
                app.UseCors("AllowAll");

                app.UseEndpoints(endpoints =>
                {
                    endpoints.MapControllers();
                });
            }

            // Static method to create the host with Consul configuration
            public static IHostBuilder CreateHostBuilder(string[] args) =>
                Host.CreateDefaultBuilder(args)
                    .ConfigureAppConfiguration((context, config) =>
                    {
                        config
                            .SetBasePath(context.HostingEnvironment.ContentRootPath)
                            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                            .AddJsonFile($"appsettings.{context.HostingEnvironment.EnvironmentName}.json", 
                                       optional: true, reloadOnChange: true)
                            .AddConsul(
                                hostName: "localhost",
                                port: 8500,
                                serviceHostName: "my-api-service",
                                mainFolder: "apps/my-api"
                            )
                            .AddEnvironmentVariables();
                    })
                    .ConfigureWebHostDefaults(webBuilder =>
                    {
                        webBuilder.UseStartup<Startup>();
                    });
        }

        // ========== MODERN MINIMAL HOSTING APPROACH (ASP.NET Core 6.0+) ==========

        /// <summary>
        /// Example Program.cs for ASP.NET Core 6.0+ with minimal hosting model
        /// </summary>
        public class ModernProgramExample
        {
            public static void ConfigureProgramExample()
            {
                // This is how your Program.cs should look:
                // var builder = WebApplication.CreateBuilder(args);
                // 
                // // Configure application settings
                // builder.Configuration
                //     .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                //     .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", 
                //                optional: true, reloadOnChange: true)
                //     .AddConsul(
                //         hostName: builder.Configuration["Consul:HostName"] ?? "localhost",
                //         port: int.Parse(builder.Configuration["Consul:Port"] ?? "8500"),
                //         serviceHostName: "my-api-service",
                //         mainFolder: "apps/my-api"
                //     )
                //     .AddEnvironmentVariables();
                //
                // // Add services to the container
                // builder.Services.AddControllers();
                // builder.Services.AddScoped<IWeatherService, WeatherService>();
                // builder.Services.AddScoped<IConsulConfigService, ConsulConfigService>();
                //
                // builder.Services.AddCors(options =>
                // {
                //     options.AddPolicy("AllowAll", policy =>
                //     {
                //         policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
                //     });
                // });
                //
                // var app = builder.Build();
                //
                // if (app.Environment.IsDevelopment())
                // {
                //     app.UseDeveloperExceptionPage();
                // }
                //
                // app.UseHttpsRedirection();
                // app.UseRouting();
                // app.UseCors("AllowAll");
                //
                // app.MapControllers();
                //
                // app.Run();
            }
        }

        /// <summary>
        /// Example: Using Consul configuration in ConfigureWebHostDefaults
        /// </summary>
        public class AdvancedHostBuilderExample
        {
            public static IHostBuilder CreateAdvancedHost(string[] args) =>
                Host.CreateDefaultBuilder(args)
                    .ConfigureAppConfiguration((context, config) =>
                    {
                        var baseConfig = config.Build();

                        // Read Consul settings from appsettings.json
                        var consulSection = baseConfig.GetSection("Consul");
                        var consulHostName = consulSection["HostName"] ?? "localhost";
                        var consulPort = int.Parse(consulSection["Port"] ?? "8500");
                        var consulServiceName = consulSection["ServiceName"] ?? "my-api";
                        var consulMainFolder = consulSection["MainFolder"] ?? "apps/my-api";

                        config
                            .SetBasePath(context.HostingEnvironment.ContentRootPath)
                            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                            .AddJsonFile($"appsettings.{context.HostingEnvironment.EnvironmentName}.json",
                                       optional: true, reloadOnChange: true)
                            .AddConsul(
                                hostName: consulHostName,
                                port: consulPort,
                                serviceHostName: consulServiceName,
                                mainFolder: consulMainFolder,
                                optional: !context.HostingEnvironment.IsProduction() // Only required in production
                            )
                            .AddEnvironmentVariables();
                    })
                    .ConfigureServices((context, services) =>
                    {
                        services.AddSingleton(context.Configuration);
                        services.AddControllers();
                        services.AddScoped<IWeatherService, WeatherService>();
                        services.AddScoped<IConsulConfigService, ConsulConfigService>();
                    })
                    .ConfigureWebHostDefaults(webBuilder =>
                    {
                        webBuilder.Configure((app) =>
                        {
                            if (app.Environment.IsDevelopment())
                            {
                                app.UseDeveloperExceptionPage();
                            }

                            app.UseHttpsRedirection();
                            app.UseRouting();
                            app.UseEndpoints(endpoints =>
                            {
                                endpoints.MapControllers();
                            });
                        });
                    });
        }
    }

    // ========== EXAMPLE SERVICES ==========

    /// <summary>
    /// Example: Service that consumes IConfiguration
    /// </summary>
    public interface IWeatherService
    {
        Task<string> GetWeatherAsync(string city);
    }

    public class WeatherService : IWeatherService
    {
        private readonly IConfiguration _configuration;

        public WeatherService(IConfiguration configuration)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        public async Task<string> GetWeatherAsync(string city)
        {
            // Get API key from Consul configuration
            var apiKey = _configuration["WeatherService:ApiKey"];
            var apiUrl = _configuration["WeatherService:Url"];

            // Use the configuration in your service logic
            Console.WriteLine($"Using WeatherAPI Url: {apiUrl}");

            // Simulate API call
            await Task.Delay(100);
            return $"Weather for {city} obtained using configuration from Consul";
        }
    }

    /// <summary>
    /// Example: Service that manages Consul configuration
    /// </summary>
    public interface IConsulConfigService
    {
        Task<string> GetConfigAsync(string key);
        Task SetConfigAsync(string key, string value);
        Task<Dictionary<string, string>> GetAllConfigAsync();
    }

    public class ConsulConfigService : IConsulConfigService
    {
        private readonly IConfiguration _configuration;
        private Manager _consulManager;

        public ConsulConfigService(IConfiguration configuration)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            InitializeConsulManager();
        }

        private void InitializeConsulManager()
        {
            var hostName = _configuration["Consul:HostName"] ?? "localhost";
            var port = int.Parse(_configuration["Consul:Port"] ?? "8500");
            var serviceHostName = _configuration["Consul:ServiceName"] ?? "";
            var mainFolder = _configuration["Consul:MainFolder"] ?? "";

            _consulManager = new Manager(hostName, port, serviceHostName, mainFolder).AsManager();
        }

        public async Task<string> GetConfigAsync(string key)
        {
            return await _consulManager.GetAsync(key);
        }

        public async Task SetConfigAsync(string key, string value)
        {
            await _consulManager.AddAsync(key, value);
        }

        public async Task<Dictionary<string, string>> GetAllConfigAsync()
        {
            return await _consulManager.AllKeyValuePairsAsync();
        }
    }

    // ========== EXAMPLE APPSETTINGS.JSON ==========

    /// <summary>
    /// Example appsettings.json configuration file
    /// 
    /// {
    ///   "Logging": {
    ///     "LogLevel": {
    ///       "Default": "Information",
    ///       "Microsoft": "Warning"
    ///     }
    ///   },
    ///   "AllowedHosts": "*",
    ///   "Consul": {
    ///     "HostName": "localhost",
    ///     "Port": 8500,
    ///     "ServiceName": "my-api-service",
    ///     "MainFolder": "apps/my-api"
    ///   },
    ///   "WeatherService": {
    ///     "Url": "https://api.weatherapi.com/v1",
    ///     "ApiKey": "your-key-here"
    ///   },
    ///   "Database": {
    ///     "ConnectionString": "Server=localhost;Database=MyDb;User Id=sa;Password=YourPassword;"
    ///   }
    /// }
    /// </summary>
    public class ExampleAppsettings
    {
    }
}
