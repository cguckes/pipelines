using Microsoft.Extensions.Logging;
using PipelinesTest;

namespace ExampleProject
{
    class Program
    {
        static void Main(string[] args)
        {
            var example = new ExampleCode(GetLogger());
            var result = example.RunExampleCode().Result;
        }

        private static ILogger GetLogger() 
            => LoggerFactory
                .Create(
                    builder => builder
                        .AddFilter("Microsoft", LogLevel.Error)
                        .AddFilter("System", LogLevel.Error)
                        .AddFilter("Program", LogLevel.Debug)
                        .AddLog4Net())
                .CreateLogger("Program");
    }
}