using System.IO;
using SharpYaml;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CBuild.Core
{
    static class CBuild
    {
        public static void BuildProject(Project project)
        {
            Compile(project);

            switch (project.CurrentConfiguration.OutputType)
            {
                case "Application":
                    Link(project);
                    break;
                case "StaticLibrary":
                    CreateStaticLibrary(project);
                    break;
                case "DynamicLibrary":
                    CreateDynamicLibrary(project);
                    break;
                default:
                    Console.WriteLine("Invalid output type");
                    break;
            }

            Builder.CopyContent(project);
        }

        public static void Compile(Project project)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write($"Compiling {project.ProjectName} " +
                new string('-', Console.BufferWidth - $"Compiling {project.ProjectName} ".Length));
            Console.ResetColor();

            string command = GenerateBasicCommand("gcc -c", project);

            // Dynamic Library
            if (project.CurrentConfiguration.OutputType == "DynamicLibrary")
                command += " -fPIC";

            // Files
            foreach (string file in project.Files)
            {
                if (!file.EndsWith(".c"))
                    continue;

                string filepath = $"{Path.GetDirectoryName(project.Filepath)}/{file}";
                string filename = Path.GetFileNameWithoutExtension(file);

                Console.WriteLine($"{file} -> {filename}.o");
                Builder.CallCommand($"{command} {filepath} -o {project.ObjectDir}/{filename}.o");
            }
        }

        public static void Link(Project project)
        {

            string command = GenerateBasicCommand("gcc", project);
            command = GenerateLinkCommand(command, project);

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write($"Linking {project.ProjectName} " +
                new string('-', Console.BufferWidth - $"Linking {project.ProjectName} ".Length));
            Console.ResetColor();

            // Output
#if WINDOWS
            command += $" -o {project.OutputDir}/{project.ProjectName}.exe";
#elif LINUX
            command += $" -o {project.OutputDir}/{project.ProjectName}";
#endif

            Console.WriteLine($"Linking -> {command}");
            Builder.CallCommand(command);
        }

        public static void CreateStaticLibrary(Project project)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write($"Linking {project.ProjectName} " +
                new string('-', Console.BufferWidth - $"Linking {project.ProjectName} ".Length));
            Console.ResetColor();

#if WINDOWS
            string command = $"ar rcs {project.OutputDir}/{project.ProjectName}.lib";
#elif LINUX
            string command = $"ar rcs {project.OutputDir}/{project.ProjectName}.a";
#else
#error PLATFORM NOT DEFINED (WINDOWS | LINUX)
#endif
            // File
            foreach (string file in project.Files)
            {
                if (!file.EndsWith(".c"))
                    continue;

                string filename = Path.GetFileNameWithoutExtension(file);

                command += $" {project.ObjectDir}/{filename}.o";
            }

            Console.WriteLine("Generating static library -> " + command);
            Builder.CallCommand(command);
        }

        public static void CreateDynamicLibrary(Project project)
        {
            string command = GenerateBasicCommand("gcc -shared", project);
            command = GenerateLinkCommand(command, project);

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write($"Linking {project.ProjectName} " +
                new string('-', Console.BufferWidth - $"Linking {project.ProjectName} ".Length));
            Console.ResetColor();

            if (string.IsNullOrWhiteSpace(command))
                return;

#if WINDOWS
            command += $" -o {project.OutputDir}/{project.ProjectName}.dll";
#elif LINUX
            command += $" -o {project.OutputDir}/{project.ProjectName}.so";
#endif
            Console.WriteLine("Generating dynamic library -> " + command);
            Builder.CallCommand(command);
        }

        public static string GenerateBasicCommand(string start, Project project)
        {
            if (project.CurrentConfiguration.CompilerWarnigns)
                start += " -Wall";

            // Platform
            switch (project.CurrentConfiguration.Platform)
            {
                case "x64":
                    start += " -m64";
                    break;
                case "x32":
                    start += " -m32";
                    break;
            }

            // Configuration
            if (project.CurrentConfiguration.Configuration == "Debug")
                start += " -g";

            // Std
            if (!string.IsNullOrWhiteSpace(project.CurrentConfiguration.Std))
                start += $" -std={project.CurrentConfiguration.Std}";

            // Include directories
            if (project.IncludeDirs != null)
                foreach (string includeDir in project.IncludeDirs)
                    start += $" -I {includeDir}";

            // Preprocessors
            if (project.CurrentConfiguration.Preprocessors != null)
            {
                foreach (string preprocessor in project.CurrentConfiguration.Preprocessors)
                    start += $" -D {preprocessor}";
            }

            // Optimization
            if (!string.IsNullOrWhiteSpace(project.CurrentConfiguration.OptimizationLevel))
                start += $" -O{project.CurrentConfiguration.OptimizationLevel}";

            return start;
        }

        public static string GenerateLinkCommand(string command, Project project)
        {
            // Configuration
            if (project.CurrentConfiguration.Configuration == "Release")
                command += " -s";

            // Project References
            List<Project> referenceProjects = new List<Project>();
            if (project.ProjectReferences != null)
            {
                foreach (string projectRef in project.ProjectReferences)
                {
                    ProjectInSolution projectInSolution;
                    try
                    {
                        projectInSolution = Builder.SolutionFile.Projects.First(project => project.Name == projectRef);
                    }
                    catch (InvalidOperationException e)
                    {
                        Console.WriteLine("Linking failed! -> " + project.Filepath);
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.Write("ERROR ");
                        Console.ResetColor();
                        Console.WriteLine($"{e.HResult} -> Project Reference not found");
                        return "";
                    }
                    Serializer serializer = new Serializer();
                    Project referenceProject;
                    try
                    {
                        referenceProject = serializer.DeserializeProject(projectInSolution.Filepath);
                        try
                        {
                            referenceProject.CurrentConfiguration = referenceProject.ProjectConfigurations.First(config =>
                                config.Configuration == project.CurrentConfiguration.Configuration &&
                                config.Platform == project.CurrentConfiguration.Platform);
                        }
                        catch (InvalidOperationException)
                        {
                            referenceProject.CurrentConfiguration = referenceProject.ProjectConfigurations[0];
                        }
                    }
                    catch (YamlException e)
                    {
                        Console.WriteLine("Parsing failed! -> " + projectInSolution.Filepath);
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.Write("ERROR ");
                        Console.ResetColor();
                        Console.WriteLine($"{e.HResult} -> {e.Message}");
                        return "";
                    }

                    Builder.BuildProject(referenceProject);
                    if (referenceProject.CurrentConfiguration.OutputType == "DynamicLibrary" && !Builder.AsFile)
                    {
#if WINDOWS
                        string inputFile = $"{referenceProject.OutputDir}/{referenceProject.ProjectName}.dll";
                        string outputFile = $"{project.OutputDir}/{referenceProject.ProjectName}.dll";
#elif LINUX
                        string inputFile = $"{referenceProject.OutputDir}/{referenceProject.ProjectName}.so";
                        string outputFile = $"{project.OutputDir}/{referenceProject.ProjectName}.so";
#endif
                        File.Copy(inputFile, outputFile, true);
                    }

                    referenceProjects.Add(referenceProject);

                    command += $" -L {referenceProject.OutputDir}";
                }
            }

            // Library directories
            if (project.LibraryDirs != null)
                foreach (string libraryDir in project.LibraryDirs)
                    command += $" -L {libraryDir}";

            // Files
            foreach (string file in project.Files)
            {
                if (!file.EndsWith(".c"))
                    continue;

                string filename = Path.GetFileNameWithoutExtension(file);
                command += $" {project.ObjectDir}/{filename}.o";
            }

            // Project reference lib
            foreach (Project refProject in referenceProjects)
                command += $" -l{ refProject.ProjectName}";

            // Dependencies
            if (project.Dependencies != null)
            {
                foreach (string dependency in project.Dependencies)
                    command += $" -l{dependency}";
            }

            return command;
        }
    }
}
