namespace Pipelines
{
    public static class APipeline
    {
        public static PipelineBuilder WithSteps(params IStepBuilder[] steps)
            => new PipelineBuilder(steps, null);
    }
}