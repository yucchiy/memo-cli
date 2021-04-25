using System.IO;

namespace Memo
{
    public class CategoryCreator
    {
        private CategoryCollector Collector { get; }
        private CategoryConfigFinder ConfigFinder { get; }
        private CommandConfig Config { get; }

        public CategoryCreator(CategoryCollector collector, CategoryConfigFinder configFinder, CommandConfig config)
        {
            Collector = collector;
            ConfigFinder = configFinder;
            Config = config;
        }

        public Category Create(string categoryName)
        {
            var category = Collector.Find(categoryName);
            if (category != null) return category;

            var path = Utility.CategoryName2CategoryAbsoluteDirectoryPath(Config.HomeDirectory, categoryName);
            var config = ConfigFinder.FindOrDefault(categoryName);
            return Directory.Exists(path) ?
               new Category(categoryName, new DirectoryInfo(path), config, null) :
               new Category(categoryName, Directory.CreateDirectory(path), config, null);
        }
    }
}
