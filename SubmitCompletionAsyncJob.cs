using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MW.Retry.Bl;
using Quartz;


namespace SubmitCompletionService
{

    [DisallowConcurrentExecution]
    public class SubmitCompletionAsyncJob : IJob
    {
        private readonly ILogger<SubmitCompletionAsyncJob> _logger;
        private readonly IRetryHandler _retryHandler;
        public SubmitCompletionAsyncJob (ILogger<SubmitCompletionAsyncJob> logger, IRetryHandler retryHandler)
        {
            _logger = logger;
            _retryHandler = retryHandler;
        }

        public Task Execute(IJobExecutionContext context)
        {
            _logger.LogInformation("SubmitCompletionAsyncJob running at: {time}", DateTimeOffset.Now);

            _retryHandler.RetryAsync();

            return Task.CompletedTask;
        }
      
    }
}