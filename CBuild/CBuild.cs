using System;
using System.Diagnostics;
using System.IO;

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
            string command = GenerateCommand(project);

            string outputDir = ConvertFilepath(project.OutputDir, project);
            if (!Directory.Exists(outputDir))
                Directory.CreateDirectory(outputDir);

            Console.WriteLine(command);
            CallCommand(command);
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

        public static string GenerateCommand(Project project)
        {
            string command = "gcc";

            if (project.CompilerWarnigns)
                command += " -Wall";

            // Std
            if (!string.IsNullOrWhiteSpace(project.Std))
                command += $" -std={project.Std}";

            // Include directories
            if (project.IncludeDirs != null)
            {
                foreach (string includeDir in project.IncludeDirs)
                {
                    string includeDirectory = ConvertFilepath(includeDir, project);
                    command += $" -I {includeDirectory}";
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

            // Preprocessors
            if (project.Preprocessors != null)
            {
                foreach (string preprocessor in project.Preprocessors)
                    command += $" -D {preprocessor}";
            }

            // Optimization
            if (!string.IsNullOrWhiteSpace(project.OptimizationLevel))
                command += $" -O{project.OptimizationLevel}";

            // Files
            foreach (string file in project.Files)
                command += " " + file;

            // Dependencies
            if (project.Dependencies != null)
            {
                foreach (string dependency in project.Dependencies)
                    command += $" -l{dependency}";
            }

            // Output
            string outputDir = ConvertFilepath(project.OutputDir, project);
            command += $" -o {outputDir}/{project.ProjectName}.exe";

            return command;
        }

        public static string ConvertFilepath(string filepath, Project project)
        {
            string returnFilepath = filepath.Replace("$(ProjectName)", project.ProjectName);
//                                             .Replace("$(SolutionDir)", project.SolutionDir);
            return returnFilepath;
        }
    }
}
