using System;
using System.Collections.Generic;

namespace Memo.Core.Notes
{
    public readonly struct Note
    {
        public readonly struct NoteId : IEquatable<NoteId>
        {
            public string Value { get; }

            public NoteId(string value)
            {
                Value = value;
            }

            public bool Equals(NoteId other) => Value.Equals(other.Value);
            public override bool Equals(object obj) => obj is NoteId other && Equals(other);
            public override int GetHashCode() => Value.GetHashCode();
            public override string ToString() => $"Note.Id {Value.ToString()}";
        }

        public readonly struct NoteSlug : IEquatable<NoteSlug>
        {
            public string Value { get; }

            public NoteSlug(string value)
            {
                Value = value;
            }

            public bool Equals(NoteSlug other) => Value.Equals(other.Value);
            public override bool Equals(object obj) => obj is NoteSlug other && Equals(other);
            public override int GetHashCode() => Value.GetHashCode();
            public override string ToString() => $"NoteSlug {Value.ToString()}";
        }

        public readonly struct NoteTitle : IEquatable<NoteTitle>
        {
            public string Value { get; }

            public NoteTitle(string value)
            {
                Value = value;
            }

            public bool Equals(NoteTitle other) => Value.Equals(other.Value);
            public override bool Equals(object obj) => obj is NoteTitle other && Equals(other);
            public override int GetHashCode() => Value.GetHashCode();
            public override string ToString() => $"NoteTitle {Value.ToString()}";
        }

        public readonly struct NoteType : IEquatable<NoteType>
        {
            public string Value { get; }

            public NoteType(string value)
            {
                Value = value;
            }

            public bool Equals(NoteType other) => Value.Equals(other.Value);
            public override bool Equals(object obj) => obj is NoteType other && Equals(other);
            public override int GetHashCode() => Value.GetHashCode();
            public override string ToString() => $"NoteType {Value.ToString()}";
        }

        public Categories.Category Category { get; }
        public NoteId Id { get; }
        public NoteTitle Title { get; }
        public NoteType? Type { get; }
        public DateTime? Created { get; }
        public IEnumerable<string> Links { get; }
        public IEnumerable<(Categories.CategoryId CategoryId, Note.NoteId NoteId)> InternalLinks { get; }

        public string RelativePath { get => $"{Category.Id.Value}/{Id.Value}/{Id.Value}.markdown"; }

        public Note(Categories.Category category, NoteId id, NoteTitle title, NoteType? type, DateTime? created, IEnumerable<string> links, IEnumerable<(Categories.CategoryId CategoryId, Note.NoteId NoteId)> internalLinks)
        {
            Category = category;
            Id = id;
            Title = title;
            Type = type;
            Created = created;
            Links = links;
            InternalLinks = internalLinks;
        }

        public override bool Equals(object obj)
        {
            return obj is Note note &&
                   EqualityComparer<Categories.Category>.Default.Equals(Category, note.Category) &&
                   Id.Equals(note.Id);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Category, Id, Title, Type);
        }
    }
}