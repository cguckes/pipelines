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
            var pipeline = new[]
            {
                AStep
                    .ThatExecutes<string, string>(Task.FromResult)
                    .WithoutLogging()
                    .Build(),
            };

            await pipeline
                .Invoking(p => p.ExecutePipeline<string, string>("test"))
                .Should()
                .NotThrowAsync();
        }

        [Fact]
        public async Task LogsDebuggingMessages()
        {
            var logMock = MockLogger();

            var pipeline = new[]
            {
                AStep
                    .ThatExecutes<string, string>(Task.FromResult)
                    .LoggingTo(logMock.Object)
                    .WithPostcondition("NotNull", str => str != null)
                    .Build(),
            };

            try
            {
                await pipeline.ExecutePipeline<string, string>(null);
            }
            catch
            {
                // ignored
            }

            logMock.Verify(LogWithLevel(LogLevel.Error), Times.AtLeastOnce);
            logMock.Verify(LogWithLevel(LogLevel.Debug), Times.AtLeastOnce);
            logMock.Verify(LogWithLevel(LogLevel.Trace), Times.AtLeastOnce);
        }

        protected virtual Expression<Action<ILogger<LoggerTest>>> LogWithLevel(LogLevel logLevel)
            => mock => mock.Log(
                It.Is<LogLevel>(level => level == logLevel),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true));

        private static Mock<ILogger<LoggerTest>> MockLogger()
        {
            var log = new Mock<ILogger<LoggerTest>>();
            return log;
        }
    }
}