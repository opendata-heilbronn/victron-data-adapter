# victron-data-adapter
.NET Core Adapter to forward the streaming data from a Victron solar charge controller to a InfluxDB

## TODO:
- more generic parser:
	- handle different fields per block
	- data persistence?
- robust network code (retry if connection died)
