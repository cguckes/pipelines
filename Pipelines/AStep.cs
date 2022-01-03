using System;
using System.Collections.Immutable;
using System.Threading.Tasks;

namespace Pipelines
{
    public static class AStep
    {
        public static AsyncStepBuilder<T, T> ThatExecutes<T>(Func<T, Task<T>> function)
            => ThatExecutes<T, T>(function);

        public static AsyncStepBuilder<TIn, TOut> ThatExecutes<TIn, TOut>(Func<TIn, Task<TOut>> function)
            => new AsyncStepBuilder<TIn, TOut>(
                function,
                function.Method.Name,
                ImmutableList<(string name, Func<TIn, bool> check)>.Empty,
                ImmutableList<(string name, Func<TOut, bool> check)>.Empty,
                null,
                input => input?.ToString() ?? $"{typeof(TIn).Name}: null",
                output => output?.ToString() ?? $"{typeof(TOut).Name}: null");
    }
}