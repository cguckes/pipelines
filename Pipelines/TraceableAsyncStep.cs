using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Pipelines
{
    public class TraceableAsyncStep<TIn, TOut> : Step<TIn, TOut>
    {
        private readonly Func<TIn, Task<TOut>> _function;
        private readonly string _name;
        private readonly ImmutableList<(string name, Func<TIn, bool> check)> _preconditions;
        private readonly ImmutableList<(string name, Func<TOut, bool> check)> _postconditions;
        private readonly ILogger _logger;
        private readonly Func<TIn, string> _inputIdentifier;
        private readonly Func<TOut, string> _outputIdentifier;

        public TraceableAsyncStep(Func<TIn, Task<TOut>> function,
            string name,
            ImmutableList<(string name, Func<TIn, bool> check)> preconditions,
            ImmutableList<(string name, Func<TOut, bool> check)> postconditions,
            ILogger logger,
            Func<TIn, string> inputIdentifier,
            Func<TOut, string> outputIdentifier)
        {
            _function = function;
            _name = name;
            _preconditions = preconditions;
            _postconditions = postconditions;
            _logger = logger;
            _inputIdentifier = inputIdentifier;
            _outputIdentifier = outputIdentifier;
        }

        protected override async Task<TOut> Process(TIn input)
        {
            _logger?.LogDebug($"{_name}: Checking preconditions for {_inputIdentifier(input)}.");
            _logger?.LogTrace($"{_name}: Input:");
            _logger?.LogTrace(JsonConvert.SerializeObject(input));

            Precheck(input);
            _logger?.LogDebug($"{_name}: Preconditions for {GetType().Name} met, start processing.");

            var output = await _function(input);
            _logger?.LogDebug($"{_name}: finished processing. Checking postconditions.");
            _logger?.LogTrace($"{_name}: Output:");
            _logger?.LogTrace(JsonConvert.SerializeObject(output));

            Postcheck(output);
            _logger?.LogDebug($"{_name}: Postconditions for {_outputIdentifier} met.");
            return output;
        }

        private void Precheck(TIn input)
        {
            var failedPrechecks = _preconditions
                .Where(pre => !pre.check(input))
                .Select(pre => pre.name).ToList();
            if (failedPrechecks.Any())
            {
                var msg = $"{_name}: The following Preconditions were not met: " +
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
                var msg = $"{_name}: The following Postconditions were not met: " +
                          $"{string.Join(", ", failedPostchecks)}, aborting.";
                _logger?.LogError(msg);
                throw new ArgumentException(msg);
            }
        }
    }
}