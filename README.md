# memo-cli

This is a small CLI tool for managing Markdown notes.
You can create/list notes using this tool on terminal.

## Installation

Install memo-cli using .NET Tool.

```
$ git clone git@github.com:yucchiy/memo-cli.git
$ cd /path/to/memo-cli
# install memo-cli as memo to local
$ dotnet tool install memo --add-source ./src/Memo/.nupkg 
# or install to global
$ dotnet tool install memo --global  --add-source ./src/Memo/.nupkg 
```

## Usage

`memo` provides subcommands for managing your markdown memos.

### Directory structure

All memos are managed under MEMO_CLI_HOME directory. You can specified its location by an environment variable named `$MEMO_CLI_HOME`.

The Directory structure under MEMO_CLI_HOME is something like below.

```
<MEMO_CLI_HOME>
├── blog
│   ├── drafts
│   │   └── memo-clis.markdown # category is blog/drafts
└── journals
    ├── 2021-03-05.markdown # category is journals
    ├── 2021-03-06.markdown
    └── 2021-03-07.markdown
```

### Create a new memo

You can make a memo with the `new` (or `n`) command.

```
Usage:
  Memo new [options]

Options:
  -t, --title <title>          Title of note.
  -c, --category <category>    Category of note. Note must belong to one category
  -f, --filename <filename>    File name of note. It automatically adds '.markdown' file extension if omitted
  --no-color                   Disable colorized output [default: False]
  -?, -h, --help               Show help and usage information
```

### List memos

`list` (or `ls`) command show list of your memos.

```
Usage:
  memo list [options]

Options:
  -c, --category <category>    Filter list by category name with regular expression [default: ]
  -t, --type <type>            Filter list by type with regular expression [default: ]
  -r, --relative               Show relative paths from $MEMO_CLI_HOME directory [default: False]
  -f, --format <format>        Specified output format [default: ]
  --no-color                   Disable colorized output [default: False]
  -?, -h, --help               Show help and usage information
```

### Category Rule

## License

[MIT License](LICENSE)