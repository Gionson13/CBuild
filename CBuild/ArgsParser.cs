using CBuild.Properties;
using System;
using System.IO;
using System.Text;

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
                    case "-f":
                    case "--full":
                        if (args.Length > 2)
                            GenerateSolution(args[2], true);
                        else
                            Console.WriteLine("Missing arguments.");
                        break;
                    default:
                        GenerateSolution(args[1], false);
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
                    case "-f":
                    case "--full":
                        if (args.Length > 2)
                            AddProject(args[2], true);
                        else
                            Console.WriteLine("Missing arguments.");
                        break;
                    default:
                        AddProject(args[1], false);
                        break;
                }
            }

            return returnArgs;
        }

        private static void GenerateSolution(string solutionName, bool full)
        {
            Directory.CreateDirectory(solutionName);
            GenerateFile($"{solutionName}/{solutionName}.cproj", solutionName, full);

            string solutionContent = Encoding.Default.GetString(Resources.solution).Replace("$(ProjectName)", solutionName);
            
            File.WriteAllText($"{solutionName}.csln", solutionContent);

            Console.WriteLine("Solution generated successfully.");
        }

        private static void AddProject(string projectName, bool full)
        {
            Directory.CreateDirectory(projectName);
            GenerateFile($"{projectName}/{projectName}.cproj", projectName, full);

            string solutionFile = Directory.GetFiles(".", "*.csln")[0];
            string solutionContent = $"\n  - {{ Name: {projectName}, Filepath: {projectName}/{projectName}.cproj }}";

            File.AppendAllText(solutionFile, solutionContent);
        }

        private static void GenerateFile(string filepath, string projectName, bool full)
        {
            string fileContent;
            if (full)
                fileContent = Encoding.Default.GetString(Resources.CBuild_full);
            else
                fileContent = Encoding.Default.GetString(Resources.CBuild);

            fileContent = fileContent.Replace("$(ProjectName)", projectName);

            File.WriteAllText(filepath, fileContent);

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
            Console.WriteLine("-f, --full:      Generates a full cproj file");
        }

        private static void PrintHelpAdd()
        {
            Console.WriteLine("usage:           CBuild --add [options] projectName");
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine("-h, --help:      shows this page");
            Console.WriteLine("-f, --full:      Generates a full cproj file");
        }
    }
}
