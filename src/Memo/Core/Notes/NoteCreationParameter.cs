namespace Memo.Core.Notes
{
    public readonly struct NoteCreationParameter
    {
        public readonly Categories.CategoryId CategoryId { get; }
        public readonly NoteCreationOptionParameter Options { get; }

        public NoteCreationParameter(Categories.CategoryId categoryName, NoteCreationOptionParameter options)
        {
            CategoryId = categoryName;
            Options = options;
        }
    }
}