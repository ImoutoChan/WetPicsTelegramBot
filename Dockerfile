FROM microsoft/aspnetcore:2.0 AS base
WORKDIR /app
EXPOSE 80

FROM microsoft/aspnetcore-build:2.0 AS build
WORKDIR /src
COPY *.sln ./
COPY NuGet.config ./
COPY WetPicsTelegramBot.WebApp/WetPicsTelegramBot.WebApp.csproj WetPicsTelegramBot.WebApp/
RUN dotnet restore
COPY . .
WORKDIR /src/WetPicsTelegramBot.WebApp
RUN dotnet build -c Release -o /app

FROM build AS publish
RUN dotnet publish -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
RUN apt-get update
RUN apt-get install -y libgdiplus
ENTRYPOINT ["dotnet", "WetPicsTelegramBot.WebApp.dll"]
