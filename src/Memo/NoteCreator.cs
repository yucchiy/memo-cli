using System;
using System.IO;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Generic;

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

        public async Task<Note> CreateNoteAsync(NoteCreateParameter parameter, CancellationToken token)
        {
            var category = CategoryCreator.Create(parameter.Category);
            return await InternalCreateNoteAsync(category, parameter, DateTime.Now, token);
        }

        private async Task<Note> InternalCreateNoteAsync(Category category, NoteCreateParameter parameter, DateTime targetDate, CancellationToken token)
        {
            var directory = new DirectoryInfo(Path.Combine(Config.HomeDirectory.FullName, category.CategoryConfig.Name, parameter.Id));
            if (!Directory.Exists(directory.FullName))
            {
                Directory.CreateDirectory(directory.FullName);
            }

            var file = new FileInfo(Path.Combine(directory.FullName, $"index.markdown"));
            var note = await NoteCollector.Find(category, parameter.Id);
            if (note != null)
            {
                return note;
            }

            var template = Scriban.Template.ParseLiquid(await Note.GetTemplate(Config.HomeDirectory.FullName, category.CategoryConfig));
            var options = new Dictionary<string, string>(parameter.Options);
            options.Add("category", category.Name);
            options.Add("created", System.DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss"));

            await File.WriteAllTextAsync(
                file.FullName,
                (await template.RenderAsync(options)),
                token
            );

            return await NoteCollector.Find(category, parameter.Id);
        }
    }
}