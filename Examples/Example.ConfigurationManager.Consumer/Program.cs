using ConfigurationManager.Api;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddConsul(
                    hostName: "http://127.0.0.1",
                    port: 8500,
                    serviceHostName: "DC1",
                    mainFolder: "apps/myapp"
                )
                .Build();

// Register IConfiguration in DI container
builder.Services.AddSingleton<IConfiguration>(configuration);

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
