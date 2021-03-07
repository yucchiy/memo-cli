using System.IO;
using System.Collections.Generic;

namespace Memo
{
    public class CategoryCollector
    {
        private DirectoryInfo RootDirectory { get; }

        public CategoryCollector()
        {
        }

        public Category[] Collect(DirectoryInfo rootDirectory)
        {
            var categories = new List<Category>();
            foreach (var directory in rootDirectory.GetDirectories())
            {
                CollectCategories(rootDirectory, directory, categories);
            }

            return categories.ToArray();
        }

        private void CollectCategories(DirectoryInfo rootDirectory, DirectoryInfo directory, List<Category> categories)
        {
            if (directory.GetFiles("*.md").Length > 0 || directory.GetFiles("*.markdown") .Length > 0)
            {
                categories.Add(new Category(
                    Path.GetRelativePath(rootDirectory.FullName, directory.FullName),
                    new DirectoryInfo(directory.FullName)
                ));
            }

            foreach (var subDirectory in directory.GetDirectories())
            {
                CollectCategories(rootDirectory, subDirectory, categories);
            }
        }
    }
}

