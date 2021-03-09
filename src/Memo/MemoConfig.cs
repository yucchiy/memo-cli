using System.Runtime.Serialization;
using System;

namespace Memo
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
            [DataMember(Name = "memo_filename_format")]
            public string MemoFileNameFormat = string.Empty;
            [DataMember(Name = "memo_title_format")]
            public string MemoTitleFormat = string.Empty;
        }

        public static CategoryConfig GetDefault(string categoryName, CreationType creationType)
        {
            switch (creationType)
            {
                case CreationType.Default:
                    return new CategoryConfig()
                    {
                        Name = categoryName,
                        MemoCreationType = CreationType.Default,
                        MemoFileNameFormat = "{{ target_date | date.to_string '%Y%m%d' }}_{{ input_filename }}",
                        MemoTitleFormat = "{{ input_title }} - {{ category }}",
                    };
                case CreationType.Daily:
                    return new CategoryConfig()
                    {
                        Name = categoryName,
                        MemoCreationType = CreationType.Daily,
                        MemoFileNameFormat = "{{ target_date | date.to_string '%Y%m%d' }}_{{ category }}",
                        MemoTitleFormat = "{{ category }} - {{ target_date | date.to_string '%Y/%m/%d' }}",
                    };
                case CreationType.Weekly:
                    return new CategoryConfig()
                    {
                        Name = categoryName,
                        MemoCreationType = CreationType.Weekly,
                        MemoFileNameFormat = "{{ target_date | date.to_string '%Y%m%d' }}_{{ category }}",
                        MemoTitleFormat = "{{ category }} - {{ target_date | date.to_string '%Y/%m/%d' }}",
                    };
                default:
                    throw new MemoCliException($"CreationType {creationType} is not support.");
            }
        }
    }
}