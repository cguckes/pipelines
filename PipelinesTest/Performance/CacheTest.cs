using System.Diagnostics;
using System.Threading.Tasks;
using FluentAssertions;
using Pipelines;
using Xunit;

namespace PipelinesTest.Performance
{
    [Trait("Category", "Performance")]
    public class CacheTest
    {
        private const int ExpectedPerformanceImprovementFactor = 20;

        [Fact]
        public void CachePerformanceTest()
        {
            var pipeline = new Pipeline(
                AStep.ThatExecutes<int, int>(Task.FromResult).Build(),
                new TestStep.IntToString(),
                new TestStep.ReverseString(),
                AStep.ThatExecutes<string, string>(Task.FromResult).Build()
            );

            var firstRunTicks = MeasureValidation(pipeline);
            var secondRunTicks = MeasureValidation(pipeline);

            secondRunTicks.Should().BeLessThan(firstRunTicks / ExpectedPerformanceImprovementFactor);
        }

        private static long MeasureValidation(Pipeline pipeline)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            pipeline.Validate<int, string>();
            stopwatch.Stop();

            return stopwatch.ElapsedTicks;
        }
    }
}