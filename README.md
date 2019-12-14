# ConfigurationManager.Api
It is used for manage of consul instance based on the composite design pattern.

## 1. Example of read-only client:
```
 IReadOnly service = new ConfigurationManager.Api.Manager(
	InputData.HostName,
	InputData.Port,
	InputData.ServiceHostName);
```

## 2. Example of read-write manager:
```
IManager service = new ConfigurationManager.Api.Manager(
	InputData.HostName,
	InputData.Port,
	InputData.ServiceHostName);
```
# Configuration.Api.Helpers
Adapter is provided as a shell for AppSettings and ConnectionStrings. It is initialized by using
already existed read-only client instance.

## 1. Usage of lazyAdapter:
```
IReadOnly manager = new Manager(
	InputData.HostName, 
	InputData.Port, 
	InputData.ServiceHostName);

var factory = _kernel.Get<IAdapterFactory>();
var lazyAdapter = factory.GetAdapter<LazyAdapter>(manager);

var @VALUE = lazyAdapter.AppSettings("KEY");
```
## 2. Usage of eagerAdapter:
```
IReadOnly manager = new Manager(
	InputData.HostName, 
	InputData.Port, 
	InputData.ServiceHostName);

var factory = _kernel.Get<IAdapterFactory>();
var eagerAdapter = factory.GetAdapter<EagerAdapter>(manager);

var @VALUE = eagerAdapter.AppSettings("KEY");
```
