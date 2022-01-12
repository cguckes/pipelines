using System;
using System.Threading.Tasks;
using FluentAssertions;
using Pipelines;
using Xunit;

namespace PipelinesTest.Unit
{
    [Trait("Category", "Unit")]
    public class AsyncStepTest
    {
        [Fact]
        public async Task WorksForSimpleTasks()
        {
            var pipeline = new Pipeline(
                AStep.ThatExecutes<string, string>(str => Task.FromResult(str.ToUpperInvariant())).Build()
            );

            (await pipeline.Execute<string, string>("test")).Should().Be("TEST");
        }

        [Fact]
        public async Task CanExecuteSynchronousFunctions()
        {
            var pipeline = new Pipeline(
                AStep.ThatExecutes<string>(str => str.ToUpperInvariant()).Build()
            );

            (await pipeline.Execute("test")).Should().Be("TEST");
        }

        [Fact]
        public async Task ChecksAllPreconditions()
        {
            var pipeline = new Pipeline(
                AStep.ThatExecutes<string, string>(Task.FromResult)
                    .AssumingThat("NotNull", str => str != null)
                    .AssumingThat("NotEmpty", str => !string.IsNullOrEmpty(str))
                    .Build()
            );

            await pipeline
                .Invoking(p => p.Execute<string, string>(null))
                .Should()
                .ThrowAsync<ArgumentException>()
                .WithMessage("*Preconditions*not*NotNull*NotEmpty*");
        }

        [Fact]
        public async Task ChecksAllPostconditions()
        {
            var pipeline = new Pipeline(
                AStep.ThatExecutes<string, string>(Task.FromResult)
                    .WithPostcondition("NotNull", str => str != null)
                    .WithPostcondition("NotEmpty", str => !string.IsNullOrEmpty(str))
                    .Build()
            );

            await pipeline
                .Invoking(p => p.Execute<string, string>(null))
                .Should()
                .ThrowAsync<ArgumentException>()
                .WithMessage("*Postconditions*not*NotNull*NotEmpty*");
        }
    }
}