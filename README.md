# ConfigurationManager.Api, Consul.ConfigurationManager.Api
It is used for manage of consul instance based on the composite design pattern.

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
private string appFolder = Guid.NewGuid().ToString();
private KeyValuePair<string, string> firstPair = new KeyValuePair<string, string>("keyOne", "valueOne");
private KeyValuePair<string, string> secondPair = new KeyValuePair<string, string>("keyTwo", "valueTwo");

service = new Manager(
                InputData.HostName,
                InputData.Port,
                InputData.ServiceHostName)
                .AsManager();

addedFolder = await service.AddFolderAsync(appFolder);
var appSettingsFolder = await addedFolder.AddFolderAsync(EagerAdapter.AppSettingsName);
await appSettingsFolder.AddAsync(firstPair.Key, firstPair.Value);
await appSettingsFolder.AddAsync(secondPair.Key, secondPair.Value);

var connectionStringFolder = await addedFolder.AddFolderAsync(EagerAdapter.ConnectionStringsName);
await connectionStringFolder.AddAsync(firstPair.Key, firstPair.Value);
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
