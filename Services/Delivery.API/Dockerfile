FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["Services/Delivery.API/Delivery.API.csproj", "Services/Delivery.API/"]
COPY ["Shared/FoodFleet.Shared.Events/FoodFleet.Shared.Events.csproj", "Shared/FoodFleet.Shared.Events/"]
COPY ["Shared/FoodFleet.Shared.Messaging/FoodFleet.Shared.Messaging.csproj", "Shared/FoodFleet.Shared.Messaging/"]
RUN dotnet restore "Services/Delivery.API/Delivery.API.csproj"
COPY . .
RUN dotnet publish "Services/Delivery.API/Delivery.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Delivery.API.dll"]
