# Use the official ASP.NET Core runtime image

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY Transconnect.sln .
COPY TAPI TAPI
COPY TransLib TransLib
COPY TransDebug TransDebug
COPY TranscoTests TranscoTests

# Restore dependencies for all projects in the solution

RUN dotnet restore TAPI/TAPI.csproj
RUN dotnet restore TransLib/TransLib.csproj

# Copy the rest of the application code
COPY . .

# Build the TAPI project
WORKDIR /src/TAPI
RUN dotnet build -c Release -o /app/build

# Publish the TAPI project
FROM build AS publish
RUN dotnet publish -c Release -o /app/publish

# Copy the published files to the base image and set the entry point
FROM mcr.microsoft.com/dotnet/aspnet:6.0
WORKDIR /app
EXPOSE 80
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TAPI.dll"]
