using SharpYaml;
using System;
using System.IO;
using System.Linq;

namespace CBuild.Core
{
    public class Serializer
    {
        private SharpYaml.Serialization.Serializer _serializer;

        public Serializer()
        {
            _serializer = new SharpYaml.Serialization.Serializer();
        }

        public Serializer(SharpYaml.Serialization.SerializerSettings settings)
        {
            _serializer = new SharpYaml.Serialization.Serializer(settings);
        }

        #region Deserialize

        public Solution DeserializeSolution(string path)
        {
            SolutionFile solutionFile = DeserializeSolutionFile(path);

            Solution solution = new Solution();
            foreach (ProjectInSolution project in solutionFile.Projects)
            {
                try
                {
                    Project toAdd = _serializer.Deserialize<Project>(File.ReadAllText(project.Filepath));
                    toAdd.Filepath = project.Filepath;
                    toAdd.CurrentConfiguration = toAdd.ProjectConfigurations[0];

                    solution.Add(toAdd);
                }
                catch (YamlException e)
                {
                    Log.Error(e.Message, project.Filepath, null);
                    return new Solution();
                }
                catch (InvalidOperationException)
                {
                    Log.Error("Project configuration not found.", project.Filepath, null);
                    return new Solution();
                }
            }

            return solution;
        }

        public Project DeserializeProject(string path)
        {
            Project project;

            try
            {
                project = _serializer.Deserialize<Project>(File.ReadAllText(path));
                project.Filepath = path;
                project.CurrentConfiguration = project.ProjectConfigurations[0];
            }
            catch (YamlException e)
            {
                Log.Error(e.Message, path, null);
                return new Project();
            }
            catch (InvalidOperationException)
            {
                Log.Error("Project configuration not found.", path, null);
                return new Project();
            }

            return project;
        }

        public Project DeserializeProject(string solutionPath, string projectName)
        {
            SolutionFile solutionFile = DeserializeSolutionFile(solutionPath);
            ProjectInSolution projectInSolution = solutionFile.Projects.First(project => project.Name == projectName);

            return DeserializeProject(projectInSolution.Filepath);
        }

        public SolutionFile DeserializeSolutionFile(string path)
        {
            SolutionFile solutionFile;

            try
            {
                solutionFile = _serializer.Deserialize<SolutionFile>(File.ReadAllText(path));
            }
            catch (YamlException e)
            {
                Log.Error(e.Message, path, null);
                return new SolutionFile();
            }

            Builder.SolutionFile = solutionFile;

            return solutionFile;
        }

        #endregion

        #region Serialize

        public void SerializeSolution(string path, Solution solution)
        {
            SolutionFile solutionFile = new SolutionFile();

            foreach (Project project in solution.Projects)
            {
                SerializeProject(project);

                solutionFile.Projects.Add(new ProjectInSolution() { Filepath = project.Filepath, Name = project.ProjectName });
            }

            SerializeSolutionFile(path, solutionFile);
        }

        public void SerializeProject(Project project)
        {
            SerializeProject(project.Filepath, project);
        }

        public void SerializeProject(string path, Project project)
        {
            File.WriteAllText(path, _serializer.Serialize(project));
        }

        public void SerializeSolutionFile(string path, SolutionFile solutionFile)
        {
            File.WriteAllText(path, _serializer.Serialize(solutionFile));
        }

        #endregion
    }
}
