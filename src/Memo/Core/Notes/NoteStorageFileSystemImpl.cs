using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.IO;

namespace Memo.Core.Notes
{
    public class NoteStorageFileSystemImpl : INoteStorage
    {
        private INoteSerializer Serializer { get; }
        private Options Option { get; }

        public NoteStorageFileSystemImpl(INoteSerializer serializer, Options option)
        {
            Serializer = serializer;
            Option = option;
        }

        public class Options
        {
            public DirectoryInfo RootDirectory { get; }

            public Options(DirectoryInfo rootDirectory)
            {
                RootDirectory = rootDirectory;
            }
        }

        public async Task<IEnumerable<Note>> ReadAllAsync(CancellationToken token)
        {
            return await CollectNotesFromDirectory(Option.RootDirectory, token);
        }

        public async Task<bool> WriteAsync(Note note, CancellationToken token)
        {
            var result = await Serializer.SerializeNoteAsync(note, token);
            if (!result.Success)
            {
                return false;
            }

            var directoryInfo = new DirectoryInfo($"{Option.RootDirectory.FullName}/{note.Category.Id.Value}/{note.Id.Value}");
            if (!directoryInfo.Exists)
            {
                Directory.CreateDirectory(directoryInfo.FullName);
            }

            var fileInfo = new FileInfo($"{directoryInfo.FullName}/index.markdown");
            await File.WriteAllTextAsync(fileInfo.FullName, result.RawContent, token);

            return true;
        }

        private async Task<IEnumerable<Note>> CollectNotesFromDirectory(DirectoryInfo directory, CancellationToken token)
        {
            var result = new List<Note>();
            foreach (var file in directory.GetFiles())
            {
                var deserializedResult = await Serializer.DeserializeNoteAsync(file, token);
                if (deserializedResult.Success)
                {
                    result.Add(deserializedResult.Note);
                }
            }

            foreach (var subDirectory in directory.GetDirectories())
            {
                result.AddRange(await CollectNotesFromDirectory(subDirectory, token));
            }

            return result;
        }
    }
}