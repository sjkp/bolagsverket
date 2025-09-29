# Use the official .NET 9.0 SDK image for building
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /app

# Install Native AOT prerequisites
RUN apt-get update && apt-get install -y \
    clang \
    zlib1g-dev \
    && rm -rf /var/lib/apt/lists/*

# Copy project files
COPY *.csproj ./
COPY *.sln ./

# Restore dependencies
RUN dotnet restore

# Copy source code
COPY . ./

# Build and publish the application with AOT
RUN dotnet publish -c Release -o out --self-contained true

# Use the runtime image for the final stage
FROM mcr.microsoft.com/dotnet/runtime-deps:9.0 AS runtime
WORKDIR /app

# Install curl and tar for downloading cloudzip
RUN apt-get update && apt-get install -y curl tar && rm -rf /var/lib/apt/lists/*

# Download and install cloudzip
RUN curl -L https://github.com/ozkatz/cloudzip/releases/download/v0.0.1/cz-v0.0.1-linux-amd64.tar.gz -o cloudzip.tar.gz \
    && tar -xzf cloudzip.tar.gz \
    && mv cz /usr/local/bin/ \
    && rm cloudzip.tar.gz

# Copy the published application
COPY --from=build /app/out .

# Expose the port (default ASP.NET Core port)
EXPOSE 8080

# Set the entry point
ENTRYPOINT ["./AspNetCoreMcpPerSessionTools"]