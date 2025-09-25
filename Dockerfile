# Use the official .NET 8 SDK image for building
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

# Set the working directory
WORKDIR /app

# Copy the project file and restore dependencies
COPY CahtBotApi/CahtBotApi.csproj CahtBotApi/
RUN dotnet restore CahtBotApi/CahtBotApi.csproj

# Copy the rest of the source code
COPY . .

# Build and publish the application
RUN dotnet publish CahtBotApi/CahtBotApi.csproj -c Release -o /app/publish

# Use the official .NET 8 runtime image for the final image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime

# Set the working directory
WORKDIR /app

# Copy the published app from the build stage
COPY --from=build /app/publish .

# Expose the port the app runs on
EXPOSE 8080

# Set environment variable for ASP.NET Core
ENV ASPNETCORE_URLS=http://+:8080

# Start the application
ENTRYPOINT ["dotnet", "CahtBotApi.dll"]