# G3Archive
G3Archive is a Gothic 3 .pak archive extractor written entirely in C# based on G3Pak's file format specification.

> [!IMPORTANT]
> G3Archive requires at least .NET 7.0 installed

# Installation
Download the [latest release](https://github.com/gekonnn/G3Archive/releases/) from Releases tab, after extracting the .zip file open the command prompt, `cd` into extracted directory and run the command following the usage below.

# Usage and options
### General usage:
```
G3Archive [options]
```
### Options:
```
--extract  -e <path>   Extracts the selected archive
--dest     -d <path>   Specifies directory the archive will be extracted in
```
### Example:
```
G3Archive --extract "C:\Program Files (x86)\Steam\steamapps\common\Gothic 3\Data\_compiledAnimation.pak"
```

# Dependencies
- https://github.com/commandlineparser/commandline