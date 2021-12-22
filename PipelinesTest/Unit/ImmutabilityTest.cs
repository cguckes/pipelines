using System.Threading.Tasks;
using FluentAssertions;
using Pipelines;
using Xunit;

namespace PipelinesTest.Unit
{
    [Trait("Category", "Unit")]
    public class ImmutabilityTest
    {
        [Fact]
        public void AddingStepCreatesNewPipeline()
        {
            var pipeline1 = new Pipeline();
            var pipeline2 = pipeline1.Add(AStep.ThatExecutes<string, string>(Task.FromResult).Build());
            pipeline1.Should().NotBeSameAs(pipeline2);
        }

        [Fact]
        public void AddingMultipleStepsCreatesNewPipeline()
        {
            var pipeline1 = new Pipeline();
            var pipeline2 = pipeline1.AddRange(
                new[] {AStep.ThatExecutes<string, string>(Task.FromResult).Build()}
            );
            pipeline1.Should().NotBeSameAs(pipeline2);
        }
    }
}