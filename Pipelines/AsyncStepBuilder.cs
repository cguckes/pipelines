using System;
using System.Collections.Immutable;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Pipelines
{
    public class AsyncStepBuilder<TIn, TOut> : IStepBuilder
    {
        private readonly Func<TIn, Task<TOut>> _function;
        private readonly string _name;
        private readonly ImmutableList<(string name, Func<TIn, bool> check)> _preconditions;
        private readonly ImmutableList<(string name, Func<TOut, bool> check)> _postconditions;
        private readonly ILogger _logger;
        private readonly Func<TIn, string> _inputIdentifier;
        private readonly Func<TOut, string> _outputIdentifier;

        public AsyncStepBuilder(
            Func<TIn, Task<TOut>> function,
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

        public IStep Build()
            => new TraceableAsyncStep<TIn, TOut>(
                _function,
                _name,
                _preconditions,
                _postconditions,
                _logger,
                _inputIdentifier,
                _outputIdentifier);

        public AsyncStepBuilder<TIn, TOut> Named(string name)
            => new AsyncStepBuilder<TIn, TOut>(
                _function,
                name,
                _preconditions,
                _postconditions,
                _logger,
                _inputIdentifier,
                _outputIdentifier);

        public AsyncStepBuilder<TIn, TOut> AssumingThat(Func<TIn, bool> check)
            => AssumingThat(check.Method.Name, check);

        public AsyncStepBuilder<TIn, TOut> AssumingThat(string name, Func<TIn, bool> check)
            => new AsyncStepBuilder<TIn, TOut>(
                _function,
                _name,
                _preconditions.Add((name, check)),
                _postconditions,
                _logger,
                _inputIdentifier,
                _outputIdentifier);

        public AsyncStepBuilder<TIn, TOut> AssumingAfter(Func<TOut, bool> check)
            => WithPostcondition(check.Method.Name, check);

        public AsyncStepBuilder<TIn, TOut> WithPostcondition(string name, Func<TOut, bool> check)
            => new AsyncStepBuilder<TIn, TOut>(
                _function,
                _name,
                _preconditions,
                _postconditions.Add((name, check)),
                _logger,
                _inputIdentifier,
                _outputIdentifier);

        IStepBuilder IStepBuilder.LoggingTo(ILogger logger) 
            => LoggingTo(logger);

        public AsyncStepBuilder<TIn, TOut> LoggingTo(ILogger logger)
            => new AsyncStepBuilder<TIn, TOut>(
                _function,
                _name,
                _preconditions,
                _postconditions,
                logger,
                _inputIdentifier,
                _outputIdentifier);

        public AsyncStepBuilder<TIn, TOut> WithoutLogging()
            => LoggingTo(null);
    }
}