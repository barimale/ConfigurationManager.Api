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
