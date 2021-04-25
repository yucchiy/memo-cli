using System;
using System.IO;
using System.Threading.Tasks;
using System.Threading;
using System.Net;
using System.Net.Http;

namespace Memo
{
    public class NoteCreator
    {
        private CommandConfig Config { get; }
        private NoteCollector NoteCollector { get; }
        private CategoryCreator CategoryCreator { get; }
        private CategoryCollector CategoryCollector { get; }

        public NoteCreator(NoteCollector noteCollector, CategoryCreator categoryCreator, CategoryCollector categoryCollector, CommandConfig config)
        {
            NoteCollector = noteCollector;
            CategoryCreator = categoryCreator;
            CategoryCollector = categoryCollector;
            Config = config;
        }

        public async Task<Note> CreateNoteAsync(NoteCreationParameter parameter, CancellationToken token)
        {
            parameter = await NormalizeParameterAsync(parameter, token);
            var category = CategoryCreator.Create(parameter.Category);
            switch (category.CategoryConfig.MemoCreationType)
            {
                case CreationType.Default:
                    return await InternalCreateNoteAsync(category, parameter, DateTime.Now, token);
                case CreationType.Daily:
                    return await InternalCreateNoteAsync(category, parameter, DateTime.Now, token);
                case CreationType.Weekly:
                    return await InternalCreateNoteAsync(category, parameter, Utility.FirstDayOfWeek(), token);
            }

            throw new MemoCliException("MemoCreationType not found.");
        }

        private async Task<NoteCreationParameter> NormalizeParameterAsync(NoteCreationParameter input, CancellationToken token)
        {
            if (!string.IsNullOrEmpty(input.Url))
            {
                var uri = new Uri(input.Url);
                var filename = (uri.Host + "-" + uri.LocalPath.Trim('/')).Replace('/', '_');
                using (var client = new HttpClient())
                {
                    var response = await client.GetAsync(input.Url, token);
                    return response.StatusCode switch
                    {
                        HttpStatusCode.OK => new NoteCreationParameter()
                        {
                            Category = input.Category,
                            Title = Utility.TryParseTitle(await response.Content.ReadAsStringAsync(), out var title) ? title : filename,
                            Filename = filename,
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

        private async Task<Note> InternalCreateNoteAsync(Category category, NoteCreationParameter parameter, DateTime targetDate, CancellationToken token)
        {
            var directoryName = Utility.Format(
                category.CategoryConfig.MemoDirectoryNameFormat,
                new { InputTitle = parameter.Title, InputFilename = parameter.Filename, Category = category.Name, Created = DateTime.Now, TargetDate = targetDate }
            );

            var directory = new DirectoryInfo(Path.Combine(Config.HomeDirectory.FullName, category.CategoryConfig.Name, directoryName));
            if (!Directory.Exists(directory.FullName))
            {
                Directory.CreateDirectory(directory.FullName);
            }

            var file = new FileInfo(Path.Combine(directory.FullName, $"index.markdown"));
            var note = await NoteCollector.Find(file.FullName, category, string.Empty);
            if (note != null)
            {
                return note;
            }

            var template = Scriban.Template.ParseLiquid(await Note.GetTemplate(Config.HomeDirectory.FullName, category.CategoryConfig));
            var meta = new NoteMetaData()
            {
                Title = Utility.Format(
                    category.CategoryConfig.MemoTitleFormat,
                    new { InputTitle = parameter.Title, InputFilename = parameter.Filename, Category = category.Name, Created = DateTime.Now, TargetDate = targetDate }
                ),
                Category = category.Name,
                Created = DateTime.Now,
                Url = string.IsNullOrEmpty(parameter.Url) ? string.Empty : parameter.Url,
            };
            await File.WriteAllTextAsync(
                file.FullName,
                (await template.RenderAsync(new { Title = meta.Title, Category = meta.Category, Created = meta.Created, TargetDate = targetDate, Url = meta.Url })),
                token
            );

            return await NoteCollector.Find(file.FullName, category, string.Empty);
        }
    }
}