# Stage 1: Compile the application using the .NET SDK
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /app

# Copy project file and restore NuGet dependencies
# Separating this step improves caching efficiency
COPY *.csproj ./
RUN dotnet restore

# Copy all source files and publish the release build
COPY . .
RUN dotnet publish -c Release -o out

# Stage 2: Create the final runtime image
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app

# Copy only the compiled output from the build stage to keep the image lightweight
COPY --from=build /app/out .

# Configure the container to listen on port 8080 by default
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

# Specify the command to run the application
ENTRYPOINT ["dotnet", "UrlShortenerService.dll"]