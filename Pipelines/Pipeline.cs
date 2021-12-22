using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;

namespace Pipelines
{
    public class Pipeline
    {
        private readonly ImmutableArray<IStep> _steps;

        public Pipeline() => _steps = ImmutableArray<IStep>.Empty;

        public Pipeline(params IStep[] steps)
            => _steps = ImmutableArray<IStep>.Empty.AddRange(steps);

        public Pipeline(params IStepBuilder[] steps)
            => _steps = ImmutableArray<IStep>.Empty.AddRange(steps.Select(step => step.Build()));

        public Pipeline(IEnumerable<IStep> steps)
            => _steps = ImmutableArray<IStep>.Empty.AddRange(steps);

        public Pipeline(IEnumerable<IStepBuilder> steps)
            => _steps = ImmutableArray<IStep>.Empty.AddRange(steps.Select(step => step.Build()));

        public async Task<TResult> Execute<TInput, TResult>(TInput input)
        {
            ValidatePipeline<TInput, TResult>();
            object result = input;

            foreach (var step in _steps)
            {
                result = await step.Execute(result);
            }

            return (TResult) result;
        }

        public void ValidatePipeline<TInput, TResult>()
        {
            EnsureFirstStepCanProcessPipelineInput<TInput>();
            EnsureLastStepProducesPipelineOutput<TResult>();
            EnsureAllStepsCanProcessTheOutputOfThePredecessor();
        }

        private void EnsureAllStepsCanProcessTheOutputOfThePredecessor()
        {
            for (var i = 0; i < _steps.Length - 1; i++)
            {
                var currentStepOutput = _steps[i].GetOutputType();
                var nextStepInput = _steps[i + 1].GetInputType();

                if (!nextStepInput.IsAssignableFrom(currentStepOutput))
                {
                    throw new ArgumentException(
                        $"Step {i} (zero based) of the pipeline produces {currentStepOutput.FullName} " +
                        $"but step {i + 1} expects {nextStepInput.FullName}");
                }
            }
        }

        private void EnsureFirstStepCanProcessPipelineInput<TInput>()
        {
            var firstStepInputType = _steps.First().GetInputType();
            if (!firstStepInputType.IsAssignableFrom(typeof(TInput)))
            {
                throw new ArgumentException(
                    $"The first pipeline step expects {firstStepInputType.FullName} but you're passing " +
                    $"{typeof(TInput).FullName}");
            }
        }

        private void EnsureLastStepProducesPipelineOutput<TResult>()
        {
            var lastStepOutputType = _steps.Last().GetOutputType();
            if (!typeof(TResult).IsAssignableFrom(lastStepOutputType))
            {
                throw new ArgumentException(
                    $"The last pipeline step produces {lastStepOutputType.FullName} but you're expecting " +
                    $"{typeof(TResult).FullName}");
            }
        }

        public Pipeline Add(IStep step)
            => new Pipeline(_steps.Add(step));

        public Pipeline AddRange(IEnumerable<IStep> step)
            => new Pipeline(_steps.AddRange(step));
    }
}