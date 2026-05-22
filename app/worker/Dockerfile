# Stage 1: Compile the .NET application (uses large SDK image)
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /source
COPY *.csproj .
RUN dotnet restore
COPY . .
RUN dotnet publish -c Release -o /app --no-restore

# Stage 2: Copy only compiled output into small runtime image
FROM mcr.microsoft.com/dotnet/runtime:8.0
WORKDIR /app
COPY --from=build /app .
ENTRYPOINT ["dotnet", "Worker.dll"]
