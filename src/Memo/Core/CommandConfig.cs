using System;
using System.IO;
using Utf8Json;

namespace Memo.Core
{
    public class CommandConfig
    {
        public DirectoryInfo HomeDirectory { get; }
        public string GitPath { get; }
        public MemoConfig MemoConfig { get; }
        public char DirectorySeparator { get; }

        public CommandConfig()
        {
            if (Environment.GetEnvironmentVariable("MEMO_CLI_HOME") is string memoCliHome)
            {
                HomeDirectory = new DirectoryInfo(memoCliHome);
            }
            else
            {
                HomeDirectory = new DirectoryInfo(Directory.GetCurrentDirectory());
            }

            if (Environment.GetEnvironmentVariable("MEMO_CLI_GIT_PATH") is string gitPath)
            {
                GitPath = gitPath;
            }
            else
            {
                GitPath = "git";
            }

            DirectorySeparator = '/';

            var configFile = new FileInfo(Path.Combine(HomeDirectory.FullName, ".config.json"));
            if (configFile.Exists)
            {
                MemoConfig = JsonSerializer.Deserialize<MemoConfig>(new FileStream(configFile.FullName, FileMode.Open, FileAccess.Read));
            }
            else
            {
                MemoConfig = new MemoConfig();
            }
        }
    }
}