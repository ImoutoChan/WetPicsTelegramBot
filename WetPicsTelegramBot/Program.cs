using System;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using NLog.Targets;

namespace WetPicsTelegramBot
{

    class Program
    {
        static ILogger Logger { get; } = ApplicationLogging.CreateLogger<Program>();

        static void Main(string[] args)
        {
            ApplicationLogging.LoggerFactory.AddNLog();
            
            var target = new FileTarget
            {
                Layout = "${date:format=HH\\:mm\\:ss.fff}|${logger}|${uppercase:${level}}|${message} ${exception}",
                FileName = "logs\\${shortdate}\\nlog-${date:format=yyyy.MM.dd}.log"
            };
            NLog.Config.SimpleConfigurator.ConfigureForTargetLogging(target, NLog.LogLevel.Trace);

            Logger.LogDebug("App started");

            using (var bot = new Bot("363430484:AAEeGzJTboUScF5V1lHX8s_3fk3EKRX9PqQ"))
            {
                while (true)
                {
                    Console.ReadLine();
                }
            };

            Logger.LogDebug("App stopped");
        }
    }
}