namespace Memo.Core.Notes
{
    public interface INoteQueryFilter
    {
        bool FilterNote(in Note note, in NoteSearchQuery query);
    }
}