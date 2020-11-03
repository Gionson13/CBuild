using SharpYaml;
using SharpYaml.Serialization;
using System;
using System.IO;
using System.Linq;

namespace CBuild
{
    class Program
    {
        public static SolutionFile solutionFile;

        static void Main(string[] args)
        {
            var arguments = ArgsParser.Get(args);

            if (string.IsNullOrWhiteSpace(arguments.Filepath)) return;

            if (!File.Exists(arguments.Filepath))
            {
                Console.WriteLine("Parsing failed! -> " + arguments.Filepath);
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write("ERROR ");
                Console.ResetColor();
                Console.WriteLine($"{new FileNotFoundException().HResult} -> {new FileNotFoundException().Message}");
                return;
            }

            var serializer = new Serializer();
            try
            {
                solutionFile = serializer.Deserialize<SolutionFile>(File.ReadAllText(arguments.Filepath));
            }
            catch (YamlException e)
            {
                Console.WriteLine("Parsing failed! -> " + arguments.Filepath);
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write("ERROR ");
                Console.ResetColor();
                Console.WriteLine($"{e.HResult} -> {e.Message}");
                return;
            }


            if (string.IsNullOrWhiteSpace(arguments.Project))
            {
                Solution solution = new Solution();
                foreach (ProjectInSolution project in solutionFile.Projects)
                {
                    Project toAdd = serializer.Deserialize<Project>(File.ReadAllText(project.Filepath));
                    toAdd.Filepath = project.Filepath;
                    solution.Add(toAdd);
                }

                CBuild.BuildSolution(solution);
            }
            else
            {
                ProjectInSolution projectInSolution = solutionFile.Projects.First(project => project.Name == arguments.Project);
                Project project;

                try
                {
                    project = serializer.Deserialize<Project>(File.ReadAllText(projectInSolution.Filepath));
                    project.Filepath = projectInSolution.Filepath;
                }
                catch (YamlException e)
                {
                    Console.WriteLine("Parsing failed! -> " + projectInSolution.Filepath);
                    Console.WriteLine($"ERROR {e.HResult} -> {e.Message}");
                    return;
                }
                
                CBuild.BuildProject(project);
            }

        }
    }

    struct ProjectInSolution
    {
        public string Filepath { get; set; }
        public string Name { get; set; }
    }

    struct SolutionFile
    {
        public ProjectInSolution[] Projects { get; set; }
    }
}
