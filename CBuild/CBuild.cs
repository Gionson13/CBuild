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

            switch (project.OutputType)
            {
                case "Application":
                    Link(project);
                    break;
                case "StaticLibrary":
                    CreateStaticLibrary(project);
                    break;
            }
        }

        public static void CallCommand(string command)
        {
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

        //         public static string GenerateCommand(Project project)
        //         {
        //             string command = "gcc";
        // 
        //             if (project.CompilerWarnigns)
        //                 command += " -Wall";
        // 
        //             // Std
        //             if (!string.IsNullOrWhiteSpace(project.Std))
        //                 command += $" -std={project.Std}";
        // 
        //             // Include directories
        //             if (project.IncludeDirs != null)
        //             {
        //                 foreach (string includeDir in project.IncludeDirs)
        //                 {
        //                     string includeDirectory = ConvertFilepath(includeDir, project);
        //                     command += $" -I {includeDirectory}";
        //                 }
        //             }
        // 
        //             // Library directories
        //             if (project.LibraryDirs != null)
        //             {
        //                 foreach (string libraryDir in project.LibraryDirs)
        //                 {
        //                     string libraryDirectory = ConvertFilepath(libraryDir, project);
        //                     command += $" -L {libraryDirectory}";
        //                 }
        //             }
        // 
        //             // Preprocessors
        //             if (project.Preprocessors != null)
        //             {
        //                 foreach (string preprocessor in project.Preprocessors)
        //                     command += $" -D {preprocessor}";
        //             }
        // 
        //             // Optimization
        //             if (!string.IsNullOrWhiteSpace(project.OptimizationLevel))
        //                 command += $" -O{project.OptimizationLevel}";
        // 
        //             // Files
        //             foreach (string file in project.Files)
        //                 command += " " + file;
        // 
        //             // Dependencies
        //             if (project.Dependencies != null)
        //             {
        //                 foreach (string dependency in project.Dependencies)
        //                     command += $" -l{dependency}";
        //             }
        // 
        //             // Output
        //             string outputDir = ConvertFilepath(project.OutputDir, project);
        //             command += $" -o {outputDir}/{project.ProjectName}.exe";
        // 
        //             return command;
        //         }

        public static void Compile(Project project)
        {
            string command = GenerateBasicCommand("gcc -c", project);

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

            List<Project> referenceProjects = new List<Project>();
            // Project References
            if (project.ProjectReferences != null)
            {
                foreach (string projectRef in project.ProjectReferences)
                {
                    ProjectInSolution projectInSolution = Program.solutionFile.Projects.First(project => project.Name == projectRef);

                    Serializer serializer = new Serializer();
                    Project referenceProject = serializer.Deserialize<Project>(File.ReadAllText(projectInSolution.Filepath));
                    BuildProject(referenceProject);
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

        public static string GenerateBasicCommand(string start, Project project)
        {
            if (project.CompilerWarnigns)
                start += " -Wall";

            // Std
            if (!string.IsNullOrWhiteSpace(project.Std))
                start += $" -std={project.Std}";

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
            if (project.Preprocessors != null)
            {
                foreach (string preprocessor in project.Preprocessors)
                    start += $" -D {preprocessor}";
            }

            // Optimization
            if (!string.IsNullOrWhiteSpace(project.OptimizationLevel))
                start += $" -O{project.OptimizationLevel}";

            return start;
        }

        public static string ConvertFilepath(string filepath, Project project)
        {
            string returnFilepath = filepath.Replace("$(ProjectName)", project.ProjectName);
            //                                             .Replace("$(SolutionDir)", project.SolutionDir);
            return returnFilepath;
        }
    }
}
