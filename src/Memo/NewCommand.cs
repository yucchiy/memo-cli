using System;
using System.Linq;
using System.IO;
using System.CommandLine;
using System.Threading;
using System.Threading.Tasks;
using System.Net;
using System.Net.Http;

namespace Memo
{
    public class NewCommand : CommandBase<NewCommand.Input>
    {
        public class Input : CommandInput
        {
            public string Title { get; set; }
            public string Category { get; set; }
            public string Filename { get; set; }
            public string Url { get; set; }
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
                new Option<string>(
                    new string[] {"--url"},
                    "Collect title and filename from url. If this option specificated, title and filename will be override."
                ),
            };
            command .AddAlias("n");

            return command;
        }

        protected override async Task<int> ExecuteCommand(Input input, CancellationToken token)
        {
            input = await NormalizeInputAsync(input, token);
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

        private async Task<Input> NormalizeInputAsync(Input input, CancellationToken token)
        {
            if (!string.IsNullOrEmpty(input.Url))
            {
                var uri = new Uri(input.Url);
                using (var client = new HttpClient())
                {
                    var response = await client.GetAsync(input.Url, token);
                    return response.StatusCode switch
                    {
                        HttpStatusCode.OK => new Input()
                        {
                            Category = input.Category,
                            Title = Utility.TryParseTitle(await response.Content.ReadAsStringAsync(), out var title) ? title : "",
                            Filename = uri.Host + "_" + Utility.LocalPath2Filename(uri.LocalPath),
                            Url = input.Url,
                        },
                        _ => input,
                    };
                }
            }
            else
            {
                return input;
            }
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

                    category.MemoTemplateFilePath = categoryConfig.MemoTemplateFilePath;
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
            return await CreateNote(fileName, meta, category, targetDate);
        }

        private async Task<int> CreateNote(string fileName, NoteMetaData meta, MemoConfig.CategoryConfig categoryConfig, DateTime targetDate)
        {
            var file = new FileInfo(Path.Combine(CommandConfig.HomeDirectory.FullName, categoryConfig.Name, $"{fileName}.markdown"));
            await Output.WriteLineAsync(file.FullName);
            if (file.Exists)
            {
                return Cli.SuccessExitCode;
            }

            var template = Scriban.Template.ParseLiquid(await Note.GetTemplate(CommandConfig.HomeDirectory.FullName, categoryConfig));
            await File.WriteAllTextAsync(
                file.FullName,
                (await template.RenderAsync(new { Title = meta.Title, Category = meta.Category, Created = meta.Created, TargetDate = targetDate }))
            );
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