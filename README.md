# G3Archive
G3Archive is a Gothic 3 .pak archive extractor and creator written entirely in C# based on NicoDE's G3Pak file format specification.

> [!IMPORTANT]
> G3Archive requires .NET 7.0 installed

# Installation
Download the [latest release](https://github.com/gekonnn/G3Archive/releases/) from Releases tab, after extracting the .zip file open the command prompt, `cd` into extracted directory and run the command following the usage below.

# Usage and options
### General usage:
```
G3Archive [options]
```
### Options:
```
--extract, -e       <path>  Extracts the selected archive
--pack, -p          <path>  Packages the selected folder into a PAK archive
--dest, -d          <path>  Specifies path of output file
--compression       <0-9>   Specifies the compression level for compressed files
--no-decompress             Prevents files from decompressing when extracting
--no-deleted                Prevents files marked as deleted from being output
--overwrite                 Forces overwriting of existing files
--quiet                     Hides any output information
```
### Example:
```
G3Archive --extract "C:\Program Files (x86)\Steam\steamapps\common\Gothic 3\Data\_compiledAnimation.pak" --overwrite
```
# Dependencies
- [CommandLineParser](https://github.com/commandlineparser/commandline)
- [DotNetZip.Semverd](https://github.com/haf/DotNetZip.Semverd)
## TODOs:
- GUI version