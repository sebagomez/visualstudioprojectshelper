using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VSRecentProjectsHelper
{
    public class VSVersion
    {
        public string Name { get; set; }
        public string RegEntry { get; set; }

        public VSVersion(string name, string reg)
        {
            this.Name = name;
            this.RegEntry = reg;
        }

        public static List<VSVersion> GetVersions()
        {
            List<VSVersion> list = new List<VSVersion>();

            VSVersion version9 = new VSVersion("2008", "9.0");
            VSVersion version8 = new VSVersion("2005", "8.0");
            VSVersion version7 = new VSVersion("2003", "7.1");


            list.Add(version9);
            list.Add(version8);
            list.Add(version7);

            return list;
        }
    }
}
