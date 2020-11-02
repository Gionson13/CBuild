using SharpYaml;
using SharpYaml.Serialization;
using System;
using System.IO;
using System.Linq;

namespace CBuild
{
    class Program
    {
        static void Main(string[] args)
        {
            var arguments = ArgsParser.Get(args);

            if (string.IsNullOrWhiteSpace(arguments.Filepath)) return;

            var serializer = new Serializer();
            SolutionFile solutionFile;
            try
            {
                solutionFile = serializer.Deserialize<SolutionFile>(File.ReadAllText(arguments.Filepath));
            }
            catch (YamlException e)
            {
                Console.WriteLine("Parsing failed!");
                Console.WriteLine(e.Message);
                return;
            }


            if (string.IsNullOrWhiteSpace(arguments.Project))
            {
                Solution solution = new Solution();
                foreach (ProjectInSolution project in solutionFile.Projects)
                    solution.Add(serializer.Deserialize<Project>(File.ReadAllText(project.Filepath)));

                CBuild.BuildSolution(solution);
            }
            else
            {
                ProjectInSolution project = solutionFile.Projects.First(project => project.Name == arguments.Project); 
                CBuild.BuildProject(serializer.Deserialize<Project>(File.ReadAllText(project.Filepath)));
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
