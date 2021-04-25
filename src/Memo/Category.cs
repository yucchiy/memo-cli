using System.IO;

namespace Memo
{
    public class Category
    {
        public string Name { get; }
        public DirectoryInfo Path { get; }
        public MemoConfig.CategoryConfig CategoryConfig { get; }
        public Category ParentCategory { get; }

        public Category(string name, DirectoryInfo path, MemoConfig.CategoryConfig categoryConfig, Category parentCategory)
        {
            Name = name;
            Path = path;
            CategoryConfig = categoryConfig;
            ParentCategory = parentCategory;
        }
    }
}