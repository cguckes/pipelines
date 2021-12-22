using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Pipelines
{
    internal static class PipelineExtensions
    {
        private static readonly ConcurrentDictionary<Type, Type> InputTypeCache =
            new ConcurrentDictionary<Type, Type>();

        internal static Type GetInputType(this IStep step)
            => InputTypeCache.GetOrAdd(step.GetStepType(), t => t.GenericTypeArguments.First());

        private static readonly ConcurrentDictionary<Type, Type> OutputTypeCache
            = new ConcurrentDictionary<Type, Type>();

        internal static Type GetOutputType(this IStep step)
            => OutputTypeCache.GetOrAdd(step.GetStepType(), t => t.GenericTypeArguments.Last());

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

        static PipelineExtensions()
        {
        }
    }
}