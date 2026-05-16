using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using ConfigurationManager.Api;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ConfigurationManager.Examples.AspNetCore.Controllers
{
    /// <summary>
    /// Example API Controller showing how to use IConfiguration and Consul services
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class ConfigurationController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly IConsulConfigService _consulConfigService;

        // IConfiguration and services are injected via DI
        public ConfigurationController(
            IConfiguration configuration,
            IConsulConfigService consulConfigService)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _consulConfigService = consulConfigService ?? throw new ArgumentNullException(nameof(consulConfigService));
        }

        /// <summary>
        /// Get a configuration value from Consul by key
        /// </summary>
        [HttpGet("{key}")]
        public async Task<ActionResult<ConfigValueResponse>> GetConfigValue(string key)
        {
            try
            {
                var value = await _consulConfigService.GetConfigAsync(key);

                return Ok(new ConfigValueResponse
                {
                    Key = key,
                    Value = value,
                    Source = "Consul"
                });
            }
            catch (Exception ex)
            {
                return NotFound(new { message = $"Configuration key '{key}' not found", error = ex.Message });
            }
        }

        /// <summary>
        /// Get all configuration from Consul
        /// </summary>
        [HttpGet("all")]
        public async Task<ActionResult<AllConfigResponse>> GetAllConfig()
        {
            try
            {
                var allConfig = await _consulConfigService.GetAllConfigAsync();

                return Ok(new AllConfigResponse
                {
                    Count = allConfig.Count,
                    Values = allConfig,
                    Source = "Consul"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to retrieve configuration", error = ex.Message });
            }
        }

        /// <summary>
        /// Set a configuration value in Consul
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<SetConfigResponse>> SetConfigValue([FromBody] SetConfigRequest request)
        {
            try
            {
                await _consulConfigService.SetConfigAsync(request.Key, request.Value);

                return Ok(new SetConfigResponse
                {
                    Success = true,
                    Key = request.Key,
                    Message = $"Configuration '{request.Key}' updated successfully in Consul"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new SetConfigResponse
                {
                    Success = false,
                    Key = request.Key,
                    Message = $"Failed to set configuration: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// Get application info using IConfiguration
        /// </summary>
        [HttpGet("app-info")]
        public ActionResult<AppInfoResponse> GetAppInfo()
        {
            var appName = _configuration["Application:Name"] ?? "ConfigurationManager.Api";
            var environment = _configuration["ASPNETCORE_ENVIRONMENT"] ?? "Production";
            var version = _configuration["Application:Version"] ?? "1.0.0";

            return Ok(new AppInfoResponse
            {
                ApplicationName = appName,
                Environment = environment,
                Version = version,
                ConsulConfigured = IsConsulConfigured()
            });
        }

        /// <summary>
        /// Get health status including Consul connectivity
        /// </summary>
        [HttpGet("health")]
        public async Task<ActionResult<HealthResponse>> GetHealth()
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
                    Timestamp = System.DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                return StatusCode(503, new HealthResponse
                {
                    Status = "Unhealthy",
                    ConsulConnected = false,
                    Timestamp = System.DateTime.UtcNow,
                    Error = ex.Message
                });
            }
        }

        private bool IsConsulConfigured()
        {
            var hostName = _configuration["Consul:HostName"];
            var port = _configuration["Consul:Port"];
            return !string.IsNullOrEmpty(hostName) && !string.IsNullOrEmpty(port);
        }
    }

    // ========== EXAMPLE ANOTHER CONTROLLER ==========

    /// <summary>
    /// Example Weather Controller using configuration from Consul
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class WeatherController : ControllerBase
    {
        private readonly IWeatherService _weatherService;
        private readonly IConfiguration _configuration;

        public WeatherController(
            IWeatherService weatherService,
            IConfiguration configuration)
        {
            _weatherService = weatherService ?? throw new ArgumentNullException(nameof(weatherService));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        [HttpGet("{city}")]
        public async Task<ActionResult<WeatherResponse>> GetWeather(string city)
        {
            try
            {
                var weatherData = await _weatherService.GetWeatherAsync(city);

                return Ok(new WeatherResponse
                {
                    City = city,
                    Data = weatherData,
                    Source = "Consul Configuration"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to get weather", error = ex.Message });
            }
        }

        [HttpGet("config/info")]
        public ActionResult<object> GetConfigInfo()
        {
            return Ok(new
            {
                WeatherServiceUrl = _configuration["WeatherService:Url"],
                WeatherServiceConfigured = !string.IsNullOrEmpty(_configuration["WeatherService:ApiKey"]),
                Environment = _configuration["ASPNETCORE_ENVIRONMENT"]
            });
        }
    }

    // ========== RESPONSE MODELS ==========

    public class ConfigValueResponse
    {
        public string Key { get; set; }
        public string Value { get; set; }
        public string Source { get; set; }
    }

    public class AllConfigResponse
    {
        public int Count { get; set; }
        public Dictionary<string, string> Values { get; set; }
        public string Source { get; set; }
    }

    public class SetConfigRequest
    {
        public string Key { get; set; }
        public string Value { get; set; }
    }

    public class SetConfigResponse
    {
        public bool Success { get; set; }
        public string Key { get; set; }
        public string Message { get; set; }
    }

    public class AppInfoResponse
    {
        public string ApplicationName { get; set; }
        public string Environment { get; set; }
        public string Version { get; set; }
        public bool ConsulConfigured { get; set; }
    }

    public class HealthResponse
    {
        public string Status { get; set; }
        public bool ConsulConnected { get; set; }
        public DateTime Timestamp { get; set; }
        public string Error { get; set; }
    }

    public class WeatherResponse
    {
        public string City { get; set; }
        public string Data { get; set; }
        public string Source { get; set; }
    }
}
