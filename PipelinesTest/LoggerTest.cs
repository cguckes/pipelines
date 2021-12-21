using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Pipelines;
using Xunit;

namespace PipelinesTest
{
    public class LoggerTest
    {
        [Fact]
        public async Task WorksWithoutLogger()
        {
            var pipeline = new Pipeline(
                AStep
                    .ThatExecutes<string, string>(Task.FromResult)
                    .WithoutLogging()
                    .Build()
            );

            await pipeline
                .Invoking(p => p.Execute<string, string>("test"))
                .Should()
                .NotThrowAsync();
        }

        [Fact]
        public async Task LogsDebuggingMessages()
        {
            var logMock = MockLogger();

            var pipeline = new Pipeline(
                AStep
                    .ThatExecutes<string, string>(Task.FromResult)
                    .Named("TestStep")
                    .LoggingTo(logMock.Object)
                    .WithPostcondition("NotNull", str => str != null)
                    .Build()
            );

            try
            {
                await pipeline.Execute<string, string>(null);
            }
            catch
            {
                // ignored
            }

            logMock.Verify(LogWithLevel(LogLevel.Error), Times.AtLeastOnce);
            logMock.Verify(LogWithLevel(LogLevel.Debug), Times.AtLeastOnce);
            logMock.Verify(LogWithLevel(LogLevel.Trace), Times.AtLeastOnce);
        }

        [Fact]
        public async Task NullsWorkInTheLogger()
        {
            var pipeline = new Pipeline(
                AStep.ThatExecutes<string, string>(Task.FromResult)
                    .LoggingTo(MockLogger().Object));
            await pipeline.Invoking(p => p.Execute<string, string>(null)).Should().NotThrowAsync();
        }

        protected virtual Expression<Action<ILogger<LoggerTest>>> LogWithLevel(LogLevel logLevel)
            => mock => mock.Log(
                It.Is<LogLevel>(level => level == logLevel),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true));

        private static Mock<ILogger<LoggerTest>> MockLogger()
            => new Mock<ILogger<LoggerTest>>();
    }
}