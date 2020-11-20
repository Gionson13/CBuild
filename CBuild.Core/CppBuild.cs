using SharpYaml;
using SharpYaml.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace CBuild.Core
{
    static class CppBuild
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
        }

        private static void Compile(Project project)
        {
            string command = GenerateBasicCommand("g++ -c", project);

            // Dynamic Library
            if (project.CurrentConfiguration.OutputType == "DynamicLibrary")
                command += " -fPIC";


            // Files
            foreach (string file in project.Files)
            {
                if (!file.EndsWith(".cpp"))
                    continue;

                string filename = Path.GetFileNameWithoutExtension(file);

                Console.WriteLine($"Compiling {file} -> {filename}.o");
                Builder.CallCommand($"{command} {file} -o {project.ObjectDir}/{filename}.o");
            }
        }

        private static void Link(Project project)
        {
            string command = GenerateBasicCommand("g++", project);
            command = GenerateLinkCommand(command, project);

            // Output
            command += $" -o {project.OutputDir}/{project.ProjectName}.exe";

            Console.WriteLine($"Linking -> {command}");

            Builder.CallCommand(command);
        }

        public static void CreateStaticLibrary(Project project)
        {
            string command = $"ar rcs {project.OutputDir}/{project.ProjectName}.lib";

            // File
            foreach (string file in project.Files)
            {
                if (!file.EndsWith(".cpp"))
                    continue;

                string filename = Path.GetFileNameWithoutExtension(file);

                command += $" {project.ObjectDir}/{filename}.o";
            }

            Console.WriteLine("Generating static library -> " + command);

            Builder.CallCommand(command);
        }

        public static void CreateDynamicLibrary(Project project)
        {
            string command = GenerateBasicCommand("g++ -shared", project);
            command = GenerateLinkCommand(command, project);

            command += $" -o {project.OutputDir}/{project.ProjectName}.dll";

            Console.WriteLine("Generating dynamic library -> " + command);
            Builder.CallCommand(command);
        }

        private static string GenerateBasicCommand(string command, Project project)
        {
            if (project.CurrentConfiguration.CompilerWarnigns)
                command += " -Wall";

            // Platform
            switch (project.CurrentConfiguration.Platform)
            {
                case "x64":
                    command += " -m64";
                    break;
                case "x32":
                    command += " -m32";
                    break;
            }

            // Configuration
            if (project.CurrentConfiguration.Configuration == "Debug")
                command += " -g";

            // Std
            if (!string.IsNullOrWhiteSpace(project.CurrentConfiguration.Std))
                command += $" -std={project.CurrentConfiguration.Std}";


            // Include directories
            if (project.IncludeDirs != null)
                foreach (string includeDir in project.IncludeDirs)
                    command += $" -I {includeDir}";

            // Preprocessors
            if (project.CurrentConfiguration.Preprocessors != null)
                foreach (string preprocessor in project.CurrentConfiguration.Preprocessors)
                    command += $" -D {preprocessor}";

            // Optimization
            if (!string.IsNullOrWhiteSpace(project.CurrentConfiguration.OptimizationLevel))
                command += $" -O{project.CurrentConfiguration.OptimizationLevel}";

            return command;
        }

        private static string GenerateLinkCommand(string command, Project project)
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
                        string inputFile = $"{referenceProject.OutputDir}/{referenceProject.ProjectName}.dll";
                        string outputFile = $"{project.OutputDir}/{referenceProject.ProjectName}.dll";

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
                if (!file.EndsWith(".cpp"))
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
