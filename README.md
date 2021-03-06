# Build C code using yaml

Requirements
------------

- Windows
  - MingGW
- Linux
  - gcc
  - g++
  - ar

Build C code
------------ 

You can use `CBuild --generate solutionName` to generate a basic solution template folder, it will look like this:

```
Main Folder
│   solutionName.csln    
│
└───projectName
    │   projectName.cproj

```

if you look in `solutionName.csln` you find this:

```yaml
Projects: 
  - Name: solutionName
    Filepath: projectName/projectName.cproj
```

By following this structure you can add as many projects as you want, or you can use `CBuild --add projectName` to add a new project automatically.

If you look in projectName.cproj

```yaml
ProjectName: projectName
OutputDir: bin
ObjectDir: obj
Language: C
Files:
  - filename.c
ProjectConfigurations:
  - Configuration: Release
    Platform: x64
    OutputType: Application
    Preprocessors: null
    Std: null
    OptimizationLevel: null
    CompilerWarnigns: true
IncludeDirs: null
LibraryDirs: null
Dependencies: null
ProjectReferences: null
Content: null
```

You add every file (path relative to project directory) you want to compile in Files and you're good to go.

> All the filepaths are relative to the solution directory unless it is specified otherwise.

The possible OutputType are `Application`, `StaticLibrary` and `DynamicLibrary`.
And the supported languages are `C` and `Cpp`. 

There are also other option: 

```yaml
IncludeDirs:
  - path/to/directory
```
 List of the additional include directories.

```yaml
LibraryDirs:
  - path/to/directory
```
 List of the additional library directories.

```yaml
Dependencies:
  - filename
```
List of additional dependencies to use.

```yaml
ProjectReferences:
  - ProjectName
```
List of reference to projects in the same solution. If you build this project, all the project referenced will be built.

```yaml
Preprocessors:
  - PREPROCESSOR
```
List of all preprocessors to use.

```yaml
OptimizationLevel: 0
```
The optimization of the compiling. (default to 0)

```yaml
CompilerWarnigns: true
```
Shows the compiler's warnings if true. (default to true)

```yaml
Std: c18
```
Set the std version. (default to latest)

```yaml
Content:
  - path/to/file.txt
```
The content will be copied to the output folder following the right folder structure, the path is relative to the projectDirectory.

Then to build just type `CBuild` in the command line to build all projects, if you want to build one specific type `CBuild projectName`, and you can specify the configuration by typing `CBuild projectName configuration`, configuration must be `Configuration/Platform` (e.g `Debug/x64`).

There also are macros for directories:
```yaml
$(ProjectName) -> The name of the project
$(Configuration) -> The current configuration (Debug / Release)
$(Platform) -> The current platform (x64 / x32)
```

Type `CBuild --help` for more.


Build
-----

To build the solution run
```
dotnet build -p:DefineConstants=YOUR_PLATFORM /p:Configuration=Release
```

> Replace YOUR_PLATFORM with WINDOWS or LINUX depending on wich platform you are.
