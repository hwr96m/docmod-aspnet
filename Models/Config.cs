using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DocMod.Models {
    public class SourceConfig_t {
        public string Name;
        public string Dir;
        public string[] Ext;
    }

    public class Config {
        public static string SiteName { get; set; }
    }
}
