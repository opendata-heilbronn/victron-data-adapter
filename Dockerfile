FROM microsoft/dotnet:2.1-runtime
WORKDIR /app
COPY src/VictronDataAdapter/bin/Release/netcoreapp2.1/ ./
ENTRYPOINT ["dotnet", "VictronDataAdapter.dll"]