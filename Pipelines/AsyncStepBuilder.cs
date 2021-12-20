using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Pipelines
{
    public class AsyncStepBuilder<TIn, TOut>
    {
        private readonly Func<TIn, Task<TOut>> _function;
        private readonly ImmutableList<(string name, Func<TIn, bool> check)> _preconditions;
        private readonly ImmutableList<(string name, Func<TOut, bool> check)> _postconditions;
        private readonly ILogger _logger;

        public AsyncStepBuilder(
            Func<TIn, Task<TOut>> function,
            ImmutableList<(string name, Func<TIn, bool> check)> preconditions,
            ImmutableList<(string name, Func<TOut, bool> check)> postconditions,
            ILogger logger)
        {
            _function = function;
            _preconditions = preconditions;
            _postconditions = postconditions;
            _logger = logger;
        }

        public Step<TIn, TOut> Build()
            => new TraceableAsyncStep<TIn, TOut>(
                _function,
                _preconditions,
                _postconditions,
                _logger);

        public AsyncStepBuilder<TIn, TOut> WithPrecondition(string name, Func<TIn, bool> check)
            => new AsyncStepBuilder<TIn, TOut>(_function, _preconditions.Add((name, check)), _postconditions, _logger);

        public AsyncStepBuilder<TIn, TOut> WithPostcondition(string name, Func<TOut, bool> check)
            => new AsyncStepBuilder<TIn, TOut>(_function, _preconditions, _postconditions.Add((name, check)), _logger);

        public AsyncStepBuilder<TIn, TOut> LoggingTo(ILogger logger)
            => new AsyncStepBuilder<TIn, TOut>(_function, _preconditions, _postconditions, logger);

        public AsyncStepBuilder<TIn, TOut> WithoutLogging()
            => LoggingTo(null);
    }

    internal class TraceableAsyncStep<TIn, TOut> : Step<TIn, TOut>
    {
        private readonly Func<TIn, Task<TOut>> _function;
        private readonly ImmutableList<(string name, Func<TIn, bool> check)> _preconditions;
        private readonly ImmutableList<(string name, Func<TOut, bool> check)> _postconditions;
        private readonly ILogger _logger;

        public TraceableAsyncStep(
            Func<TIn, Task<TOut>> function,
            ImmutableList<(string name, Func<TIn, bool> check)> preconditions,
            ImmutableList<(string name, Func<TOut, bool> check)> postconditions,
            ILogger logger)
        {
            _function = function;
            _preconditions = preconditions;
            _postconditions = postconditions;
            _logger = logger;
        }

        protected override async Task<TOut> Process(TIn input)
        {
            _logger?.LogDebug($"Checking preconditions for {GetType().Name}.");
            _logger?.LogTrace("Input:");
            _logger?.LogTrace(JsonConvert.SerializeObject(input));

            Precheck(input);
            _logger?.LogDebug($"Preconditions for {GetType().Name} met, start processing.");

            var result = await _function(input);
            _logger?.LogDebug($"{GetType().Name} finished processing. Checking postconditions.");
            _logger?.LogTrace("Output:");
            _logger?.LogTrace(JsonConvert.SerializeObject(result));

            Postcheck(result);
            _logger?.LogDebug($"Postconditions for {GetType().Name} met.");
            return result;
        }

        private void Precheck(TIn input)
        {
            var failedPrechecks = _preconditions
                .Where(pre => !pre.check(input))
                .Select(pre => pre.name).ToList();
            if (failedPrechecks.Any())
            {
                var msg = "The following Preconditions were not met: " +
                          $"{string.Join(", ", failedPrechecks)}, aborting.";
                _logger?.LogError(msg);
                throw new ArgumentException(msg);
            }
        }

        private void Postcheck(TOut output)
        {
            var failedPostchecks = _postconditions
                .Where(post => !post.check(output))
                .Select(post => post.name).ToList();
            if (failedPostchecks.Any())
            {
                var msg = "The following Postconditions were not met: " +
                          $"{string.Join(", ", failedPostchecks)}, aborting.";
                _logger?.LogError(msg);
                throw new ArgumentException(msg);
            }
        }
    }
}