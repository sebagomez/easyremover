using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace Program_Finder
{
    class Entry
    {
        public Entry(string name)
        {
            Name = name;
        }

        public string Description { get; set; }
        public string Name { get; set; }
        public string Version { get; set; }
        public string Publisher { get; set; }
        public string Source { get; set; }
        public string Unisntall { get; set; }
        public DateTime InstallationDate { get; set; }
        public Icon LargeIcon { get; set; }
        public Icon SmallIcon { get; set; }
        public bool IsUpdate { get; set; }
        public string Help { get; set; }
    }
}
