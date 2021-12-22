using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Pipelines;
using Xunit;

namespace PipelinesTest.Unit
{
    [Trait("Category", "Unit")]
    public class ValidationTest
    {
        [Fact]
        public async Task InputTypeIsChecked()
        {
            var wrongInAndOutput = new Pipeline(new TestStep.ReverseString());
            await wrongInAndOutput.Invoking(p => p.Execute<int, string>(1))
                .Should().ThrowAsync<ArgumentException>();
        }

        [Fact]
        public async Task OutputTypeIsChecked()
        {
            var wrongInAndOutput = new Pipeline(new List<IStep> {new TestStep.ReverseString()});
            await wrongInAndOutput.Invoking(p => p.Execute<string, int>("test"))
                .Should().ThrowAsync<ArgumentException>();
        }

        [Fact]
        public async Task InternalTypesAreChecked()
        {
            var wrongInternalParameters = new Pipeline(new TestStep.ReverseString(), new TestStep.IntToString());
            await wrongInternalParameters.Invoking(p => p.Execute<string, string>("test"))
                .Should().ThrowAsync<ArgumentException>();
        }
    }
}