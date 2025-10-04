# ConfigurationManager.Api, Consul.ConfigurationManager.Api
It is used for manage of consul instance.

## 1. Example of read-only client:
```
 var service = new ConfigurationManager.Api.Manager(
	InputData.HostName,
	InputData.Port,
	InputData.ServiceHostName).AsReadOnly();
```

## 2. Example of read-write manager:
```
var service = new ConfigurationManager.Api.Manager(
	InputData.HostName,
	InputData.Port,
	InputData.ServiceHostName).AsManager();

var isAdded = await service.AddAsync("foo", "bar");
var getValue = await service.GetAsync("foo");
```

## 3. Example of appSettings and connectionStrings:
```
private KeyValuePair<string, string> firstPair = new KeyValuePair<string, string>("AllowedHosts", "*");

private KeyValuePair<string, string> secondPair = new KeyValuePair<string, string>(
	"Database", 
	"Data Source=localhost,1500;Database=DB;Uid=sa;Password=Password_123#; TrustServerCertificate=True");

var service = new Manager(
                InputData.HostName,
                InputData.Port,
                InputData.ServiceHostName)
                .AsManager();

var appSettingsFolder = await service.AddFolderAsync(EagerAdapter.AppSettingsName);
await appSettingsFolder.AddAsync(firstPair.Key, firstPair.Value);

var connectionStringFolder = await service.AddFolderAsync(EagerAdapter.ConnectionStringsName);
await connectionStringFolder.AddAsync(secondPair.Key, secondPair.Value);
```
# Configuration.Api.Helpers
Adapter is provided as a shell for AppSettings and ConnectionStrings. It is initialized by using
already existed read-only client instance.

## 1. Usage of lazyAdapter:
```
var manager = new Manager(
	InputData.HostName, 
	InputData.Port, 
	InputData.ServiceHostName).AsReadOnly();

var lazyAdapter = new LazyAdapter(manager);

var @VALUE = lazyAdapter.AppSettings("KEY");
```
## 2. Usage of eagerAdapter:
```
var manager = new Manager(
	InputData.HostName, 
	InputData.Port, 
	InputData.ServiceHostName).AsReadOnly();

var eagerAdapter = new EagerAdapter(manager);

var @VALUE = eagerAdapter.AppSettings("KEY");
```
