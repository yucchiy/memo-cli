using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace Memo
{
    public class CategoryCollector
    {
        private CategoryConfigFinder ConfigFinder { get; }
        private CommandConfig Config { get; }

        public CategoryCollector(CategoryConfigFinder configFinder, CommandConfig config)
        {
            ConfigFinder = configFinder;
            Config = config;
        }

        public Category[] Collect(DirectoryInfo rootDirectory)
        {
            var categories = new List<Category>();
            CollectCategories(Config.HomeDirectory, rootDirectory, categories, null);

            return categories.ToArray();
        }

        public Category Find(string categoryName)
        {
            return Collect(Config.HomeDirectory)
                .Where(category => category.Name == categoryName)
                .FirstOrDefault();
        }

        private void CollectCategories(DirectoryInfo rootDirectory, DirectoryInfo directory, List<Category> categories, Category parentCategory = null)
        {
            if (directory.GetFiles("*.md").Length > 0 ||
                directory.GetFiles("*.markdown") .Length > 0 ||
                directory.GetDirectories().Length > 0)
            {
                var categoryName = Path.GetRelativePath(Config.HomeDirectory.FullName, directory.FullName);
                var category = new Category(
                    categoryName,
                    new DirectoryInfo(directory.FullName),
                    ConfigFinder.FindOrDefault(categoryName),
                    parentCategory
                );

                categories.Add(category);
                foreach (var subDirectory in directory.GetDirectories())
                {
                    CollectCategories(rootDirectory, subDirectory, categories, category);
                }
            }
        }
    }
}

