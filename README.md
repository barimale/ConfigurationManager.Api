# ConfigurationManager.Api
It is used for manage of consul instance based on a composite design pattern.

## 1. Use it as a read-only client:
'''
 IReadOnly service = new ConfigurationManager.Api.Manager(
                InputData.HostName,
                InputData.Port,
                InputData.ServiceHostName);
'''

## 2. Use it as a read-write manager:
'''
IManager service = new ConfigurationManager.Api.Manager(
                InputData.HostName,
                InputData.Port,
                InputData.ServiceHostName);

'''