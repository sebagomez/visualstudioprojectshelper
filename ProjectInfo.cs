using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VSRecentProjectsHelper
{
    public class ProjectInfo
    {
        public string FullPath { get; set; }
        public string Name { get; set; }
        public string Extension { get; set; }
        public string Folder { get; set; }
        public string Entry { get; set; }

        public static ProjectInfo CreateProjectInfo(string entry, string regKey)
        {
            
                ProjectInfo proj = new ProjectInfo();

                string[] splited = entry.Split('|');

                if (splited.Length > 1)
                    proj.FullPath = splited[0];
                else
                    proj.FullPath = entry;

                if (proj.FullPath.Contains("."))
                {
                    proj.Extension = proj.FullPath.Substring(proj.FullPath.LastIndexOf('.') + 1);
                    proj.Name = proj.FullPath.Substring(proj.FullPath.LastIndexOf('\\') + 1, proj.FullPath.LastIndexOf('.') - (proj.FullPath.LastIndexOf('\\') + 1));
                    proj.Folder = proj.FullPath.Substring(0, proj.FullPath.LastIndexOf('\\'));
                }
                else if (proj.FullPath.StartsWith("http://"))
                {
                    proj.Extension = "folder";
                    string[] names = proj.FullPath.Split('/');
                    proj.Name = names[names.Length - 1];
                    proj.Folder = proj.FullPath;
                }
                else
                {
                    proj.Extension = "folder";
                    string[] names = proj.FullPath.Split('\\');
                    proj.Name = names[names.Length - 2];
                    proj.Folder = proj.FullPath.Substring(0, proj.FullPath.LastIndexOf('\\'));
                }

                
                proj.Entry = regKey;

                return proj;
          
        }
    }
}
