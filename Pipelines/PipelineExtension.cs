using System;
using System.Collections.Generic;
using System.Linq;

namespace Pipelines
{
    internal static class PipelineExtensions
    {
        internal static Type GetInputType(this IStep step)
            => step.GetStepType().GenericTypeArguments.First();

        internal static Type GetOutputType(this IStep step)
            => step.GetStepType().GenericTypeArguments.Last();

        private static Type GetStepType(this IStep step)
            => step.GetType().GetBaseTypes().ToList().First(IsStep);

        private static bool IsStep(this Type type)
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
    }
}