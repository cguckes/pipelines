using System.Threading.Tasks;

namespace Pipelines
{
    public interface IStep
    {
        Task<object> Execute(object input);
    }
}