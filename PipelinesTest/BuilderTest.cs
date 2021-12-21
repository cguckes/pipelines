using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Pipelines;
using Xunit;

namespace PipelinesTest
{
    public class BuilderTest
    {
        [Fact]
        public void PipelineCanBeCreatedWithBuilders()
        {
            new Pipeline(AStep.ThatExecutes<int, int>(Task.FromResult))
                .Should().NotBeNull();
            
            new Pipeline(new[]
                {
                    AStep.ThatExecutes<int, int>(Task.FromResult),
                    AStep.ThatExecutes<int, int>(Task.FromResult)
                }.Cast<IStepBuilder>())
                .Should().NotBeNull();
        }
    }
}