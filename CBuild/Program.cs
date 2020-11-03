using SharpYaml;
using SharpYaml.Serialization;
using System;
using System.Collections.Generic;
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

            string[] projectConfiguration = null;
            if (!string.IsNullOrWhiteSpace(arguments.ProjectConfiguration))
                projectConfiguration = arguments.ProjectConfiguration.Split('/');

            if (string.IsNullOrWhiteSpace(arguments.Project))
            {
                Solution solution = new Solution();
                foreach (ProjectInSolution project in solutionFile.Projects)
                {
                    try
                    {
                        Project toAdd = serializer.Deserialize<Project>(File.ReadAllText(project.Filepath));
                        toAdd.Filepath = project.Filepath;
                        if (projectConfiguration != null)
                            toAdd.CurrentConfiguration = toAdd.ProjectConfigurations.First(projectConfig =>
                                projectConfig.Configuration == projectConfiguration[0] &&
                                projectConfig.Platform == projectConfiguration[1]
                            );
                        else toAdd.CurrentConfiguration = toAdd.ProjectConfigurations[0];

                        solution.Add(toAdd);
                    }
                    catch (YamlException e)
                    {
                        Console.WriteLine("Parsing failed! -> " + project.Filepath);
                        Console.WriteLine($"ERROR {e.HResult} -> {e.Message}");
                        return;
                    }
                    catch (InvalidOperationException e)
                    {
                        Console.WriteLine("Parsing failed! -> " + project.Filepath);
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.Write("ERROR ");
                        Console.ResetColor();
                        Console.WriteLine($"{e.HResult} -> Project Configuration not found");
                        return;
                    }
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
                    if (projectConfiguration != null)
                        project.CurrentConfiguration = project.ProjectConfigurations.First(projectConfig =>
                            projectConfig.Configuration == projectConfiguration[0] &&
                            projectConfig.Platform == projectConfiguration[1]
                        );
                    else project.CurrentConfiguration = project.ProjectConfigurations[0];
                }
                catch (YamlException e)
                {
                    Console.WriteLine("Parsing failed! -> " + projectInSolution.Filepath);
                    Console.WriteLine($"ERROR {e.HResult} -> {e.Message}");
                    return;
                }
                catch (InvalidOperationException e)
                {
                    Console.WriteLine("Parsing failed! -> " + projectInSolution.Filepath);
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Write("ERROR ");
                    Console.ResetColor();
                    Console.WriteLine($"{e.HResult} -> Project Configuration not found");
                    return;
                }

                CBuild.BuildProject(project);
            }

        }
    }

    struct ProjectInSolution
    {
        [YamlMember(0)]
        public string Name { get; set; }
        [YamlMember(1)]
        public string Filepath { get; set; }
    }

    struct SolutionFile
    {
        public List<ProjectInSolution> Projects { get; set; }
    }
}
