#nullable enable
using System.Collections.Generic;

namespace Memo.Core.Notes
{
    public class NoteSearchQueryBuilder
    {
        private Categories.CategoryId? CategoryId { get; set; }
        private Note.NoteId? Id { get; set; }
        private Note.NoteType? Type { get; set; }

        public NoteSearchQueryBuilder WithCategoryId(Categories.CategoryId categoryId)
        {
            CategoryId = categoryId;
            return this;
        }

        public NoteSearchQueryBuilder WithCategoryId(string categoryId)
        {
            return WithCategoryId(new Categories.CategoryId(categoryId));
        }

        public NoteSearchQueryBuilder WithId(Note.NoteId id)
        {
            Id = id;
            return this;
        }

        public NoteSearchQueryBuilder WithId(string id)
        {
            return WithId(new Note.NoteId(id));
        }

        public NoteSearchQueryBuilder WithType(Note.NoteType type)
        {
            Type = type;
            return this;
        }

        public NoteSearchQueryBuilder WithType(string type)
        {
            return WithType(new Note.NoteType(type));
        }

        public NoteSearchQueryBuilder WithQueryStrings(IEnumerable<string> queries)
        {
            foreach (var query in queries)
            {
                if (TryParseQuery(query, out var result))
                {
                    switch (result.Key)
                    {
                        case "type":
                            WithType(result.Value);
                            break;
                        case "id":
                            WithId(result.Value);
                            break;
                        case "slug":
                            break;
                        case "timestamp":
                            break;
                    }
                }
            }

            return this;
        }

        private bool TryParseQuery(string query, out KeyValuePair<string, string> result)
        {
            var indexOf = query.IndexOf(":");
            if (indexOf > 0)
            {
                result = new KeyValuePair<string, string>(query.Substring(0, indexOf), query.Substring(indexOf + 1));
                return true;
            }

            result = default;
            return false;
        }


        public NoteSearchQuery Build()
        {
            return new NoteSearchQuery(CategoryId, Id, Type);
        }
    }
}