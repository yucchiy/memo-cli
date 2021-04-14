using System.CommandLine.Parsing;
using System.Threading.Tasks;

namespace Memo
{
    class Program
    {
        static async Task<int> Main(string[] args)
        {
            return await Cli.CreateCommandLineParser().InvokeAsync(args);
        }
    }
}