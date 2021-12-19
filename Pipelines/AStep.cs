using System;
using System.Collections.Immutable;
using System.Threading.Tasks;

namespace Pipelines
{
    public static class AStep
    {
        public static AsyncStepBuilder<TIn, TOut> ThatExecutes<TIn, TOut>(Func<TIn, Task<TOut>> function)
            => new AsyncStepBuilder<TIn, TOut>(
                function,
                ImmutableList<(string name, Func<TIn, bool> check)>.Empty,
                ImmutableList<(string name, Func<TOut, bool> check)>.Empty);
    }
}