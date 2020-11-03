# Build C code using yaml

Requirements
------------

- GCC compiler
- ar (Archiver - comes with mingw)
- Windows

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
OutputType: Application
Files:
  - filename.c
```

You add every file you want to compile in Files and you're good to go.

> All the filepath must be relative to the solution directory.

The possible OutputType are `Application`, `StaticLibrary` and `DynamicLibrary`.

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

Then to build just type `CBuild` in the command line to build all projects, if you want to build one specific type `CBuild projectName`

Type `CBuild --help` for more.