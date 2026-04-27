FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["Services/Notification.API/Notification.API.csproj", "Services/Notification.API/"]
COPY ["Shared/FoodFleet.Shared.Events/FoodFleet.Shared.Events.csproj", "Shared/FoodFleet.Shared.Events/"]
COPY ["Shared/FoodFleet.Shared.Messaging/FoodFleet.Shared.Messaging.csproj", "Shared/FoodFleet.Shared.Messaging/"]
RUN dotnet restore "Services/Notification.API/Notification.API.csproj"
COPY . .
RUN dotnet publish "Services/Notification.API/Notification.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Notification.API.dll"]
