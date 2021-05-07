using Xunit;
using Memo.Tests;
using System.Threading;
using System.Threading.Tasks;
using System.IO;

namespace Memo.Core.Notes.Tests
{
    public class NoteStorageTest
    {
        [Fact]
        public async void ReadAsyncAll_Test()
        {
            using (var rootDirectory = new TemporaryDirectory($"{typeof(NoteStorageTest).FullName}_ReadAsyncAll_Test-"))
            {
                var storage = new NoteStorageFileSystemImpl(new NoteParser(new NoteBuilder(), new Categories.CategoryConfigStore(new MemoConfig.CategoryConfig[0]), new NoteParser.Options(rootDirectory.Dir, '/')), new NoteStorageFileSystemImpl.Options(rootDirectory.Dir));

                var content = @"---
title: TestTitle
---

[this is a test1](cat1/cat2/note1/index.markdown) [this is a test2](cat1/cat2/note2/index.markdown) [this is a test3](cat1/cat2/note3/index.markdown)
[this is a test4](cat1/cat2/note4/index.markdown)

[this is a test4](https://localhost.markdown)
";
                Directory.CreateDirectory($"{rootDirectory.Dir.FullName}/cat1/cat2/note1");
                File.WriteAllText($"{rootDirectory.Dir.FullName}/cat1/cat2/note1/index.markdown", content);

                var notes = await storage.ReadAllAsync(CancellationToken.None);
            }
        }

        [Fact]
        public async void ReadAsyncAll_RealWorldTest_Dotnote()
        {
            var rootDirectory = new System.IO.DirectoryInfo("/Users/yucchiy/.ghq/github.com/yucchiy/dotnote");
            var storage = new NoteStorageFileSystemImpl(new NoteParser(new NoteBuilder(), new Categories.CategoryConfigStore(new MemoConfig.CategoryConfig[0]), new NoteParser.Options(rootDirectory, '/')), new NoteStorageFileSystemImpl.Options(rootDirectory));

            var notes = await storage.ReadAllAsync(CancellationToken.None);
        }
    }
}