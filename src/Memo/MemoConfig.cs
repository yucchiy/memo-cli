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
            public string MemoFileNameFormat = "{{ target_date | date.to_string '%y%m%d' }}_{{ input_filename }}";
            [DataMember(Name = "memo_title_format")]
            public string MemoTitleFormat = "{{ input_title }} - {{ category }}";
        }
    }
}