using System.Threading.Tasks;

namespace Pipelines
{
    public interface IStep
    {
        public Task<object> Execute(object input);
    }
}