# Build C code using yaml

Requirements
------------

- GCC compiler
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
  - { Name: solutionName, Filepath: projectName/projectName.cproj }
```

By following this structure you can add as many projects as you want, or you can use `CBuild --add projectName` to add a new project automatically.

If you look in projectName.cproj

```yaml
ProjectName: projectName
Files:
  - filename.c
OutputDir: bin
```

You add every file you want to compile in Files and you're good to go.

> All the filepath must be relative to the solution directory.

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
Std: c11
```
Set the std version. (default to latest)

Then to build just type `CBuild solutionName.csln` in the command line to build all projects, if you want to build one specific type `CBuild solutionName.csln projectName`

