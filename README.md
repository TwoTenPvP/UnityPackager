# UnityPackager
UnityPackager is a simple utility program written in C# to pack and unpack Unity's own UnityPackage file format (usually associated with the .unitypackage extension). It's very useful for CI servers.

The UnityPackage format is not open and everything this utility does is based on reverse engineering the fairly simplistic file format. Thus it might not fit all specifications, as they are not public.

_Note that this is not officially supported or endorsed by Unity. Use at your own risk._

## Runtime
Runs on Linux with the Mono runtime and Windows

## Usage

#### Pack
``./UnityPackager pack Output.unitypackage MyInputFile.cs Assets/Editor/TargetExportPath.cs``
#### Unpack
``./UnityPackager unpack Input.unitypackage ProjectFolder``