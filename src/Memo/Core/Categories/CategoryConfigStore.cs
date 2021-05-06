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

        public MemoConfig.CategoryConfig GetConfig(Category category)
        {
            foreach (var config in CategoryConfigs)
            {
                if (config.Name == category.Id.Value)
                {
                    return config;
                }
            }

            return MemoConfig.GetDefault(category.Id.Value, CreationType.Default);
        }
    }
}