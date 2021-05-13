using System.Collections.Generic;

namespace Memo.Core.Categories
{
    public class CategoryConfigStore : ICategoryConfigStore
    {
        private IEnumerable<MemoConfig.CategoryConfig> CategoryConfigs;

        public CategoryConfigStore(IEnumerable<MemoConfig.CategoryConfig> categoryConfigs)
        {
            CategoryConfigs = categoryConfigs;
        }

        public MemoConfig.CategoryConfig GetConfig(CategoryId categoryId)
        {
            foreach (var config in CategoryConfigs)
            {
                if (config.Name == categoryId.Value)
                {
                    return config;
                }
            }

            return MemoConfig.GetDefault(categoryId.Value, CreationType.Default);
        }
    }
}