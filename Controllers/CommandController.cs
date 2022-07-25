using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DocMod.Models;
using System.IO;
using Newtonsoft.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;

namespace DocMod.Controllers {

    public class CommandController : Controller {
        Info info;
        public CommandController(IWebHostEnvironment env) {
            info = new Info(env);
        }
        public IActionResult GetTree() {            
            return Content(info.GetTree());
        }
        public IActionResult GetContent(string path) {            
            return Content(info.GetContent(path));
        }
        public IActionResult GetHLStyles() {
            return Content(info.GetHLStyles());
        }
    }
}
