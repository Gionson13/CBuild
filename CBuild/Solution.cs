using System.Collections.Generic;

namespace CBuild
{
    class Solution
    {
        public List<Project> Projects = new List<Project>();

        public void Add(Project project)
        {
            Projects.Add(project);
        }
    }
}
