using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pipelines
{
    public static class Pipeline
    {
        public static async Task<TResult> ExecutePipeline<TInput, TResult>(
            this IEnumerable<IStep> steps,
            TInput input)
        {
            var stepArray = steps as IStep[] ?? steps.ToArray();
            stepArray.ValidatePipeline<TInput, TResult>();
            object result = input;
            foreach (var step in stepArray)
            {
                result = await step.Execute(result);
            }

            return (TResult) result;
        }

        public static void ValidatePipeline<TInput, TResult>(this IEnumerable<IStep> steps)
        {
            var stepArray = steps as IStep[] ?? steps.ToArray();
            EnsureFirstStepCanProcessPipelineInput<TInput, TResult>(stepArray);
            EnsureLastStepProducesPipelineOutput<TInput, TResult>(stepArray);
            EnsureAllStepsCanProcessTheOutputOfThePredecessor(stepArray);
        }

        private static void EnsureAllStepsCanProcessTheOutputOfThePredecessor(IReadOnlyList<IStep> stepArray)
        {
            for (var i = 0; i < stepArray.Count - 1; i++)
            {
                var currentStepOutput = stepArray[i].GetOutputType();
                var nextStepInput = stepArray[i + 1].GetInputType();

                if (!nextStepInput.IsAssignableFrom(currentStepOutput))
                {
                    throw new ArgumentException(
                        $"Step {i} (zero based) of the pipeline produces {currentStepOutput.FullName} " +
                        $"but step {i + 1} expects {nextStepInput.FullName}");
                }
            }
        }

        private static void EnsureLastStepProducesPipelineOutput<TInput, TResult>(IStep[] stepArray)
        {
            var lastStepOutputType = stepArray.Last().GetOutputType();
            if (!typeof(TResult).IsAssignableFrom(lastStepOutputType))
            {
                throw new ArgumentException(
                    $"The first pipeline step produces {lastStepOutputType.FullName} but you're expecting " +
                    $"{typeof(TResult).FullName}");
            }
        }

        private static void EnsureFirstStepCanProcessPipelineInput<TInput, TResult>(IStep[] stepArray)
        {
            var firstStepInputType = stepArray.First().GetInputType();
            if (!firstStepInputType.IsAssignableFrom(typeof(TInput)))
            {
                throw new ArgumentException(
                    $"The first pipeline step expects {firstStepInputType.FullName} but you're passing " +
                    $"{typeof(TInput).FullName}");
            }
        }

        private static Type GetInputType(this IStep step)
            => step.GetStepType().GenericTypeArguments.First();

        private static Type GetOutputType(this IStep step)
            => step.GetStepType().GenericTypeArguments.Last();

        private static Type GetStepType(this IStep step)
            => step.GetType().GetBaseTypes().ToList().First(IsStep);

        private static bool IsStep(Type type)
            => type.Name.StartsWith("Step") &&
               type.GetInterface(nameof(IStep)) != null &&
               type.IsAbstract &&
               type.GenericTypeArguments.Length == 2;

        private static IEnumerable<Type> GetBaseTypes(this Type type)
        {
            for (var current = type; current != null; current = current.BaseType)
            {
                yield return current;
            }
        }
        
        // TODO: cache input and output for steps, reflection is slow!
    }
}