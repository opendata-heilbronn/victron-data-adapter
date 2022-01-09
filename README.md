# victron-data-adapter
.NET Core Adapter to forward the streaming data from a Victron solar charge controller to a InfluxDB. You can use this directly if you want to write to InfluxDB, else you can view this as an example project on how to use the VeDirectCommunication Library.

# VeDirectCommunication
.NET Standard implementation of the protocols (Text and Hex) used by Victron Solar Charge Controllers (BlueSolar/SmartSolar) and other Victron Energy products (mainly tested with a "SmartSolar MPPT 100/15").

## Communication with the device

There is an abstraction for the communication with the device called `IVictronStream`. This allows the communication with the device to happen without the need of a direct serial connection to the device.

There is a sample implementation `NetworkVictronStream` for doing the VE.Direct communication over an [Ethernet to TTL Serial](https://www.pusr.com/products/serial-to-ethernet-converter-modules-usr-tcp232-t2.html) adapter (there are <20$ clones from eBay/AliExpress, be aware you'll usually also need a 5V/3.3V level converter).

The protocol implementation on the Victron device is very robust, it worked fine even _with an Ethernet-To-Serial adapter through a VPN over a 4G mobile network_ (this (+ some TCP virtual serial port software) is also, how you can use the Victron software remotely).

You could easily write an implementation of `IVictronStream` to use a directly hooked up serial port with `System.IO.Ports.SerialPort`, likely it would also be possible ot connect to the device via bluetooth.

## Initialization, Basic Usage
To read and write the data stream sent by the charge controller the `VeDirectDevice` needs a `IVictronStream`, like a `NetworkVictronStream`. The `NetworkVictronStream` takes an `IOptions<IpDataSourceConfig>` and an `ILogger<VeDirectDevice>` as arguments. Consult the Microsoft manual for `Microsoft.Extensions.Options` and `Microsoft.Extensions.Logging` if you don't know how to get those.

You also need a `RegisterParser` to interpret the raw bytes the Victron device returns.

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
If there are have been hex messages in the last two minutes (you can use the `Ping()` method to force this) the charge controller sends some registers by itself. You can get a list of supported registers by calling `SupportedAsyncRegisters.Get(VictronVersion)` with the charge controller firmware version.

#### Getting the supported registers (run this in an async method)
```csharp
var pingResponse = await _device.Ping();
var version = _registerParser.ParsePingResponse(pingResponse);
var asyncRegisters = SupportedAsyncRegisters.Get(version).ToList();
```

#### Handling the async messages
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


## Unimplemented features:
- hex mode:
	- add SetRegister support
	- version release candidate
	- capabilities enum
