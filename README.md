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

var appSettingsFolder = await service.AddFolderAsync(Manager.GetAppSettingsFolderName());

var isAdded = await appSettingsFolder.AddAsync("foo", "bar");
var getValue = await appSettingsFolder.GetAsync("foo");
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
