## Developer Notes

VinylCutter is a .NET console application written in C# which converts C# definitions to C# generated code. It is powered by [roslyn](https://github.com/dotnet/roslyn) under the hood.

## Building

VinyCutter can be built and run from either the command line or Visual Studio for Mac.

### Command Line

- `make prepare` - Fetch dependencies from nuget. A bit slow, it only needs to be done on fresh syncs or when dependencies have been updated.
- `make` - Build debug
- `make test` - Run all nunit test
- `make dist` - Generate a "bundle" for local use


### Visual Studio for Mac

- Open VinylCutter.sln and Build
- Run tests from the the "Unit Tests" pad
- `make dist` from the command line is needed currently to generate a distribution folder


## Archtecture

VinyCutter is structured as a set of loosely coupled steps, with unit tests and record classes connecting them.

- [EntryPoint](https://github.com/chamons/VinylCutter/blob/master/src/VinylCutter/EntryPoint.cs) contains the Main method and uses [Mono.Options](https://github.com/mono/mono/blob/master/mcs/class/Mono.Options/Mono.Options/Options.cs) to parse command line input.
- These options are fed into a [VinylCutterTool](https://github.com/chamons/VinylCutter/blob/master/src/VinylCutter/VinylCutterTool.cs) which validates options and is `Run()` to begin.
- The file or standard in is read and the next is passed to a [Parser](https://github.com/chamons/VinylCutter/blob/master/src/VinylCutter/Parser.cs).
- The Parser uses Roslyn to compile the file and reflect upon its structure, generating a tree of [data structures](https://github.com/chamons/VinylCutter/blob/master/src/VinylCutter/ParserRecords.g.cs) that describe the record structure desired.
- VinylCutterTool passes the top level data structure, a FileInfo, to a [CodeGenerator](https://github.com/chamons/VinylCutter/blob/master/src/VinylCutter/CodeGenerator.cs) to generate C# code.
- CodeGenerator is a simple and straight forward code generator, outputing line of text into a [CodeWriter](https://github.com/chamons/VinylCutter/blob/master/src/VinylCutter/CodeWriter.cs), which is a thin wrapper around StringBuilder than takes care of indentation.
- VinylCutterTool then writes the output out to the desired location (or standard out).

## Development Process

New features tend to begin with a Parser Test that describes the C# that should be processed and what data structure changes should occur.

Almost all feature then end up with either a CodeGenerator Test or a line in the top level Integration Test, depending on their scope.

The coding conventions are roughly the ones from [mono](http://www.mono-project.com/community/contributing/coding-guidelines/) with a few major exceptions:

- `{' and '}' are on new lines in almost all cases
- 4 space tabs

PRs should be filed for all changes moving forward.

## Vim Tricks

The unit test workflow in Visual Studio for Mac is a bit rough, so I often run tests from vim. 

I've developed a few special makefile targets and bit of vim script that may be useful:

`nnoremap <leader>r :w<cr>:let $TEST_FILES=expand('%')<cr>:!make test-fast<cr>` - This builds the current file as a nunit test and runs just the tests inside.

`nnoremap <leader>a :w<cr>:!make test-fast<cr>` - This runs all nunit tests without invoking msbuild (2x faster on average).

`nnoremap <leader>b :w<cr>:!make<cr>` - This runs a full build, useful to check for syntax errors.