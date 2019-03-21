# victron-data-adapter
.NET Core Adapter to forward the streaming data from a Victron solar charge controller to a InfluxDB

# VeDirectCommunication
.NET Standard implementation of the protocols (Text and Hex) used by Victron Solar Charge Controllers (BlueSolar/SmartSolar)

## Initialization, Basic Usage
To read and write the data stream sent by the Charge Controller the `VeDirectDevice` needs a `IVictronStream`, a sample implementation with a TCP Socket is implemented by `NetworkVictronStream`. The `NetworkVictronStream` takes an `IOptions<IpDataSourceConfig>` and an `ILogger<VeDirectDevice>` as arguments. Consult the Microsoft manual for `Microsoft.Extensions.Options` and `Microsoft.Extensions.Logging` if you don't know how to get those.

You also need a `RegisterParser` to do something useful with the data the Victron device returns.

### Complete example: Getting a single register (run this in an async method)
```csharp
var networkConfig = new IpDataSourceConfig
{
	Hostname = "192.168.178.123",
	Port = 12345
};
IVictronStream stream = new NetworkVictronStream(new OptionsWrapper<IpDataSourceConfig>(networkConfig));
var device = new VeDirectDevice(stream, NullLogger<VeDirectDevice>.Instance);
await device.Start();

var registerParser = new RegisterParser();

var modelNameBytes = await device.GetRegister(VictronRegister.ModelName);
var modelName = registerParser.ParseString(modelNameBytes);
Console.WriteLine(modelName); // prints e.g. "SmartSolar Charger MPPT 100/15"

await device.Stop();
```

## Getting data without polling
Instead of polling all registers constantly, you have two ways of letting the charge controller send you the values by itself:

### Simple way (Text mode)
If there are no hex messages for a certain time (the Victron datasheet is very vague on this by saying "several seconds") the charge controller sends a defined list of fields in ASCII (see the datasheet for which fields there are).

```csharp

public void Start() {
	// initialization here

	device.TextMessageReceived += TextReceived;
}

private void TextReceived(object sender, TextMessageReceivedEventArgs e)
{
	foreach (var field in e.Data.Fields)
	{
		Console.WriteLine($"{field.Key}: {field.Value}");
	}
}
```

### Advanced way (Hex Async mode)
If there are have been hex messages in the last two minutes (you can use the `Ping()` method to force this) the charge controller sends some registers by itself. You can get the supported Registers by calling `SupportedAsyncRegisters.Get(VictronVersion)` with the charge controller firmware version.

#### Getting the supported registers (run this in an async method)
```csharp
var pingResponse = await _device.Ping();
var version = _registerParser.ParsePingResponse(pingResponse);
var asyncRegisters = SupportedAsyncRegisters.Get(version).ToList();
```

#### Handling the messages
```csharp
public void Start() {
	// initialization here

	device.AsyncMessageReceived += AsyncReceived;
}

private void AsyncReceived(object sender, AsyncMessageReceivedEventArgs e)
{
	Console.WriteLine($"{e.Data.Register}: {e.Data.RegisterValue}");
}
```


## TODO:
- hex mode:
	- add SetRegister support
	- version release candidate
	- capabilities enum
- maybe put library part in seperate repo
