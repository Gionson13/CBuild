using System.Collections.Generic;
using System.Linq;

namespace CBuild.Core
{
    public class Solution
    {
        public List<Project> Projects = new List<Project>();

        public void Add(Project project)
        {
            Projects.Add(project);
        }

        public Project Get(string name)
        {
            return Projects.First(project => project.ProjectName == name);
        }

        public bool IsEmpty()
        {
            return Projects.Count == 0;
        }

    }
}
