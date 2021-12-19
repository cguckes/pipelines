using System.Threading.Tasks;

namespace Pipelines
{
    public abstract class Step<TIn, TOut> : IStep
    {
        async Task<object> IStep.Execute(object input)
            => await Process((TIn) input);

        protected abstract Task<TOut> Process(TIn input);
    }
}