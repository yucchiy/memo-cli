using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace Memo
{
    public class CategoryConfigFinder
    {
        private CommandConfig Config { get; }

        public CategoryConfigFinder(CommandConfig config)
        {
            Config = config;
        }

        public MemoConfig.CategoryConfig FindOrDefault(string categoryName)
        {
            foreach (var categoryConfig in Config.MemoConfig.Categories)
            {
                if (categoryConfig.Name == categoryName)
                {
                    var category = MemoConfig.GetDefault(categoryName, categoryConfig.MemoCreationType);
                    if (!string.IsNullOrEmpty(categoryConfig.MemoFileNameFormat))
                    {
                        category.MemoFileNameFormat = categoryConfig.MemoFileNameFormat;
                    }

                    if (!string.IsNullOrEmpty(categoryConfig.MemoTitleFormat))
                    {
                        category.MemoTitleFormat = categoryConfig.MemoTitleFormat;
                    }

                    category.MemoTemplateFilePath = categoryConfig.MemoTemplateFilePath;
                    return category;
                }
            }

            return MemoConfig.GetDefault(categoryName, CreationType.Default);
        }
    }
}

