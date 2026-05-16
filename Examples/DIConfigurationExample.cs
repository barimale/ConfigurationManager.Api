using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ConfigurationManager.Api;

namespace ConfigurationManager.Examples
{
    /// <summary>
    /// Example: Configuring Consul with Dependency Injection
    /// 
    /// This example demonstrates how to:
    /// 1. Build IConfiguration with Consul provider
    /// 2. Register IConfiguration in the DI container
    /// 3. Inject IConfiguration into services
    /// </summary>
    public class DIConfigurationExample
    {
        public static void ConfigureDependencyInjection()
        {
            // Example 1: Basic setup with ConfigurationBuilder and ServiceCollection
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddConsul(
                    hostName: "localhost",
                    port: 8500,
                    serviceHostName: "my-service",
                    mainFolder: "apps/myapp"
                )
                .Build();

            var services = new ServiceCollection();

            // Register IConfiguration in DI container
            services.AddSingleton<IConfiguration>(configuration);

            // Example 2: Using AddSingleton to make IConfigurationRoot available
            services.AddSingleton(configuration as IConfigurationRoot);

            // Register a service that depends on IConfiguration
            services.AddSingleton<IMyConfiguredService, MyConfiguredService>();

            var serviceProvider = services.BuildServiceProvider();

            // Retrieve the service from DI container
            var myService = serviceProvider.GetRequiredService<IMyConfiguredService>();
            myService.PrintConfiguration();
        }

        public static void ConfigureWithOptionalConsul()
        {
            // If Consul connection might fail, mark it as optional
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddConsul(
                    hostName: "localhost",
                    port: 8500,
                    optional: true  // Won't throw if Consul is unavailable
                )
                .Build();

            var services = new ServiceCollection();
            services.AddSingleton<IConfiguration>(configuration);
            services.AddSingleton<IMyConfiguredService, MyConfiguredService>();

            var serviceProvider = services.BuildServiceProvider();
            var myService = serviceProvider.GetRequiredService<IMyConfiguredService>();
        }

        public static void ConfigureWithCustomAction()
        {
            // Using the custom action overload for more control
            var configuration = new ConfigurationBuilder()
                .AddConsul(options =>
                {
                    options.HostName = "consul.example.com";
                    options.Port = 8500;
                    options.ServiceHostName = "production-service";
                    options.MainFolder = "config/production";
                    options.Optional = true;
                })
                .Build();

            var services = new ServiceCollection();
            services.AddSingleton<IConfiguration>(configuration);
        }
    }

    /// <summary>
    /// Example service that depends on IConfiguration
    /// </summary>
    public interface IMyConfiguredService
    {
        void PrintConfiguration();
        string GetSetting(string key);
    }

    public class MyConfiguredService : IMyConfiguredService
    {
        private readonly IConfiguration _configuration;

        // IConfiguration is injected via DI
        public MyConfiguredService(IConfiguration configuration)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        public void PrintConfiguration()
        {
            Console.WriteLine("Configuration from Consul:");

            // Access settings from Consul configuration
            var setting1 = _configuration["AppSettings:Setting1"];
            var setting2 = _configuration["Database:ConnectionString"];

            Console.WriteLine($"Setting1: {setting1}");
            Console.WriteLine($"Database: {setting2}");

            // Access nested configuration
            var dbSection = _configuration.GetSection("Database");
            var host = dbSection["Host"];
            var port = dbSection["Port"];

            Console.WriteLine($"DB Host: {host}, Port: {port}");
        }

        public string GetSetting(string key)
        {
            return _configuration[key] ?? throw new KeyNotFoundException($"Setting '{key}' not found");
        }
    }

    /// <summary>
    /// Example: Advanced DI patterns
    /// </summary>
    public class AdvancedDIExample
    {
        public static void SetupWithOptions()
        {
            // Setup with strongly-typed options pattern
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: true)
                .AddConsul("localhost", 8500, mainFolder: "myapp")
                .Build();

            var services = new ServiceCollection();
            services.AddSingleton<IConfiguration>(configuration);

            // Use Options pattern for type-safe configuration
            services.Configure<ConsulOptions>(configuration.GetSection("Consul"));
            services.Configure<ApplicationOptions>(configuration.GetSection("Application"));

            // Register services that use the options
            services.AddSingleton<IMyConfiguredService, MyConfiguredService>();
            services.AddSingleton<IConsulManager, ConsulManagerService>();
        }
    }

    public class ConsulOptions
    {
        public string HostName { get; set; }
        public int Port { get; set; }
        public string ServiceHostName { get; set; }
        public string MainFolder { get; set; }
    }

    public class ApplicationOptions
    {
        public string AppName { get; set; }
        public string Environment { get; set; }
    }

    public interface IConsulManager
    {
        Task<string> GetValueAsync(string key);
        Task SetValueAsync(string key, string value);
    }

    public class ConsulManagerService : IConsulManager
    {
        private readonly IConfiguration _configuration;
        private Manager _manager;

        public ConsulManagerService(IConfiguration configuration)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            InitializeManager();
        }

        private void InitializeManager()
        {
            var hostName = _configuration["Consul:HostName"] ?? "localhost";
            var port = int.Parse(_configuration["Consul:Port"] ?? "8500");
            var serviceHostName = _configuration["Consul:ServiceHostName"] ?? "";
            var mainFolder = _configuration["Consul:MainFolder"] ?? "";

            _manager = new Manager(hostName, port, serviceHostName, mainFolder);
        }

        public async Task<string> GetValueAsync(string key)
        {
            return await _manager.GetAsync(key);
        }

        public async Task SetValueAsync(string key, string value)
        {
            await _manager.AddAsync(key, value);
        }
    }
}
