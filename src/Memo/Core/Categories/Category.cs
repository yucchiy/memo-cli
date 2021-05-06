using System;

namespace Memo.Core.Categories
{
    public readonly struct Category
    {
        public readonly CategoryId Id { get; }

        public Category(CategoryId id)
        {
            Id = id;
        }
    }

    public readonly struct CategoryId : IEquatable<CategoryId>
    {
        public string Value { get; }

        public CategoryId(string value)
        {
            Value = value;
        }

        public bool Equals(CategoryId other) => Value.Equals(other.Value);
        public override bool Equals(object obj) => obj is CategoryId other && Equals(other);
        public override int GetHashCode() => Value.GetHashCode();
        public override string ToString() => $"CategoryId {Value.ToString()}";
    }
}