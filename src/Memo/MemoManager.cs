using System.Threading.Tasks;
using System.IO;

namespace Memo
{
    public class MemoManager
    {
        private NoteCollector Note { get; }
        private CategoryCollector Category { get; }
        private Configuration Config { get; }

        public MemoManager(Configuration config)
        {
            Note = new NoteCollector();
            Category = new CategoryCollector();
            Config = config;
        }

        public Category[] GetCategories()
        {
            return Category.Collect(Config.HomeDirectory);
        }

        public async Task<Note[]> GetNotes(Category category, string type)
        {
            return await Note.Collect(category, type);
        }

        public class Configuration
        {
            public DirectoryInfo HomeDirectory;
        }
    }
}