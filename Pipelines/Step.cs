using System.Threading.Tasks;

namespace Pipelines
{
    public interface IStep
    {
        public Task<object> Execute(object input);
    }

    public abstract class Step<TIn, TOut> : IStep
    {
        async Task<object> IStep.Execute(object input)
            => await Process((TIn) input);

        protected abstract Task<TOut> Process(TIn input);
    }
}