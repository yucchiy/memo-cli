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
            var command = new Command("list-category")
            {
                new Option<string>(
                    new string[] {"--category", "-c"},
                    () => string.Empty,
                    "Filter list by category name with regular expression"
                ),
            };
            command.AddAlias("ls-category");

            return command;
        }

        protected override async Task<int> ExecuteCommand(Input input, CancellationToken token)
        {
            foreach (var category in CollectCategories(input.Category))
            {
                await Output.WriteAsync(string.Format("{0}\n", category.Name));
            }

            return Cli.SuccessExitCode;
        }

        private Category[] CollectCategories(string categoryPattern)
        {
            if (string.IsNullOrEmpty(categoryPattern)) return Categories;

            return Categories
                .Where(category => Regex.IsMatch(category.Name, categoryPattern))
                .OrderBy(category => category.Name)
                .ToArray();
        }
    }
}