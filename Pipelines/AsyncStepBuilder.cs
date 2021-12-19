using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;

namespace Pipelines
{
    public class AsyncStepBuilder<TIn, TOut>
    {
        private readonly Func<TIn, Task<TOut>> _function;
        private readonly ImmutableList<(string name, Func<TIn, bool> check)> _preconditions;
        private readonly ImmutableList<(string name, Func<TOut, bool> check)> _postconditions;

        public AsyncStepBuilder(
            Func<TIn, Task<TOut>> function,
            ImmutableList<(string name, Func<TIn, bool> check)> preconditions,
            ImmutableList<(string name, Func<TOut, bool> check)> postconditions)
        {
            _function = function;
            _preconditions = preconditions;
            _postconditions = postconditions;
        }

        public Step<TIn, TOut> Build()
            => new TraceableAsyncStep<TIn, TOut>(
                _function,
                _preconditions, 
                _postconditions);

        public AsyncStepBuilder<TIn, TOut> WithPrecondition(string name, Func<TIn, bool> check)
            => new AsyncStepBuilder<TIn, TOut>(_function, _preconditions.Add((name, check)), _postconditions);

        public AsyncStepBuilder<TIn, TOut> WithPostcondition(string name, Func<TOut, bool> check)
            => new AsyncStepBuilder<TIn, TOut>(_function, _preconditions, _postconditions.Add((name, check)));
    }

    internal class TraceableAsyncStep<TIn, TOut> : Step<TIn, TOut>
    {
        private readonly Func<TIn, Task<TOut>> _function;
        private readonly ImmutableList<(string name, Func<TIn, bool> check)> _preconditions;
        private readonly ImmutableList<(string name, Func<TOut, bool> check)> _postconditions;

        public TraceableAsyncStep(
            Func<TIn, Task<TOut>> function,
            ImmutableList<(string name, Func<TIn, bool> check)> preconditions,
            ImmutableList<(string name, Func<TOut, bool> check)> postconditions)
        {
            _function = function;
            _preconditions = preconditions;
            _postconditions = postconditions;
        }

        protected override Task<TOut> Process(TIn input)
        {
            var failedPrechecks = _preconditions.Where(pre => !pre.check(input)).Select(pre => pre.name).ToList();
            if (failedPrechecks.Any())
            {
                throw new ArgumentException(
                    $"The following Preconditions were not met: {string.Join(", ", failedPrechecks)}");
            }

            var result = _function(input);
            return result;
        }
    }
}