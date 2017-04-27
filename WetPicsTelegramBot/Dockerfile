FROM microsoft/dotnet:1.1-runtime
ARG source
WORKDIR /app
ENV ASPNETCORE_ENVIRONMENT Development
COPY ${source:-obj/Docker/publish} .
ENTRYPOINT ["dotnet", "WetPicsTelegramBot.dll"]
