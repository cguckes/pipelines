using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Pipelines;
using Xunit;

namespace PipelinesTest
{
    public class ValidationTest
    {
        [Fact]
        public async Task InputTypeIsChecked()
        {
            var wrongInAndOutput = new List<IStep> {new TestStep.ReverseString()};
            await wrongInAndOutput.Invoking(p => p.ExecutePipeline<int, string>(1))
                .Should().ThrowAsync<ArgumentException>();
        }

        [Fact]
        public async Task OutputTypeIsChecked()
        {
            var wrongInAndOutput = new List<IStep> {new TestStep.ReverseString()};
            await wrongInAndOutput.Invoking(p => p.ExecutePipeline<string, int>("test"))
                .Should().ThrowAsync<ArgumentException>();
        }

        [Fact]
        public async Task InternalTypesAreChecked()
        {
            var wrongInternalParameters = new List<IStep>
                {new TestStep.ReverseString(), new TestStep.IntToString()};
            await wrongInternalParameters.Invoking(p => p.ExecutePipeline<string, string>("test"))
                .Should().ThrowAsync<ArgumentException>();
        }
    }
}