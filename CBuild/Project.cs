using SharpYaml.Serialization;

namespace CBuild
{
    class Project
    {
        [YamlMember(0)]
        public string ProjectName { get; set; }
        [YamlMember(1)]
        public string OutputDir { get; set; }
        [YamlMember(2)]
        public string ObjectDir { get; set; }
        [YamlMember(3)]
        public string OutputType { get; set; }
        [YamlMember(4)]
        public string[] Files { get; set; }
        [YamlMember(5)]
        public string[] IncludeDirs { get; set; }
        [YamlMember(6)]
        public string[] LibraryDirs { get; set; }
        [YamlMember(7)]
        public string[] Dependencies { get; set; }
        [YamlMember(8)]
        public string[] ProjectReferences { get; set; }  
        [YamlMember(9)]
        public string[] Preprocessors { get; set; }
        [YamlMember(10)]
        public bool CompilerWarnigns { get; set; } = true;
        [YamlMember(11)]
        public string Std { get; set; }
        [YamlMember(12)]
        public string OptimizationLevel { get; set; }

        [YamlIgnore]
        public string SolutionDir { get; set; }
        [YamlIgnore]
        public string Filepath { get; set; }
    }
}
