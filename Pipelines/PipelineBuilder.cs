using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Pipelines
{
    public class PipelineBuilder
    {
        private readonly ILogger _logger;
        private readonly ImmutableArray<IStepBuilder> _steps;

        public PipelineBuilder(IEnumerable<IStepBuilder> steps, ILogger logger)
        {
            _logger = logger;
            _steps = ImmutableArray.Create(steps.ToArray());
        }

        public PipelineBuilder LoggingTo(ILogger logger)
            => new PipelineBuilder(_steps, logger);

        public Pipeline Build()
            => new Pipeline(_logger != null
                ? _steps.Select(s => s.LoggingTo(_logger))
                : _steps);

        public Task<T> Execute<T>(T input)
            => Execute<T, T>(input);

        public Task<TOutput> Execute<TInput, TOutput>(TInput input)
            => Build().Execute<TInput, TOutput>(input);
    }
}