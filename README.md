# Build C code using yaml

Requirements
------------

- GCC compiler
- Windows

Build C code
------------ 

You can use `CBuild --generate` to generate a basic yaml template file, it will look like this:

```yaml
ProjectName: HelloWorld
Files:
  - src/HelloWorld.c
OutputDir: bin
```

You add every file you want to compile in Files and you're good to go.

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
  - filename.lib
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

Then to build just type `CBuild cbuild.yaml` in the command line

