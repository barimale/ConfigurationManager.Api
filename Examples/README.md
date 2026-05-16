# ConfigurationManager.Api Examples

This folder contains comprehensive examples showing how to integrate `ConfigurationManager.Api` with Microsoft's `IConfiguration` and dependency injection in ASP.NET Core applications.

## Files Overview

### Example Files

1. **DIConfigurationExample.cs**
   - Dependency Injection setup with ConfigurationBuilder
   - Registering IConfiguration in ServiceCollection
   - Creating services that depend on IConfiguration
   - Advanced DI patterns with Options pattern
   - Strongly typed configuration

2. **AspNetCoreWebApiExample.cs**
   - ASP.NET Core 3.1-5.x Startup.cs approach
   - ASP.NET Core 6.0+ minimal hosting with Program.cs
   - Configuring HostBuilder with Consul
   - Example services and service interfaces
   - Complete host builder configuration examples

3. **AspNetCoreWebApiControllerExample.cs**
   - API Controller examples showing DI usage
   - GET/POST endpoints for configuration management
   - Health check endpoint
   - App info endpoint
   - Request/Response models
   - Two example controllers (Configuration and Weather)

4. **QuickStartExamples.cs**
   - 13 quick start examples you can copy and paste
   - From basic setup to advanced patterns
   - Health checks, nested configuration, read/write operations
   - Strongly typed configuration examples
   - Service factory patterns

### Configuration Files

1. **appsettings.json**
   - Base configuration file with common settings
   - Database connections
   - Consul settings
   - API settings
   - Feature flags
   - Caching configuration

2. **appsettings.Development.json**
   - Development-specific settings
   - Local Consul connection
   - Debug logging
   - Development database

3. **appsettings.Staging.json**
   - Staging environment configuration
   - Staging Consul and database connections
   - Production-like settings but for testing

4. **appsettings.Production.json**
   - Production configuration
   - Production Consul and database connections
   - Minimal logging for performance
   - Connection pooling and optimizations

### Documentation

1. **GUIDE.md**
   - Comprehensive integration guide
   - Table of contents with all sections
   - Basic setup and DI patterns
   - ASP.NET Core integration (3.1+ and 6.0+)
   - Configuration file examples
   - Advanced scenarios
   - Best practices

## Quick Start

### 1. Basic Configuration with Consul

```csharp
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

### 2. Dependency Injection Setup

```csharp
var services = new ServiceCollection();
services.AddSingleton<IConfiguration>(configuration);
services.AddScoped<IMyService, MyService>();

var serviceProvider = services.BuildServiceProvider();
var myService = serviceProvider.GetRequiredService<IMyService>();
```

### 3. ASP.NET Core WebAPI (Program.cs)

```csharp
var builder = WebApplication.CreateBuilder(args);

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

builder.Services.AddControllers();
builder.Services.AddScoped<IConsulConfigService, ConsulConfigService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
	app.UseDeveloperExceptionPage();

app.UseRouting();
app.MapControllers();
app.Run();
```

### 4. Using IConfiguration in a Service

```csharp
public class MyService : IMyService
{
	private readonly IConfiguration _configuration;

	public MyService(IConfiguration configuration)
	{
		_configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
	}

	public void DoSomething()
	{
		var setting = _configuration["AppSettings:Setting1"];
		var dbConnection = _configuration["ConnectionStrings:DefaultConnection"];
	}
}
```

## Running the Examples

### Prerequisites

- .NET 6.0 or later (or .NET Standard 2.0 compatible environment)
- Consul running locally (or accessible)
- Visual Studio, VS Code, or command line

### Option 1: Run QuickStartExamples

```bash
dotnet run --project Examples/QuickStartExamples.cs
```

### Option 2: Create a Console Application

1. Create a new console project: `dotnet new console -n ConfigManagerExample`
2. Add reference to ConfigurationManager.Api
3. Copy code from one of the example files
4. Modify the Consul host/port as needed
5. Run: `dotnet run`

### Option 3: Create an ASP.NET Core WebAPI

1. Create a new web API: `dotnet new webapi -n MyConfigApiExample`
2. Add NuGet package: `dotnet add package ConfigurationManager.Api`
3. Copy the Program.cs code from `AspNetCoreWebApiExample.cs`
4. Copy the controller from `AspNetCoreWebApiControllerExample.cs`
5. Copy the appsettings.json files
6. Run: `dotnet run`
7. Access the API at `https://localhost:7001/swagger` (if Swagger is enabled)

## Common Scenarios

### Scenario 1: Using IConfiguration in Multiple Services

See: `DIConfigurationExample.cs` - Multiple service registration example

### Scenario 2: Type-Safe Configuration with Options Pattern

See: `DIConfigurationExample.cs` - `AdvancedDIExample` class

### Scenario 3: Health Check with Consul

See: `AspNetCoreWebApiControllerExample.cs` - `GetHealth` endpoint

### Scenario 4: Managing Configuration via API

See: `AspNetCoreWebApiControllerExample.cs` - Configuration controller

### Scenario 5: Environment-Specific Configuration

See: All `appsettings.{Environment}.json` files

### Scenario 6: Optional Consul Configuration

See: `DIConfigurationExample.cs` - `ConfigureWithOptionalConsul` method

## API Endpoints (WebAPI Example)

When running the ASP.NET Core example, these endpoints are available:

### Configuration Management
- `GET /api/configuration/{key}` - Get a single configuration value
- `GET /api/configuration/all` - Get all configuration values
- `POST /api/configuration` - Set a configuration value
- `GET /api/configuration/app-info` - Get application information
- `GET /api/configuration/health` - Get health status

### Weather Service (Example)
- `GET /api/weather/{city}` - Get weather for a city
- `GET /api/weather/config/info` - Get weather service configuration

## Best Practices

1. **Always validate null configuration:**
   ```csharp
   _configuration ?? throw new ArgumentNullException(nameof(_configuration))
   ```

2. **Use environment-specific configuration files:**
   ```
   appsettings.json
   appsettings.{Environment}.json
   ```

3. **Use the Options pattern for complex configuration:**
   ```csharp
   services.Configure<MyOptions>(configuration.GetSection("MySection"));
   ```

4. **Mark Consul as optional in non-production environments:**
   ```csharp
   optional: !context.HostingEnvironment.IsProduction()
   ```

5. **Handle missing configuration gracefully:**
   ```csharp
   var value = _configuration["Key"] ?? "default-value";
   ```

## Troubleshooting

### Consul Connection Failed

- Ensure Consul is running: `consul agent -dev`
- Check host and port in configuration
- Verify network connectivity

### Configuration Not Loading

- Check that appsettings.json exists and is valid JSON
- Verify JSON file is set to "Copy if newer" in Visual Studio
- Check Consul folder path matches the configuration

### Missing IConfiguration

- Ensure `AddSingleton<IConfiguration>()` is called in services
- For ASP.NET Core, the framework adds this automatically

## Additional Resources

- [Official Guide](GUIDE.md) - Comprehensive integration guide
- [Quick Start Examples](QuickStartExamples.cs) - 13 copy-paste examples
- [ASP.NET Core Configuration Docs](https://docs.microsoft.com/aspnet/core/fundamentals/configuration)
- [Consul Documentation](https://www.consul.io/docs)
- [Microsoft.Extensions.Configuration](https://docs.microsoft.com/dotnet/api/microsoft.extensions.configuration)

## Contributing

To add more examples:

1. Create a new file in the Examples folder
2. Follow the same documentation style
3. Include clear comments explaining the code
4. Add a reference in this README

## License

These examples are provided as part of ConfigurationManager.Api and follow the same license.
