#nullable enable

namespace Memo.Core.Notes
{
    public readonly struct NoteSearchQuery
    {
        public readonly Categories.CategoryId? CategoryId { get; }

        public readonly Note.NoteId? Id { get; }

        public readonly Note.NoteType? Type { get; }

        public NoteSearchQuery(Categories.CategoryId? categoryName, Note.NoteId? id, Note.NoteType? type)
        {
            CategoryId = categoryName;
            Id = id;
            Type = type;
        }
    }
}