using CBuild.Properties;
using System;
using System.IO;
using System.Text;

namespace CBuild
{
    static class ArgsParser
    {
        public static string GetFilepath(string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine("Missing arguments.");
                return "";
            }


            switch (args[0])
            {
                case "-h":
                case "--help":
                    PrintHelp();
                    break;
                case "-g":
                case "--generate":
                    if (args.Length > 1)
                        GenerateFile(args[1]);
                    else
                        GenerateFile(null);
                    break;
                default:
                    return args[0];
            }

            return "";

        }

        private static void PrintHelp()
        {
            Console.WriteLine("usage:           CBuild filepath");
            Console.WriteLine("                 CBuild [options]");
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine("-h, --help:      shows this page");
            Console.WriteLine("-g, --generate:  Generates a basic CBuild file");
        }

        private static void GenerateFile(string arg2)
        {
            if (arg2 != null && arg2 == "--full")
                File.WriteAllText("CBuild.yaml", Encoding.Default.GetString(Resources.CBuild_full));
            else
                File.WriteAllText("CBuild.yaml", Encoding.Default.GetString(Resources.CBuild));

            Console.WriteLine("File generated successfully.");
        }
    }
}
