using Microsoft.Extensions.Logging;

namespace Pipelines
{
    public interface IStepBuilder
    {
        IStep Build();
        IStepBuilder LoggingTo(ILogger logger);
    }
}