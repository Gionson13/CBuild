namespace CBuild
{
    class Project
    {
        public string ProjectName { get; set; }
        public string OutputDir { get; set; }
        public string ObjectDir { get; set; }
        public string OutputType { get; set; }
        public string[] Files { get; set; }
        public string[] IncludeDirs { get; set; }
        public string[] LibraryDirs { get; set; }
        public string[] Dependencies { get; set; }
        public string[] ProjectReferences { get; set; }  
        public string[] Preprocessors { get; set; }
        public string Std { get; set; }
        public string OptimizationLevel { get; set; }
        public bool CompilerWarnigns { get; set; } = true;
        public string SolutionDir { get; set; }
    }
}
