FROM microsoft/dotnet:2.1-sdk AS build
WORKDIR /app/src
# copy csproj only so restored project will be cached
COPY src/VictronDataAdapter/VictronDataAdapter.csproj /app/src/VictronDataAdapter/
COPY src/VeDirectCommunication/VeDirectCommunication.csproj /app/src/VeDirectCommunication/
RUN dotnet restore VictronDataAdapter/VictronDataAdapter.csproj
COPY src/ /app/src
RUN dotnet publish -c Release VictronDataAdapter/VictronDataAdapter.csproj -o /app/build

FROM microsoft/dotnet:2.1-runtime
WORKDIR /app
COPY --from=build /app/build/ ./
ENTRYPOINT ["dotnet", "VictronDataAdapter.dll"]
