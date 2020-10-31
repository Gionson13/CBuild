using SharpYaml;
using SharpYaml.Serialization;
using System;
using System.Diagnostics;
using System.IO;

namespace CBuild
{
    class Program
    {
        static void Main(string[] args)
        {
            string filepath = ArgsParser.GetFilepath(args);

            if (string.IsNullOrWhiteSpace(filepath)) return;

            var serializer = new Serializer();
            BuildProperty buildProperties;
            try
            {
                buildProperties = serializer.Deserialize<BuildProperty>(File.ReadAllText(filepath));
            }
            catch (YamlException e)
            {
                Console.WriteLine("Parsing failed!");
                Console.WriteLine(e.Message);
                return;
            }

            string command = GenerateCommand(buildProperties);

            if (!Directory.Exists(buildProperties.OutputDir))
                Directory.CreateDirectory(buildProperties.OutputDir);

            Console.WriteLine(command);
            CallCommand(command);
            Console.WriteLine("Build Success!");
        }

        static void CallCommand(string command)
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
        }

        static string GenerateCommand(BuildProperty properties)
        {
            string command = "gcc";

            if (properties.CompilerWarnigns)
                command += " -Wall";

            // Std
            if (!string.IsNullOrWhiteSpace(properties.Std))
                command += $" -std={properties.Std}";

            // Include directories
            if (properties.IncludeDirs != null)
            {
                foreach (string includeDir in properties.IncludeDirs)
                    command += $" -I {includeDir}";
            }

            // Library directories
            if (properties.LibraryDirs != null)
            {
                foreach (string libraryDir in properties.LibraryDirs)
                    command += $" -L {libraryDir}";
            }

            // Preprocessors
            if (properties.Preprocessors != null)
            {
                foreach (string preprocessor in properties.Preprocessors)
                    command += $" -D {preprocessor}";
            }

            // Optimization
            if (!string.IsNullOrWhiteSpace(properties.OptimizationLevel))
                command += $" -O{properties.OptimizationLevel}";

            // Files
            foreach (string file in properties.Files)
                command += " " + file;
            
            // Dependencies
            if (properties.Dependencies != null)
            {
                foreach (string dependency in properties.Dependencies)
                    command += $" -l{dependency}";
            }
            
            // Output
            command += $" -o {properties.OutputDir}/{properties.ProjectName}.exe";

            return command;
        }
    }
}
