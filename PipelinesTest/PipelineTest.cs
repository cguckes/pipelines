using System.Threading.Tasks;
using FluentAssertions;
using Pipelines;
using Xunit;

namespace PipelinesTest
{
    public class PipelineTest
    {
        [Fact]
        public async Task SimplePipelineWorks()
        {
            var pipeline = new Pipeline(
                new TestStep.ReverseString(),
                new TestStep.ReverseString()
            );

            var result = await pipeline.Execute<string, string>("reverse-twice");
            result.Should().Be("reverse-twice");
        }
    }
}