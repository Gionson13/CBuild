using SharpYaml.Serialization;
using System.Collections.Generic;

namespace CBuild.Core
{
    public struct ProjectInSolution
    {
        [YamlMember(0)]
        public string Name { get; set; }
        [YamlMember(1)]
        public string Filepath { get; set; }
    }

    public struct SolutionFile
    {
        public List<ProjectInSolution> Projects { get; set; }
    }
}
