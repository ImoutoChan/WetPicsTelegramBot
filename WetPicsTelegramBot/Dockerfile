FROM microsoft/dotnet:1.1.2-runtime
ARG source
WORKDIR /app
COPY ${source:-obj/Docker/publish} .
ENTRYPOINT ["dotnet", "WetPicsTelegramBot.dll"]
