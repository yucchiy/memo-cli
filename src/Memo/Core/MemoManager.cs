using System.IO;
using Memo.Core;

namespace Memo
{
    public class MemoManager : IMemoManager
    {
       private Configuration Config { get; }

        public MemoManager()
        {
            // TODO: Fix
            var commandConfig = new CommandConfig();
            Config = new Configuration()
            {
                HomeDirectory = commandConfig.HomeDirectory
            };
        }

        public DirectoryInfo GetRoot() => Config.HomeDirectory;

        public class Configuration
        {
            public DirectoryInfo HomeDirectory;
        }
    }
}