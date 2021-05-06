#nullable enable
using System;
using System.Collections.Generic;

namespace Memo.Core.Notes
{
    public class NoteCreationParameterBuilder
    {
        public NoteCreationParameterBuilder()
        {
        }

        public NoteCreationParameterBuilder(in NoteCreationParameter parameter)
        {
            if (parameter.CategoryId is Categories.CategoryId categoryId)
            {
                CategoryName = categoryId;
            }

            if (parameter.Options.Slug is Note.NoteSlug slug)
            {
                Slug = slug;
            }

            if (parameter.Options.Title is Note.NoteTitle title)
            {
                Title = title;
            } 

             if (parameter.Options.Type is Note.NoteType type)
            {
                Type = type;
            } 

             if (parameter.Options.Timestamp is DateTime timestamp)
            {
                Timestamp = timestamp;
            }
 
            if (parameter.Options.CreationType is NoteCreationOptionParameter.NoteCreationType noteCreationType)
            {
                CreationType = noteCreationType;
            }

            Url = parameter.Options.Url;
        }

        private Categories.CategoryId? CategoryName { get; set; } = default;
        private Note.NoteSlug? Slug { get; set; } = null; 
        private Note.NoteTitle? Title { get; set; } = null;
        private Note.NoteType? Type { get; set; } = null;
        private DateTime? Timestamp { get; set; } = null;
        private NoteCreationOptionParameter.NoteCreationType? CreationType { get; set; } = null;
        private string? Url { get; set; } = null;

        public NoteCreationParameterBuilder WithCategoryId(string categoryId)
        {
            return WithCategoryId(new Categories.CategoryId(categoryId));
        }

        public NoteCreationParameterBuilder WithCategoryId(Categories.CategoryId categoryId)
        {
            CategoryName = categoryId;
            return this;
        }

        public NoteCreationParameterBuilder WithSlug(string slug)
        {
            return WithSlug(new Note.NoteSlug(slug));
        }

        public NoteCreationParameterBuilder WithSlug(Note.NoteSlug slug)
        {
            Slug = slug;
            return this;
        }

        public NoteCreationParameterBuilder WithTitle(string title)
        {
            return WithTitle(new Note.NoteTitle(title));
        }

        public NoteCreationParameterBuilder WithTitle(Note.NoteTitle title)
        {
            Title = title;
            return this;
        }

        public NoteCreationParameterBuilder WithType(string type)
        {
            return WithType(new Note.NoteType(type));
        }

        public NoteCreationParameterBuilder WithType(Note.NoteType type)
        {
            Type = type;
            return this;
        }

        public NoteCreationParameterBuilder WithTimestamp(DateTime timestamp)
        {
            Timestamp = timestamp;
            return this;
        }

        public NoteCreationParameterBuilder WithCreationType(NoteCreationOptionParameter.NoteCreationType creationType)
        {
            CreationType = creationType;
            return this;
        }

        public NoteCreationParameterBuilder WithUrl(string url)
        {
            Url = url;
            return this;
        }

        public NoteCreationParameterBuilder WithQueryStrings(IEnumerable<string> optionStrings)
        {
            foreach (var option in optionStrings)
            {
                if (TryParseQuery(option, out var result))
                {
                    switch (result.Key)
                    {
                        case "creationType":
                            WithCreationType(System.Enum.Parse<NoteCreationOptionParameter.NoteCreationType>(result.Value));
                            break;
                        case "type":
                            WithType(result.Value);
                            break;
                        case "url":
                            WithUrl(result.Value);
                            break;
                        case "title":
                            WithTitle(result.Value);
                            break;
                        case "slug":
                            WithSlug(result.Value);
                            break;
                    }
                }
            }

            return this;
        }

        private bool TryParseQuery(string option, out KeyValuePair<string, string> result)
        {
            var indexOf = option.IndexOf(":");
            if (indexOf > 0)
            {
                result = new KeyValuePair<string, string>(option.Substring(0, indexOf), option.Substring(indexOf + 1));
                return true;
            }

            result = default;
            return false;
        }

        public NoteCreationParameter Build()
        {
            if (CategoryName is Categories.CategoryId categoryName)
            {
                return new NoteCreationParameter(categoryName, new NoteCreationOptionParameter(Slug, Title, Type, Timestamp, CreationType, Url));
            }

            throw new System.InvalidOperationException("CategoryName should specificate.");
        }
    }
}