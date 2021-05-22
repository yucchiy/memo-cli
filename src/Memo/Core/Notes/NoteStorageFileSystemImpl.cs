using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using System.Linq;

namespace Memo.Core.Notes
{
    public class NoteStorageFileSystemImpl : INoteStorage
    {
        private INoteSerializer Serializer { get; }
        private Options Option { get; }

        public NoteStorageFileSystemImpl(INoteSerializer serializer, CommandConfig commandConfig)
        {
            Serializer = serializer;
            Option = new Options(commandConfig.HomeDirectory, commandConfig.DirectorySeparator);
        }

        private class Options
        {
            public DirectoryInfo RootDirectory { get; }
            public char NoteDirectorySeparator { get; }

            public Options(DirectoryInfo rootDirectory, char noteDirectorySeparator = '/')
            {
                RootDirectory = rootDirectory;
                NoteDirectorySeparator = noteDirectorySeparator;
            }
        }

        public async Task<IEnumerable<Note>> ReadAllAsync(CancellationToken token)
        {
            var notes = await CollectNotesFromDirectory(Option.RootDirectory, token);

            return notes
                .OrderByDescending(note => note.Timestamp.Value);
        }

        public async Task<bool> WriteAsync(Note note, CancellationToken token)
        {
            var result = await Serializer.SerializeNoteAsync(note, token);
            if (!result.Success)
            {
                return false;
            }

            var filePath = Path.Combine(Option.RootDirectory.FullName, note.RelativePath.Replace(Option.NoteDirectorySeparator, Path.DirectorySeparatorChar));
            var fileInfo = new FileInfo(filePath);
            if (!fileInfo.Directory.Exists)
            {
                Directory.CreateDirectory(fileInfo.Directory.FullName);
            }

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