# Stage 1: Build stage using the .NET SDK image
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /app

# Copy csproj and restore dependencies to leverage Docker layer caching
COPY *.csproj ./
RUN dotnet restore

# Copy the remaining source code and build the application in Release mode
COPY . .
RUN dotnet publish -c Release -o out

# Stage 2: Runtime stage using a smaller ASP.NET runtime image
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app

# Copy the compiled output from the build stage to the runtime image
COPY --from=build /app/out .

# Configure the environment variable for the application port
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

# Define the entry point for the container to start the service
ENTRYPOINT ["dotnet", "UrlShortenerService.dll"]