using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DocMod.Models {
    public class Src_t {
        public string Name;
        public string Dir;
        public string[] Ext;
    }

    public class Config {
        public static string SiteName { get; set; }
        public static string IP { get; set; }
        public static string Port { get; set; }
    }
}
