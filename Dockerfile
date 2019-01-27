FROM microsoft/dotnet:2.2-aspnetcore-runtime AS base
WORKDIR /app
EXPOSE 80

FROM microsoft/dotnet:2.2-sdk AS build
WORKDIR /src
COPY *.sln ./
COPY NuGet.config ./
COPY WetPicsTelegramBot.WebApp/WetPicsTelegramBot.WebApp.csproj WetPicsTelegramBot.WebApp/
COPY WetPicsTelegramBot.Data/WetPicsTelegramBot.Data.csproj WetPicsTelegramBot.Data/
COPY PixivApi/PixivApi.csproj PixivApi/
RUN dotnet restore "WetPicsTelegramBot.WebApp/WetPicsTelegramBot.WebApp.csproj"
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
