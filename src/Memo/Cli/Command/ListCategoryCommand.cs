using System.Threading.Tasks;
using System.CommandLine;
using System.CommandLine.Invocation;

namespace Memo
{
    public class ListCategoryCommand : Command
    {
        public class Input
        {
            public string Category { get; set; }
        }

        public ListCategoryCommand() : base("list-category")
        {
            AddAlias("ls-category");
        }

        public class CommandHandler : ICommandHandler
        {
            public ListCategoryCommand.Input Input { get; set; }
            private Core.Categories.ICategoryService CategoryService { get; }

            public CommandHandler(Core.Categories.ICategoryService categoryService)
            {
                CategoryService = categoryService;
            }

            public async Task<int> InvokeAsync(InvocationContext context)
            {
                var token = context.GetCancellationToken();
                foreach (var category in await CategoryService.GetAllAsync(token))
                {
                    await System.Console.Out.WriteAsync(string.Format("{0}\n", category.Id.Value));
                }

                return Cli.SuccessExitCode;
            }
        }
    }
}