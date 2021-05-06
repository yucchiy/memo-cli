namespace Memo.Core.Categories
{
    public interface ICategoryConfigStore
    {
        MemoConfig.CategoryConfig GetConfig(Categories.Category category);
    }
}