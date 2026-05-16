using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ConfigurationManager.Api;
using System;
using System.Threading.Tasks;

namespace ConfigurationManager.Examples
{
    /// <summary>
    /// Quick Start Examples - Copy and paste these to get started quickly
    /// </summary>
    public class QuickStartExamples
    {
        // ========== EXAMPLE 1: Basic Configuration Builder ==========
        public static void Example1_BasicConfigurationBuilder()
        {
            Console.WriteLine("=== Example 1: Basic Configuration Builder ===\n");

            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddConsul(
                    hostName: "localhost",
                    port: 8500,
                    serviceHostName: "my-service",
                    mainFolder: "apps/myapp"
                )
                .Build();

            var value = configuration["app-key"];
            Console.WriteLine($"Value from Consul: {value}");
        }

        // ========== EXAMPLE 2: Dependency Injection Setup ==========
        public static void Example2_DependencyInjection()
        {
            Console.WriteLine("\n=== Example 2: Dependency Injection Setup ===\n");

            var configuration = new ConfigurationBuilder()
                .AddConsul("localhost", 8500, mainFolder: "myapp")
                .Build();

            var services = new ServiceCollection();
            services.AddSingleton<IConfiguration>(configuration);
            services.AddScoped<IMyService, MyService>();

            var serviceProvider = services.BuildServiceProvider();
            var myService = serviceProvider.GetRequiredService<IMyService>();
            myService.PrintSettings();
        }

        // ========== EXAMPLE 3: Service with IConfiguration ==========
        public interface IMyService
        {
            void PrintSettings();
        }

        public class MyService : IMyService
        {
            private readonly IConfiguration _configuration;

            public MyService(IConfiguration configuration)
            {
                _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            }

            public void PrintSettings()
            {
                var setting1 = _configuration["Setting1"];
                var setting2 = _configuration["Setting2"];

                Console.WriteLine($"Setting1: {setting1}");
                Console.WriteLine($"Setting2: {setting2}");
            }
        }

        // ========== EXAMPLE 4: ASP.NET Core Program.cs (Minimal) ==========
        public static void Example4_AspNetCoreMinimalHosting()
        {
            Console.WriteLine("\n=== Example 4: ASP.NET Core Minimal Hosting ===\n");

            Console.WriteLine(@"
// In your Program.cs:

var builder = WebApplication.CreateBuilder(args);

// Add Consul configuration
builder.Configuration
    .AddJsonFile(""appsettings.json"", optional: false, reloadOnChange: true)
    .AddJsonFile($""appsettings.{builder.Environment.EnvironmentName}.json"", 
               optional: true, reloadOnChange: true)
    .AddConsul(
        hostName: builder.Configuration[""Consul:HostName""] ?? ""localhost"",
        port: int.Parse(builder.Configuration[""Consul:Port""] ?? ""8500""),
        serviceHostName: ""my-api"",
        mainFolder: ""apps/my-api""
    )
    .AddEnvironmentVariables();

// Add services
builder.Services.AddControllers();
builder.Services.AddScoped<IMyConfigService, MyConfigService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseRouting();
app.MapControllers();

app.Run();
");
        }

        // ========== EXAMPLE 5: Using IConfiguration in a Service ==========
        public interface IMyConfigService
        {
            string GetDatabaseConnection();
            Task<string> GetConsulValueAsync(string key);
        }

        public class MyConfigService : IMyConfigService
        {
            private readonly IConfiguration _configuration;

            public MyConfigService(IConfiguration configuration)
            {
                _configuration = configuration;
            }

            public string GetDatabaseConnection()
            {
                return _configuration["ConnectionStrings:DefaultConnection"] 
                    ?? "Server=localhost;Database=DefaultDb";
            }

            public async Task<string> GetConsulValueAsync(string key)
            {
                var hostName = _configuration["Consul:HostName"] ?? "localhost";
                var port = int.Parse(_configuration["Consul:Port"] ?? "8500");

                var manager = new Manager(hostName, port).AsReadOnly();
                return await manager.GetAsync(key);
            }
        }

        // ========== EXAMPLE 6: Optional Consul Configuration ==========
        public static void Example6_OptionalConsulConfiguration()
        {
            Console.WriteLine("\n=== Example 6: Optional Consul Configuration ===\n");

            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: true)
                .AddConsul(
                    hostName: "localhost",
                    port: 8500,
                    optional: true  // Won't throw if Consul is unavailable
                )
                .Build();

            Console.WriteLine("Configuration loaded (Consul may or may not be available)");
        }

        // ========== EXAMPLE 7: Multiple Environment Configuration ==========
        public static void Example7_MultipleEnvironments()
        {
            Console.WriteLine("\n=== Example 7: Multiple Environments ===\n");

            var environment = System.Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";

            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false)
                .AddJsonFile($"appsettings.{environment}.json", optional: true)
                .AddConsul(
                    hostName: "localhost",
                    port: 8500,
                    mainFolder: $"apps/myapp/{environment.ToLower()}",
                    optional: environment != "Production"
                )
                .Build();

            Console.WriteLine($"Configuration loaded for environment: {environment}");
        }

        // ========== EXAMPLE 8: Configuration with Custom Action ==========
        public static void Example8_ConfigurationWithCustomAction()
        {
            Console.WriteLine("\n=== Example 8: Configuration with Custom Action ===\n");

            var configuration = new ConfigurationBuilder()
                .AddConsul(options =>
                {
                    options.HostName = "consul.example.com";
                    options.Port = 8500;
                    options.ServiceHostName = "my-service";
                    options.MainFolder = "config/myapp";
                    options.Optional = true;
                })
                .Build();

            Console.WriteLine("Configuration created with custom options");
        }

        // ========== EXAMPLE 9: Accessing Nested Configuration ==========
        public static void Example9_NestedConfiguration()
        {
            Console.WriteLine("\n=== Example 9: Nested Configuration ===\n");

            var configuration = new ConfigurationBuilder()
                .AddConsul("localhost", 8500, mainFolder: "myapp")
                .Build();

            // Assuming Consul has values like:
            // Database/Host = localhost
            // Database/Port = 5432
            // Database/Name = mydb

            var dbSection = configuration.GetSection("Database");
            var host = dbSection["Host"];
            var port = dbSection["Port"];
            var name = dbSection["Name"];

            Console.WriteLine($"Database Host: {host}");
            Console.WriteLine($"Database Port: {port}");
            Console.WriteLine($"Database Name: {name}");
        }

        // ========== EXAMPLE 10: Reading and Writing Configuration ==========
        public static async Task Example10_ReadAndWriteConfiguration()
        {
            Console.WriteLine("\n=== Example 10: Read and Write Configuration ===\n");

            var manager = new Manager(
                hostName: "localhost",
                port: 8500,
                serviceHostName: "my-service",
                mainFolder: "apps/myapp"
            ).AsManager();

            // Write a configuration value
            var addedSuccess = await manager.AddAsync("my-key", "my-value");
            Console.WriteLine($"Added configuration: {addedSuccess}");

            // Read the configuration value back
            var configuration = new ConfigurationBuilder()
                .AddConsul("localhost", 8500, mainFolder: "apps/myapp")
                .Build();

            var value = configuration["my-key"];
            Console.WriteLine($"Retrieved configuration: {value}");
        }

        // ========== EXAMPLE 11: Strongly Typed Configuration ==========
        public static void Example11_StronglyTypedConfiguration()
        {
            Console.WriteLine("\n=== Example 11: Strongly Typed Configuration ===\n");

            var configuration = new ConfigurationBuilder()
                .AddConsul("localhost", 8500, mainFolder: "myapp")
                .Build();

            var services = new ServiceCollection();
            services.AddSingleton(configuration);

            // Bind to strongly typed class
            var consulOptions = new ConsulOptions();
            configuration.GetSection("Consul").Bind(consulOptions);

            Console.WriteLine($"Consul Host: {consulOptions.HostName}");
            Console.WriteLine($"Consul Port: {consulOptions.Port}");
        }

        public class ConsulOptions
        {
            public string HostName { get; set; }
            public int Port { get; set; }
            public string ServiceName { get; set; }
            public string MainFolder { get; set; }
        }

        // ========== EXAMPLE 12: Health Check with Consul ==========
        public static void Example12_HealthCheckWithConsul()
        {
            Console.WriteLine("\n=== Example 12: Health Check with Consul ===\n");

            try
            {
                var manager = new Manager("localhost", 8500);
                var isConnected = manager.IsConnected();

                if (isConnected)
                {
                    Console.WriteLine("✓ Successfully connected to Consul");
                }
                else
                {
                    Console.WriteLine("✗ Failed to connect to Consul");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ Consul health check failed: {ex.Message}");
            }
        }

        // ========== EXAMPLE 13: Service Factory Pattern ==========
        public static IConfigurationService CreateConfigurationService(
            IConfiguration configuration)
        {
            return new ConfigurationService(configuration);
        }

        public interface IConfigurationService
        {
            string GetApplicationName();
            string GetEnvironment();
            string GetDatabaseConnection();
        }

        public class ConfigurationService : IConfigurationService
        {
            private readonly IConfiguration _configuration;

            public ConfigurationService(IConfiguration configuration)
            {
                _configuration = configuration;
            }

            public string GetApplicationName()
            {
                return _configuration["Application:Name"] ?? "MyApp";
            }

            public string GetEnvironment()
            {
                return _configuration["ASPNETCORE_ENVIRONMENT"] ?? "Production";
            }

            public string GetDatabaseConnection()
            {
                return _configuration["ConnectionStrings:DefaultConnection"]
                    ?? throw new InvalidOperationException("Database connection not configured");
            }
        }

        // ========== Main - Run Examples ==========
        public static void Main(string[] args)
        {
            Console.WriteLine("ConfigurationManager.Api - Quick Start Examples\n");
            Console.WriteLine("=".PadRight(50, '=') + "\n");

            try
            {
                Example1_BasicConfigurationBuilder();
                Example2_DependencyInjection();
                Example4_AspNetCoreMinimalHosting();
                Example6_OptionalConsulConfiguration();
                Example7_MultipleEnvironments();
                Example8_ConfigurationWithCustomAction();
                Example9_NestedConfiguration();
                Example12_HealthCheckWithConsul();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\nNote: Some examples require Consul to be running.");
                Console.WriteLine($"Error: {ex.Message}");
            }

            Console.WriteLine("\n" + "=".PadRight(50, '='));
            Console.WriteLine("Examples completed!");
        }
    }
}
