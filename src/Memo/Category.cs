using System.IO;

namespace Memo
{
    public class Category
    {
        public string Name { get; }
        public DirectoryInfo Path { get; }

        public Category(string name, DirectoryInfo path)
        {
            Name = name;
            Path = path;
        }
    }
}