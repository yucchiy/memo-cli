using System.Runtime.Serialization;

namespace Memo.Core
{
    public class MemoConfig
    {
        [DataMember(Name = "categories")]
        public CategoryConfig[] Categories;

        public MemoConfig()
        {
            Categories = new CategoryConfig[0];
        }

        public class CategoryConfig
        {
            [DataMember(Name = "name")]
            public string Name;
            [DataMember(Name = "memo_creation_type")]
            public CreationType MemoCreationType = CreationType.Default;
            [DataMember(Name = "memo_template_path")]
            public string MemoTemplateFilePath = string.Empty;
        }

        public static CategoryConfig GetDefault(string categoryName, CreationType creationType)
        {
            switch (creationType)
            {
                case CreationType.Default:
                    return new CategoryConfig()
                    {
                        Name = categoryName,
                    };
                case CreationType.Daily:
                    return new CategoryConfig()
                    {
                        Name = categoryName,
                    };
                case CreationType.Weekly:
                    return new CategoryConfig()
                    {
                        Name = categoryName,
                    };
                default:
                    throw new MemoCliException($"CreationType {creationType} is not support.");
            }
        }
    }
}