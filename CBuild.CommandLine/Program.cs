using CBuild.Core;
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

            if (!File.Exists(arguments.Filepath))
            {
                Console.WriteLine("Parsing failed! -> " + arguments.Filepath);
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write("ERROR ");
                Console.ResetColor();
                Console.WriteLine($"{new FileNotFoundException().HResult} -> {new FileNotFoundException().Message}");
                return;
            }

            string[] projectConfiguration = null;
            if (!string.IsNullOrWhiteSpace(arguments.ProjectConfiguration))
                projectConfiguration = arguments.ProjectConfiguration.Split('/');

            Serializer serializer = new Serializer();

            if (string.IsNullOrWhiteSpace(arguments.Project))
            {
                Solution solution = serializer.DeserializeSolution(arguments.Filepath);

                if (solution.IsEmpty()) return;

                for (int i = 0; i < solution.Projects.Count; i++)
                {
                    if (projectConfiguration != null)
                        solution.Projects[i].CurrentConfiguration = solution.Projects[i].ProjectConfigurations.First(projectConfig =>
                            projectConfig.Configuration == projectConfiguration[0] &&
                            projectConfig.Platform == projectConfiguration[1]);
                }

                Builder.BuildSolution(solution);
            }
            else
            {
                Project project = serializer.DeserializeProject(arguments.Filepath, arguments.Project);

                if (projectConfiguration != null)
                    project.CurrentConfiguration = project.ProjectConfigurations.First(projectConfig =>
                        projectConfig.Configuration == projectConfiguration[0] &&
                        projectConfig.Platform == projectConfiguration[1]);

                Builder.BuildProject(project);
            }

        }
    }

}
