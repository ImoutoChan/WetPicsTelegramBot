using System;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
using Polly;

namespace WetPicsTelegramBot.WebApp.Factories
{
    public class PolicesFactory : IPolicesFactory
    {
        private readonly ILogger _logger;

        public PolicesFactory(ILogger<PolicesFactory> logger)
        {
            _logger = logger;
        }

        public IAsyncPolicy GetDefaultHttpRetryPolicy([CallerMemberName] string methodName = null)
            => Policy
              .Handle<Exception>()
              .RetryAsync(3, (exception, retryCount) => OnRetry(exception, retryCount, methodName));

        private void OnRetry(Exception exception, int retryCount, string methodName) 
            => _logger.LogError(
                exception, 
                $"Http request encountered an error. " 
                + $"RetryCount: {retryCount}. MethodName: {methodName}.");
    }
}