# Consul Configuration Integration Guide

## Overview

This guide demonstrates how to integrate the `ConfigurationManager.Api` library with Microsoft's `IConfiguration` and `IConfigurationBuilder` for dependency injection in ASP.NET Core applications.

## Table of Contents

1. [Basic Setup](#basic-setup)
2. [Dependency Injection Setup](#dependency-injection-setup)
3. [ASP.NET Core WebAPI Integration](#aspnet-core-webapi-integration)
4. [Configuration Files](#configuration-files)
5. [Advanced Scenarios](#advanced-scenarios)
6. [Best Practices](#best-practices)

---

## Basic Setup

### Installation

First, ensure you have the required NuGet packages:

```bash
dotnet add package ConfigurationManager.Api
dotnet add package Microsoft.Extensions.Configuration
```

### Simple Configuration Builder

```csharp
using Microsoft.Extensions.Configuration;
using ConfigurationManager.Api;

var configuration = new ConfigurationBuilder()
	.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
	.AddConsul(
		hostName: "localhost",
		port: 8500,
		serviceHostName: "my-service",
		mainFolder: "apps/myapp"
	)
	.Build();

var value = configuration["some-key"];
```

---

## Dependency Injection Setup

### Registering IConfiguration in ServiceCollection

```csharp
var configuration = new ConfigurationBuilder()
	.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
	.AddConsul("localhost", 8500, mainFolder: "myapp")
	.Build();

var services = new ServiceCollection();

// Register IConfiguration in DI container
services.AddSingleton<IConfiguration>(configuration);

// Register services that depend on IConfiguration
services.AddScoped<IMyService, MyService>();

var serviceProvider = services.BuildServiceProvider();
var myService = serviceProvider.GetRequiredService<IMyService>();
```

### Service Implementation

```csharp
public class MyService : IMyService
{
	private readonly IConfiguration _configuration;

	// IConfiguration is injected via constructor
	public MyService(IConfiguration configuration)
	{
		_configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
	}

	public void DoSomething()
	{
		var setting = _configuration["AppSettings:Setting1"];
		var dbConnection = _configuration["ConnectionStrings:DefaultConnection"];

		Console.WriteLine($"Setting: {setting}");
		Console.WriteLine($"DB: {dbConnection}");
	}
}
```

### Optional Consul Configuration

```csharp
var configuration = new ConfigurationBuilder()
	.AddJsonFile("appsettings.json", optional: true)
	.AddConsul(
		hostName: "localhost",
		port: 8500,
		serviceHostName: "my-service",
		mainFolder: "myapp",
		optional: true  // Won't throw if Consul is unavailable
	)
	.Build();
```

### Custom Action Configuration

```csharp
var configuration = new ConfigurationBuilder()
	.AddConsul(options =>
	{
		options.HostName = "consul.example.com";
		options.Port = 8500;
		options.ServiceHostName = "production-service";
		options.MainFolder = "config/production";
		options.Optional = true;
		options.ReloadDelay = 5000; // Reload every 5 seconds
	})
	.Build();
```

---

## ASP.NET Core WebAPI Integration

### ASP.NET Core 3.1 - 5.x (Startup.cs)

```csharp
public class Startup
{
	public IConfiguration Configuration { get; }

	public Startup(IConfiguration configuration)
	{
		Configuration = configuration;
	}

	public void ConfigureServices(IServiceCollection services)
	{
		services.AddControllers();
		services.AddScoped<IWeatherService, WeatherService>();
		services.AddScoped<IConsulConfigService, ConsulConfigService>();
	}

	public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
	{
		if (env.IsDevelopment())
		{
			app.UseDeveloperExceptionPage();
		}

		app.UseHttpsRedirection();
		app.UseRouting();

		app.UseEndpoints(endpoints =>
		{
			endpoints.MapControllers();
		});
	}

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
```

### ASP.NET Core 6.0+ (Program.cs - Minimal Hosting)

```csharp
var builder = WebApplication.CreateBuilder(args);

// Configure application settings
builder.Configuration
	.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
	.AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", 
			   optional: true, reloadOnChange: true)
	.AddConsul(
		hostName: builder.Configuration["Consul:HostName"] ?? "localhost",
		port: int.Parse(builder.Configuration["Consul:Port"] ?? "8500"),
		serviceHostName: "my-api-service",
		mainFolder: "apps/my-api"
	)
	.AddEnvironmentVariables();

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddScoped<IWeatherService, WeatherService>();
builder.Services.AddScoped<IConsulConfigService, ConsulConfigService>();

builder.Services.AddCors(options =>
{
	options.AddPolicy("AllowAll", policy =>
	{
		policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
	});
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
	app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseCors("AllowAll");

app.MapControllers();

app.Run();
```

### Using IConfiguration in Controllers

```csharp
[ApiController]
[Route("api/[controller]")]
public class ConfigurationController : ControllerBase
{
	private readonly IConfiguration _configuration;
	private readonly IConsulConfigService _consulConfigService;

	public ConfigurationController(
		IConfiguration configuration,
		IConsulConfigService consulConfigService)
	{
		_configuration = configuration;
		_consulConfigService = consulConfigService;
	}

	[HttpGet("{key}")]
	public async Task<ActionResult<string>> GetConfigValue(string key)
	{
		try
		{
			var value = await _consulConfigService.GetConfigAsync(key);
			return Ok(new { key, value });
		}
		catch
		{
			return NotFound();
		}
	}

	[HttpGet("app-info")]
	public ActionResult<object> GetAppInfo()
	{
		return Ok(new
		{
			ApplicationName = _configuration["Application:Name"],
			Environment = _configuration["ASPNETCORE_ENVIRONMENT"],
			Version = _configuration["Application:Version"]
		});
	}
}
```

---

## Configuration Files

### Example appsettings.json

```json
{
  "Logging": {
	"LogLevel": {
	  "Default": "Information",
	  "Microsoft": "Warning"
	}
  },
  "AllowedHosts": "*",
  "Consul": {
	"HostName": "localhost",
	"Port": 8500,
	"ServiceName": "my-api-service",
	"MainFolder": "apps/my-api"
  },
  "Application": {
	"Name": "My API Service",
	"Version": "1.0.0"
  },
  "WeatherService": {
	"Url": "https://api.weatherapi.com/v1",
	"ApiKey": "your-key-here"
  },
  "ConnectionStrings": {
	"DefaultConnection": "Server=localhost;Database=MyDb;User Id=sa;Password=YourPassword;"
  }
}
```

### Example appsettings.Development.json

```json
{
  "Logging": {
	"LogLevel": {
	  "Default": "Debug",
	  "Microsoft": "Debug"
	}
  },
  "Consul": {
	"HostName": "localhost",
	"Port": 8500,
	"ServiceName": "my-api-service-dev",
	"MainFolder": "apps/my-api/dev"
  }
}
```

### Example appsettings.Production.json

```json
{
  "Logging": {
	"LogLevel": {
	  "Default": "Warning"
	}
  },
  "Consul": {
	"HostName": "consul.prod.company.com",
	"Port": 8500,
	"ServiceName": "my-api-service-prod",
	"MainFolder": "apps/my-api/prod"
  }
}
```

---

## Advanced Scenarios

### 1. Options Pattern with Type-Safe Configuration

```csharp
// In Program.cs or Startup.ConfigureServices
var configuration = new ConfigurationBuilder()
	.AddJsonFile("appsettings.json")
	.AddConsul("localhost", 8500, mainFolder: "myapp")
	.Build();

services.Configure<ConsulOptions>(configuration.GetSection("Consul"));
services.Configure<ApplicationOptions>(configuration.GetSection("Application"));

// Usage in a service
public class MyService
{
	private readonly IOptions<ConsulOptions> _consulOptions;

	public MyService(IOptions<ConsulOptions> consulOptions)
	{
		_consulOptions = consulOptions;
	}

	public void DoSomething()
	{
		var hostName = _consulOptions.Value.HostName;
		var port = _consulOptions.Value.Port;
	}
}

// Define option classes
public class ConsulOptions
{
	public string HostName { get; set; }
	public int Port { get; set; }
	public string ServiceName { get; set; }
	public string MainFolder { get; set; }
}

public class ApplicationOptions
{
	public string Name { get; set; }
	public string Version { get; set; }
}
```

### 2. Dynamic Consul Configuration Service

```csharp
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
		_configuration = configuration;
		InitializeManager();
	}

	private void InitializeManager()
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
```

### 3. Health Check with Consul Connection

```csharp
[HttpGet("health")]
public ActionResult<HealthResponse> GetHealth()
{
	try
	{
		var manager = new Manager(
			_configuration["Consul:HostName"] ?? "localhost",
			int.Parse(_configuration["Consul:Port"] ?? "8500")
		);

		var isConnected = manager.IsConnected();

		return Ok(new HealthResponse
		{
			Status = isConnected ? "Healthy" : "Degraded",
			ConsulConnected = isConnected,
			Timestamp = DateTime.UtcNow
		});
	}
	catch (Exception ex)
	{
		return StatusCode(503, new HealthResponse
		{
			Status = "Unhealthy",
			ConsulConnected = false,
			Timestamp = DateTime.UtcNow,
			Error = ex.Message
		});
	}
}
```

### 4. Configuration Hierarchies

```json
{
  "Database": {
	"Host": "localhost",
	"Port": 5432,
	"Name": "mydb",
	"Credentials": {
	  "UserId": "admin",
	  "Password": "secret"
	}
  }
}
```

```csharp
// In Consul, store as:
// Database/Host = localhost
// Database/Port = 5432
// Database/Name = mydb
// Database/Credentials/UserId = admin
// Database/Credentials/Password = secret

// Access in code
var dbSection = _configuration.GetSection("Database");
var host = dbSection["Host"];
var port = dbSection["Port"];
var userId = dbSection["Credentials:UserId"];
```

---

## Best Practices

### 1. Always Check for Null Configuration

```csharp
public MyService(IConfiguration configuration)
{
	_configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
}
```

### 2. Use IOptions Pattern for Complex Configuration

```csharp
// Better than injecting IConfiguration directly
services.Configure<MyOptions>(configuration.GetSection("MySection"));

public MyService(IOptions<MyOptions> options)
{
	_options = options.Value;
}
```

### 3. Handle Missing Configuration Gracefully

```csharp
var setting = _configuration["Setting:Name"] ?? "default-value";
var port = int.Parse(_configuration["Port"] ?? "8500");
```

### 4. Use Environment-Specific Configuration Files

```
appsettings.json
appsettings.Development.json
appsettings.Staging.json
appsettings.Production.json
```

### 5. Mark Consul as Optional in Non-Production Environments

```csharp
.AddConsul(
	hostName: "localhost",
	port: 8500,
	optional: !context.HostingEnvironment.IsProduction()
)
```

### 6. Implement Logging for Configuration Loading

```csharp
.ConfigureAppConfiguration((context, config) =>
{
	var logger = LoggerFactory.Create(x => x.AddConsole())
		.CreateLogger("ConfigurationSetup");

	logger.LogInformation("Loading Consul configuration from {Host}:{Port}", 
		consulHost, consulPort);

	config.AddConsul(consulHost, consulPort, mainFolder: consulFolder);
})
```

### 7. Cache Configuration in Services

```csharp
public class CachedConfigService
{
	private readonly IConfiguration _configuration;
	private readonly Dictionary<string, string> _cache = new();

	public string GetSetting(string key)
	{
		if (_cache.ContainsKey(key))
			return _cache[key];

		var value = _configuration[key];
		if (value != null)
			_cache[key] = value;

		return value;
	}
}
```

---

## Summary

The `ConfigurationManager.Api` library provides seamless integration with ASP.NET Core's `IConfiguration` system, enabling you to:

- ✅ Load configuration from Consul using standard .NET Core patterns
- ✅ Inject configuration into services via dependency injection
- ✅ Use environment-specific configuration files
- ✅ Implement type-safe configuration with the Options pattern
- ✅ Build flexible, cloud-native applications

For more examples, see the `Examples` folder in this repository.
