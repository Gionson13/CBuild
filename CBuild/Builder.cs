using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace CBuild
{
    static class Builder
    {
        public static bool AsFile = false;

        public static void BuildSolution(Solution solution)
        {
            foreach (Project project in solution.Projects)
                BuildProject(project);
        }

        public static void BuildProject(Project project)
        {
            project = ConvertAllFilepaths(project);

            if (!Directory.Exists(project.OutputDir))
                Directory.CreateDirectory(project.OutputDir);

            if (!Directory.Exists(project.ObjectDir))
                Directory.CreateDirectory(project.ObjectDir);

            switch (project.Language)
            {
                case "C":
                    CBuild.BuildProject(project);
                    break;
                case "Cpp":
                    CppBuild.BuildProject(project);
                    break;
                default:
                    Console.WriteLine("Parsing failed! -> " + project.Filepath);
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Write("ERROR ");
                    Console.ResetColor();
                    Console.WriteLine($"{new KeyNotFoundException().HResult} -> Invalid language");
                    return;
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

        private static Project ConvertAllFilepaths(Project project)
        {
            // OtputDir
            project.OutputDir = ConvertFilepath(project.OutputDir, project);

            // ObjectDir
            project.ObjectDir = ConvertFilepath(project.ObjectDir, project);

            // Files
            for (int i = 0; i < project.Files.Length; i++)
                project.Files[i] = ConvertFilepath(project.Files[i], project);

            // Include directories
            if (project.IncludeDirs != null)
                for (int i = 0; i < project.IncludeDirs.Length; i++)
                    project.IncludeDirs[i] = ConvertFilepath(project.IncludeDirs[i], project);

            // Library directories
            if (project.LibraryDirs != null)
                for (int i = 0; i < project.LibraryDirs.Length; i++)
                    project.LibraryDirs[i] = ConvertFilepath(project.LibraryDirs[i], project);

            return project;
        }

        public static string ConvertFilepath(string filepath, Project project)
        {
            string returnFilepath = filepath.Replace("$(ProjectName)", project.ProjectName)
                                            .Replace("$(ProjectDir)", Path.GetDirectoryName(project.Filepath))
                                            .Replace("$(Configuration)", project.CurrentConfiguration.Configuration)
                                            .Replace("$(Platform)", project.CurrentConfiguration.Platform);
            //                                             .Replace("$(SolutionDir)", project.SolutionDir);
            return returnFilepath;
        }
    }
}
