using System;
using System.Linq;
using System.IO;
using System.CommandLine;
using System.Threading;
using System.Threading.Tasks;

namespace Memo
{
    public class NewCommand : CommandBase<NewCommand.Input>
    {
        public class Input : CommandInput
        {
            public string Title { get; set; }
            public string Category { get; set; }
            public string Filename { get; set; }
        }

        protected override Command CreateCommand()
        {
            var command = new Command("new")
            {
                new Option<string>(
                    new string[] {"--title", "-t"},
                    "Title of note."
                ),
                new Option<string>(
                    new string[] {"--category", "-c"},
                    "Category of note. Note must belong to one category"
                ),
                new Option<string>(
                    new string[] {"--filename", "-f"},
                    "File name of note. It automatically adds '.markdown' file extension if omitted"
                ),
            };
            command .AddAlias("n");

            return command;
        }

        protected override async Task<int> ExecuteCommand(Input input, CancellationToken token)
        {
            var selectedCategory = GetOrCreateCategory(input.Category);
            var category = FindCategoryConfigOrGetDefault(selectedCategory.Name);
            switch (category.MemoCreationType)
            {
                case CreationType.Default:
                    return await CreateDefault(category, input, token);
                case CreationType.Daily:
                    return await CreateDaily(category, input, token);
                case CreationType.Weekly:
                    return await CreateWeekly(category, input, token);
            }

            return Cli.FailedExitCode;
        }

        private MemoConfig.CategoryConfig FindCategoryConfigOrGetDefault(string categoryName)
        {
            foreach (var categoryConfig in CommandConfig.MemoConfig.Categories)
            {
                if (categoryConfig.Name == categoryName)
                {
                    var category = MemoConfig.GetDefault(categoryName, categoryConfig.MemoCreationType);
                    if (!string.IsNullOrEmpty(categoryConfig.MemoFileNameFormat))
                    {
                        category.MemoFileNameFormat = categoryConfig.MemoFileNameFormat;
                    }

                    if (!string.IsNullOrEmpty(categoryConfig.MemoTitleFormat))
                    {
                        category.MemoTitleFormat = categoryConfig.MemoTitleFormat;
                    }

                    return category;
                }
            }

            return MemoConfig.GetDefault(categoryName, CreationType.Default);
        }

        private async Task<int> CreateDefault(MemoConfig.CategoryConfig category, Input input, CancellationToken token) => await CreateNoteWithTargetDateTime(category, DateTime.Now, input, token);

        private async Task<int> CreateDaily(MemoConfig.CategoryConfig category, Input input, CancellationToken token) => await CreateNoteWithTargetDateTime(category, DateTime.Now, input, token);

        private async Task<int> CreateWeekly(MemoConfig.CategoryConfig category, Input input, CancellationToken token) => await CreateNoteWithTargetDateTime(category, Utility.FirstDayOfWeek(), input, token);

        private async Task<int> CreateNoteWithTargetDateTime(MemoConfig.CategoryConfig category, DateTime targetDate, Input input, CancellationToken token)
        {
            var created = DateTime.Now;
            var meta = new NoteMetaData()
            {
                Title = Utility.Format(
                    category.MemoTitleFormat,
                    new { InputTitle = input.Title, InputFilename = input.Filename, Category = category.Name, Created = created, TargetDate = targetDate }
                ),
                Category = category.Name,
                Created = DateTime.Now,
            };
            var fileName = Utility.Format(
                category.MemoFileNameFormat,
                new { InputTitle = input.Title, InputFilename = input.Filename, Category = category.Name, Created = created, TargetDate = targetDate }
            );
            return await CreateNote(new FileInfo(Path.Combine(CommandConfig.HomeDirectory.FullName, category.Name, $"{fileName}.markdown")), meta);
        }

        private async Task<int> CreateNote(FileInfo file, NoteMetaData meta)
        {
            await Output.WriteLineAsync(file.FullName);
            if (file.Exists)
            {
                return Cli.SuccessExitCode;
            }

            await File.WriteAllTextAsync(file.FullName, meta.ToText());
            return Cli.SuccessExitCode;
        }

        private Category GetOrCreateCategory(string categoryName)
        {
            var result = Categories
                .Where(category => category.Name == categoryName);
            if (result.Any()) return result.First();

            var path = Utility.CategoryName2CategoryAbsoluteDirectoryPath(CommandConfig, categoryName);

            return Directory.Exists(path) ?
               new Category(categoryName, new DirectoryInfo(path)) :
               new Category(categoryName, Directory.CreateDirectory(path));
        }
    }
}