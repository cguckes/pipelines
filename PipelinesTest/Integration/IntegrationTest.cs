using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace PipelinesTest.Integration
{
    public class IntegrationTest
    {
        [Fact]
        public async Task ExecuteExampleCode()
        {
            var example = new ExampleCode(new Mock<ILogger>().Object);
            var result = await example.RunExampleCode();
            result.Internal.Id.Should().NotBeEmpty();
            result.Internal.ExternalId.Should().Be("ExternalId");
        }
    }
}
