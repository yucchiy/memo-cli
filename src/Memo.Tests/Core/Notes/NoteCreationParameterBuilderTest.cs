using System;
using Xunit;

namespace Memo.Core.Notes.Tests
{
    public class NoteCreationParameterBuilderTest
    {
        [Fact]
        public void Build_CategoryName_ShouldBeSpecified()
        {
            var builder = new NoteCreationParameterBuilder();
            var exception = Assert.Throws<InvalidOperationException>(() => builder.Build());
        }

        [Fact]
        public void Build_CategoryName_Set()
        {
            var builder = new NoteCreationParameterBuilder();
            var categoryName = new Categories.CategoryId("TestCategory1");

            var parameter = builder
                .WithCategoryId(categoryName)
                .Build();

            Assert.Equal(parameter.CategoryId, categoryName);
        }

        [Fact]
        public void Build_CheckProperties_IsNull()
        {
            var builder = new NoteCreationParameterBuilder();

            var parameter = builder
                .WithCategoryId("TestCategory")
                .Build();

            Assert.Null(parameter.Options.Slug);
            Assert.Null(parameter.Options.Title);
            Assert.Null(parameter.Options.Type);
            Assert.False(parameter.Options.Timestamp.HasValue);
        }

        [Fact]
        public void Build_CheckProperties_IsNotNull()
        {
            var builder = new NoteCreationParameterBuilder();

            var now = DateTime.Now;
            var slug = new Note.NoteSlug("TestSlug");
            var title = new Note.NoteTitle("TestTitle");
            var type = new Note.NoteType("TestType");

            var parameter = builder
                .WithCategoryId("TestCategory")
                .WithSlug(slug.Value)
                .WithTitle(title.Value)
                .WithType(type.Value)
                .WithTimestamp(now)
                .Build();

            Assert.NotNull(parameter.Options.Slug);
            Assert.NotNull(parameter.Options.Title);
            Assert.NotNull(parameter.Options.Type);
            Assert.True(parameter.Options.Timestamp.HasValue);            

            Assert.Equal(parameter.Options.Slug, slug);
            Assert.Equal(parameter.Options.Title, title);
            Assert.Equal(parameter.Options.Type, type);
            Assert.Equal(parameter.Options.Timestamp.Value, now);
        }
    }
}
