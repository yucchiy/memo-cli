using Xunit;
using Memo.Tests;
using System.Threading;
using System.Threading.Tasks;

namespace Memo.Core.Notes.Tests
{
    public class NoteStorageTest
    {
        [Fact]
        public void ReadAsyncAll_Test()
        {
            using (var rootDirectory = new TemporaryDirectory($"{typeof(NoteStorageTest).FullName}_ReadAsyncAll_Test-"))
            {
                var storage = new NoteStorageFileSystemImpl(
                    null,
                    new NoteStorageFileSystemImpl.Options(rootDirectory.Dir)
                );
            }
        }

        // [Fact]
        // public async void ReadAsyncAll_RealWorldTest_Dotnote()
        // {
        //     var rootDirectory = new System.IO.DirectoryInfo("/Users/yucchiy/.ghq/github.com/yucchiy/dotnote");
        //     var storage = new NoteStorageFileSystemImpl(new NoteParser(new NoteParser.Options(rootDirectory, '/')), new NoteStorageFileSystemImpl.Options(rootDirectory));

        //     var notes = await storage.ReadAllAsync(CancellationToken.None);
        // }
    }
}