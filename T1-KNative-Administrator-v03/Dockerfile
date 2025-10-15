# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app
EXPOSE 8080


# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["T1-KNative-Administrator-v03.csproj", "."]
RUN dotnet restore "./T1-KNative-Administrator-v03.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "./T1-KNative-Administrator-v03.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
WORKDIR /src
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./T1-KNative-Administrator-v03.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

RUN mkdir -p /home/app/.kube
COPY kubeconfig /home/app/.kube/config
# RUN chmod 600 /home/app/.kube/config

ENTRYPOINT ["dotnet", "T1-KNative-Administrator-v03.dll"]
