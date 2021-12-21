using System.Linq;
using System.Threading.Tasks;
using Pipelines;

namespace PipelinesTest
{
    public static class TestStep
    {
        internal class ReverseString : Step<string, string>
        {
            protected override Task<string> Process(string input)
                => Task.FromResult(new string(input.Reverse().ToArray()));
        }

        internal class IntToString : Step<int, string>
        {
            protected override Task<string> Process(int input)
                => Task.FromResult($"{input}");
        }
    }
}