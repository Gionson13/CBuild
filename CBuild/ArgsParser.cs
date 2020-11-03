using SharpYaml.Serialization;
using System;
using System.Collections.Generic;
using System.IO;

namespace CBuild
{
    struct Args
    {
        public string Filepath;
        public string Project;
    }

    static class ArgsParser
    {
        public static Args Get(string[] args)
        {
            Args returnArgs = new Args();

            if (args.Length < 1)
            {
                Console.WriteLine("Missing arguments.");
                return returnArgs;
            }

            bool switchGenerate = false;
            bool switchAdd = false;

            switch (args[0])
            {
                case "-h":
                case "--help":
                    PrintHelp();
                    break;
                case "-g":
                case "--generate":
                    if (args.Length > 1)
                        switchGenerate = true;
                    else
                        Console.WriteLine("Missing arguments.");
                    break;
                case "-a":
                case "--add":
                    if (args.Length > 1)
                        switchAdd = true;
                    else
                        Console.WriteLine("Missing arguments.");
                    break;
                case "--as-file":
                    if (args.Length > 1)
                    {
                        CBuild.AsFile = true;
                        returnArgs.Filepath = args[1];
                        if (args.Length > 2)
                            returnArgs.Project = args[2];
                    }
                    else
                        Console.WriteLine("Missing arguments.");
                    break;
                default:
                    returnArgs.Filepath = args[0];
                    if (args.Length > 1)
                        returnArgs.Project = args[1];
                    break;
            }

            if (switchGenerate)
            {
                switch (args[1])
                {
                    case "-h":
                    case "--help":
                        PrintHelpGenerate();
                        break;
                    default:
                        GenerateSolution(args[1]);
                        break;
                }
            }

            if (switchAdd)
            {
                switch (args[1])
                {
                    case "-h":
                    case "--help":
                        PrintHelpAdd();
                        break;
                    default:
                        AddProject(args[1]);
                        break;
                }
            }

            return returnArgs;
        }

        private static void GenerateSolution(string solutionName)
        {
            Directory.CreateDirectory(solutionName);
            GenerateFile($"{solutionName}/{solutionName}.cproj", solutionName);

            SolutionFile solutionFile = new SolutionFile();
            solutionFile.Projects = new List<ProjectInSolution>(){
                new ProjectInSolution() { Name = solutionName, Filepath = $"{solutionName}/{solutionName}.cproj" }
            };

            SerializerSettings settings = new SerializerSettings() { EmitAlias = false, EmitTags = false };
            Serializer serializer = new Serializer(settings);
            File.WriteAllText($"{solutionName}.csln", serializer.Serialize(solutionFile));

            Console.WriteLine("Solution generated successfully.");
        }

        private static void AddProject(string projectName)
        {
            Directory.CreateDirectory(projectName);
            GenerateFile($"{projectName}/{projectName}.cproj", projectName);

            string solutionFilepath = Directory.GetFiles(".", "*.csln")[0];

            SerializerSettings settings = new SerializerSettings() { EmitAlias = false, EmitTags = false };
            Serializer serializer = new Serializer(settings);

            SolutionFile solutionFile = serializer.Deserialize<SolutionFile>(File.ReadAllText(solutionFilepath));
            solutionFile.Projects.Add(new ProjectInSolution() {
                Filepath = $"{projectName}/{projectName}.cproj",
                Name = projectName
            });

            File.WriteAllText(solutionFilepath, serializer.Serialize(solutionFile));

            Console.WriteLine("Project added successfully.");
        }

        private static void GenerateFile(string filepath, string projectName)
        {
            Project project = new Project();
            project.ProjectName = projectName;
            project.OutputDir = "bin";
            project.ObjectDir = "obj";
            project.OutputType = "Application";
            project.Files = new string[] { "filename.c" };

            SerializerSettings settings = new SerializerSettings() { EmitAlias = false, EmitTags = false };
            Serializer serializer = new Serializer(settings);
            File.WriteAllText(filepath, serializer.Serialize(project));

            Console.WriteLine("CBuild file generated successfully.");
        }

        private static void PrintHelp()
        {
            Console.WriteLine("usage:           CBuild filepath");
            Console.WriteLine("                 CBuild [options]");
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine("-h, --help:      shows this page");
            Console.WriteLine("-g, --generate:  Generates a basic CBuild file");
            Console.WriteLine("-a, --add:       Add a project to the solution");
            Console.WriteLine("--as-file:       Generate a bat file instead of building the project");
        }

        private static void PrintHelpGenerate()
        {
            Console.WriteLine("usage:           CBuild --generate [options] solutionName");
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine("-h, --help:      shows this page");
        }

        private static void PrintHelpAdd()
        {
            Console.WriteLine("usage:           CBuild --add [options] projectName");
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine("-h, --help:      shows this page");
        }
    }
}
