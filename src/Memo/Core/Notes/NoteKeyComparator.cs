using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Memo.Core.Notes
{
    public struct NoteKeyComparator : IEqualityComparer<Note>
    {
        public bool Equals(Note x, Note y)
        {
            return EqualityComparer<Categories.Category>.Default.Equals(x.Category, y.Category)
                && EqualityComparer<Note.NoteId>.Default.Equals(x.Id, y.Id);
        }

        public int GetHashCode([DisallowNull] Note note)
        {
            return HashCode.Combine(note.Category, note.Id);
        }
    }
}