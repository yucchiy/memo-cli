using System.Linq;
using System.CommandLine;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Threading;

namespace Memo
{
    public class ListCategoryCommand : CommandBase<ListCategoryCommand.Input>
    {
        public class Input : CommandInput
        {
            public string Category { get; set; }
        }

        protected override Command CreateCommand()
        {
            var command = new Command("list-category");
            command.AddAlias("ls-category");

            return command;
        }

        protected override async Task<int> ExecuteCommand(Input input, CancellationToken token)
        {
            foreach (var category in await Context.CategoryService.GetAllAsync(token))
            {
                await Context.Output.WriteAsync(string.Format("{0}\n", category.Id));
            }

            return Cli.SuccessExitCode;
        }
    }
}