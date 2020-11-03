using SharpYaml.Serialization;
using System;
using System.Collections.Generic;
using System.Text;

namespace CBuild
{
    class ProjectConfiguration
    {
        [YamlMember(0)]
        public string Configuration { get; set; }
        [YamlMember(1)]
        public string Platform { get; set; }
        [YamlMember(2)]
        public string OutputType { get; set; }
        [YamlMember(3)]
        public string[] Preprocessors { get; set; }
        [YamlMember(4)]
        public string Std { get; set; }
        [YamlMember(5)]
        public string OptimizationLevel { get; set; }
        [YamlMember(6)]
        public bool CompilerWarnigns { get; set; } = true;
    }
}
