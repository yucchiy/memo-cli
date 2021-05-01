using System.Threading;
using System.Threading.Tasks;
using System.IO;
using System.Collections.Generic;

namespace Memo
{
    public class MemoManager : IMemoManager
    {
        private NoteCollector NoteCollector { get; }
        private NoteCreator NoteCreator { get; }
        private NoteCreationParameterBuilder NoteCreationParameterBuilder { get; }
        private CategoryConfigFinder CategoryConfigFinder { get; }
        private CategoryCreator CategoryCreator { get; }
        private CategoryCollector CategoryCollector { get; }
        private Configuration Config { get; }

        public MemoManager()
        {
            // TODO: Fix
            var commandConfig = new CommandConfig();
            CategoryConfigFinder = new CategoryConfigFinder(commandConfig);
            CategoryCollector = new CategoryCollector(CategoryConfigFinder, commandConfig);
            CategoryCreator = new CategoryCreator(CategoryCollector, CategoryConfigFinder, commandConfig);
            NoteCollector = new NoteCollector(CategoryCollector);
            NoteCreator = new NoteCreator(NoteCollector, CategoryCreator, CategoryCollector, commandConfig);
            NoteCreationParameterBuilder = new NoteCreationParameterBuilder();
            Config = new Configuration()
            {
                HomeDirectory = commandConfig.HomeDirectory
            };
        }

        public Category CreateCategory(string categoryName) => CategoryCreator.Create(categoryName);

        public Category GetCategory(string categoryName) => CategoryCollector.Find(categoryName);

        public Category[] GetCategories() => CategoryCollector.Collect(Config.HomeDirectory);

        public async Task<Note[]> GetNotesAsync(Category category, IEnumerable<string> queries) => await NoteCollector.Collect(category, queries);

        public async Task<Note> CreateNoteAsync(NoteCreationParameter parameter, CancellationToken token) => await NoteCreator.CreateNoteAsync(parameter, token);
        public async Task<NoteCreationParameter> CreateNoteCreationParameter(string category, string id, IEnumerable<string> options, CancellationToken token)
        {
            return await NoteCreationParameterBuilder.Build(category, id, options, token);
        }

        public class Configuration
        {
            public DirectoryInfo HomeDirectory;
        }
    }
}