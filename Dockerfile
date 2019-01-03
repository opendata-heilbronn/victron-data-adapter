FROM microsoft/dotnet:2.1-runtime
WORKDIR /app
COPY build/ ./
ENTRYPOINT ["dotnet", "VictronDataAdapter.dll"]
