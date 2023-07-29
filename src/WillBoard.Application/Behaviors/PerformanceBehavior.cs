﻿using MediatR;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace WillBoard.Application.Behaviors
{
    public class PerformanceBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> where TRequest : IRequest<TResponse>
    {
        private readonly ILogger _logger;

        public PerformanceBehavior(ILogger<PerformanceBehavior<TRequest, TResponse>> logger)
        {
            _logger = logger;
        }

        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            // From .NET 7 it can be changed to more memory efficient way (https://github.com/dotnet/runtime/issues/65858).
            // var startTime = Stopwatch.GetTimestamp();
            // var response = await next();
            // var timeSpanElapsed = Stopwatch.GetElapsedTime(startTime);

            var stopWatch = Stopwatch.StartNew();

            stopWatch.Start();

            var response = await next();

            var timeSpanElapsed = stopWatch.Elapsed;

            _logger.LogInformation("Handled {0}. The execution took: {1}.", typeof(TRequest).Name, timeSpanElapsed.ToString(@"mm\:ss\.fff"));

            return response;
        }
    }
}