using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace CBuild.Core
{
    public static class Builder
    {
        public static bool AsFile = false;
        public static SolutionFile SolutionFile;

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

            CBuild.BuildProject(project);
        }

        public static void CallCommand(string command)
        {
            if (AsFile)
            {
#if WINDOWS
                File.AppendAllText("CBuild.bat", command + "\n");
#elif LINUX
                File.AppendAllText("CBuild.sh", command + "\n");
#endif
                return;
            }

            Process cmd = new Process();
#if WINDOWS
            cmd.StartInfo.FileName = "cmd.exe";
#elif LINUX
            cmd.StartInfo.FileName = "sh";
#endif
            cmd.StartInfo.RedirectStandardInput = true;
            cmd.StartInfo.RedirectStandardOutput = true;
            cmd.StartInfo.UseShellExecute = false;
            cmd.Start();

            cmd.StandardInput.WriteLine(command);
            cmd.StandardInput.Flush();
            cmd.StandardInput.Close();
            cmd.WaitForExit();

        }

        public static void CopyContent(Project project)
        {
            if (project.Content != null)
            {
                foreach (string content in project.Content)
                {
                    string file = $"{Path.GetDirectoryName(project.Filepath)}/{content}";
                    string outputFile = $"{project.OutputDir}/{content}";
                    string outputDir = Path.GetDirectoryName(outputFile);

                    if (!Directory.Exists(outputDir))
                        Directory.CreateDirectory(outputDir);

                    File.Copy(file, outputFile, true);
                }
            }
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

            // Content
            if (project.Content != null)
                for (int i = 0; i < project.Content.Length; i++)
                    project.Content[i] = ConvertFilepath(project.Content[i], project);

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
