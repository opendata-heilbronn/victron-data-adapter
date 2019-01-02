# victron-data-adapter
.NET Core Adapter to forward the streaming data from a Victron solar charge controller to a InfluxDB

## TODO:
- fix parser implementation (see https://www.victronenergy.com/live/vedirect_protocol:faq#framehandler_reference_implementation)
	- Check checksum?
	- handle different fields per block
	- data persistence
- Influx Sink
- robust network code (retry if connection died)
- Code Cleanup
- Dockerfile
