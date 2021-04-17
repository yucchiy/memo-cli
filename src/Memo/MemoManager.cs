using System.Threading;
using System.Threading.Tasks;
using System.IO;

namespace Memo
{
    public class MemoManager : IMemoManager
    {
        private NoteCollector NoteCollector { get; }
        private NoteCreator NoteCreator { get; }
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
            NoteCollector = new NoteCollector();
            NoteCreator = new NoteCreator(NoteCollector, CategoryCreator, CategoryCollector, commandConfig);
            Config = new Configuration()
            {
                HomeDirectory = commandConfig.HomeDirectory
            };
        }

        public Category CreateCategory(string categoryName) => CategoryCreator.Create(categoryName);

        public Category GetCategory(string categoryName) => CategoryCollector.Find(categoryName);

        public Category[] GetCategories() => CategoryCollector.Collect(Config.HomeDirectory);

        public async Task<Note[]> GetNotesAsync(Category category, string type) => await NoteCollector.Collect(category, type);

        public async Task<Note> CreateNoteAsync(NoteCreationParameter input, CancellationToken token) => await NoteCreator.CreateNoteAsync(input, token);

        public class Configuration
        {
            public DirectoryInfo HomeDirectory;
        }
    }
}