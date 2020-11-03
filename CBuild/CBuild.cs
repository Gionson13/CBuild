using SharpYaml;
using SharpYaml.Serialization;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace CBuild
{
    static class CBuild
    {
        public static bool AsFile = false;

        public static void BuildSolution(Solution solution)
        {
            foreach (Project project in solution.Projects)
            {
                BuildProject(project);
            }
        }

        public static void BuildProject(Project project)
        {
            string outputDir = ConvertFilepath(project.OutputDir, project);
            if (!Directory.Exists(outputDir))
                Directory.CreateDirectory(outputDir);

            string objectDir = ConvertFilepath(project.ObjectDir, project);
            if (!Directory.Exists(objectDir))
                Directory.CreateDirectory(objectDir);

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

        public static void CallCommand(string command)
        {
            if (AsFile)
            {
                File.AppendAllText("CBuild.bat", command + "\n");
                return;
            }

            Process cmd = new Process();
            cmd.StartInfo.FileName = "cmd.exe";
            cmd.StartInfo.RedirectStandardInput = true;
            cmd.StartInfo.RedirectStandardOutput = true;
            cmd.StartInfo.UseShellExecute = false;
            cmd.Start();

            cmd.StandardInput.WriteLine(command);
            cmd.StandardInput.Flush();
            cmd.StandardInput.Close();
            cmd.WaitForExit();

            Console.WriteLine(new string('-', Console.BufferWidth));
        }

        public static void Compile(Project project)
        {
            string command = GenerateBasicCommand("gcc -c", project);

            // Dynamic Library
            if (project.CurrentConfiguration.OutputType == "DynamicLibrary")
                command += " -fPIC";

            // Files
            foreach (string file in project.Files)
            {
                if (!file.EndsWith(".c"))
                    continue;

                string filename = Path.GetFileNameWithoutExtension(file);
                string objectDir = ConvertFilepath(project.ObjectDir, project);

                Console.WriteLine($"Compiling {file} -> {filename}.o");
                CallCommand($"{command} {file} -o {objectDir}/{filename}.o");
            }
        }

        public static void Link(Project project)
        {
            string command = GenerateBasicCommand("gcc", project);
            command = GenerateLinkCommand(command, project);

            if (string.IsNullOrWhiteSpace(command))
                return;
            
            // Output
            string outputDir = ConvertFilepath(project.OutputDir, project);
            command += $" -o {outputDir}/{project.ProjectName}.exe";

            Console.WriteLine($"Linking -> {command}");

            CallCommand(command);
        }

        public static void CreateStaticLibrary(Project project)
        {
            string outputDir = ConvertFilepath(project.OutputDir, project);
            string command = $"ar rcs {outputDir}/{project.ProjectName}.lib";


            // File
            foreach (string file in project.Files)
            {
                if (!file.EndsWith(".c"))
                    continue;

                string filename = Path.GetFileNameWithoutExtension(file);
                string objectDir = ConvertFilepath(project.ObjectDir, project);

                command += $" {objectDir}/{filename}.o";
            }

            Console.WriteLine("Generating static library -> " + command);

            CallCommand(command);
        }

        public static void CreateDynamicLibrary(Project project)
        {
            string outputDir = ConvertFilepath(project.OutputDir, project);
            string command = GenerateBasicCommand("gcc -shared", project);
            command = GenerateLinkCommand(command, project);

            if (string.IsNullOrWhiteSpace(command))
                return;

            command += $" -o {outputDir}/{project.ProjectName}.dll";

            Console.WriteLine("Generating dynamic library -> " + command);
            CallCommand(command);
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
            {
                foreach (string includeDir in project.IncludeDirs)
                {
                    string includeDirectory = ConvertFilepath(includeDir, project);
                    start += $" -I {includeDirectory}";
                }
            }

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
                        projectInSolution = Program.solutionFile.Projects.First(project => project.Name == projectRef);
                    }
                    catch (InvalidOperationException e)
                    {
                        Console.WriteLine("Linking failed! -> " +  project.Filepath);
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
                        referenceProject = serializer.Deserialize<Project>(File.ReadAllText(projectInSolution.Filepath));
                        referenceProject.Filepath = projectInSolution.Filepath;
                        try
                        {
                        referenceProject.CurrentConfiguration = referenceProject.ProjectConfigurations.First(config =>
                            config.Configuration == project.CurrentConfiguration.Configuration &&
                            config.Platform == project.CurrentConfiguration.Platform);
                        }
                        catch (InvalidOperationException e)
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

                    BuildProject(referenceProject);
                    if (referenceProject.CurrentConfiguration.OutputType == "DynamicLibrary" && !AsFile)
                    {
                        string inputFile = $"{ConvertFilepath(referenceProject.OutputDir, referenceProject)}/{referenceProject.ProjectName}.dll";
                        string outputFile = $"{ConvertFilepath(project.OutputDir, project)}/{referenceProject.ProjectName}.dll";

                        File.Copy(inputFile, outputFile, true);
                    }

                    referenceProjects.Add(referenceProject);

                    command += $" -L {ConvertFilepath(referenceProject.OutputDir, referenceProject)}";
                }
            }

            // Library directories
            if (project.LibraryDirs != null)
            {
                foreach (string libraryDir in project.LibraryDirs)
                {
                    string libraryDirectory = ConvertFilepath(libraryDir, project);
                    command += $" -L {libraryDirectory}";
                }
            }

            // Files
            foreach (string file in project.Files)
            {
                if (!file.EndsWith(".c"))
                    continue;

                string filename = Path.GetFileNameWithoutExtension(file);
                string objectDir = ConvertFilepath(project.ObjectDir, project);

                command += $" {objectDir}/{filename}.o";
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

        public static string ConvertFilepath(string filepath, Project project)
        {
            string returnFilepath = filepath.Replace("$(ProjectName)", project.ProjectName)
                                            .Replace("$(Configuration)", project.CurrentConfiguration.Configuration)
                                            .Replace("$(Platform)", project.CurrentConfiguration.Platform);
            //                                             .Replace("$(SolutionDir)", project.SolutionDir);
            return returnFilepath;
        }
    }
}
