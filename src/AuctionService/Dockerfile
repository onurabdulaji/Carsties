# Stage 1: Build the application
# Use the official .NET SDK image for building
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

# Set the working directory inside the container
WORKDIR /app

# Copy the solution file and project files first to leverage Docker cache
# This layer will only be rebuilt if the project/solution files change
COPY Carsties.sln ./
COPY src/AuctionService/AuctionService.csproj ./src/AuctionService/
COPY src/Contracts/Contracts.csproj ./src/Contracts/

# Restore dependencies
RUN dotnet restore ./src/AuctionService/AuctionService.csproj

# Copy the rest of the source code
COPY . .

# Publish the application for release
RUN dotnet publish ./src/AuctionService/AuctionService.csproj -c Release -o /app/publish

# Stage 2: Runtime image
# Use the official ASP.NET runtime image, which is smaller and more secure
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime

WORKDIR /app

# Expose the port the application runs on inside the container
EXPOSE 80

# Copy the published output from the build stage
COPY --from=build /app/publish .

# Define the command to run the application when the container starts
ENTRYPOINT [ "dotnet" , "AuctionService.dll" ]