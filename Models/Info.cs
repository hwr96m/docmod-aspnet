using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;

namespace DocMod.Models {
    public class Info {
        //---- DATA --------------------------------
        struct Tree_t {
            public string text;
            public string state;
            public string icon;
            public List<Tree_t> children;
        }
        struct File_t {
            public string Name;
            public bool IsDir;
            public string Body;
        }
        struct Dir_t {
            public string Path;
            public List<File_t> Files;
        }
        IWebHostEnvironment env;
        List<Src_t> config = new List<Src_t>();
        //----- FUNC -------------------------------
        public Info(IWebHostEnvironment env) {
            this.env = env;
            var f = System.IO.File.ReadAllText("appsettings.json");
            dynamic DynamicObj = JsonConvert.DeserializeObject(f);
            string s = DynamicObj.Src.ToString();
            config = JsonConvert.DeserializeObject<List<Src_t>>(s);
        }
        public string GetTree() {
            var tree = new List<Tree_t>();
            foreach(var source in config) {
                var d = new Tree_t();
                //добавляем корневую директорию
                d.text = source.Name;
                d.state = "{\"opened\": false}";
                d.icon = "";
                d.children = DirList(Path.GetFullPath(source.Dir), source.Ext);//рекурсивно обходим остальные директории
                tree.Add(d);
            }
            var json = JsonConvert.SerializeObject(tree);
            return json;
        }
        private List<Tree_t> DirList(string path, string[] Ext) {
            var tree = new List<Tree_t>();
            //составляем список папок, в которых есть файлы с нужным расширением
            var dirs = Directory.GetDirectories(path)
                .Where(folder => {
                    int count = Directory.GetFiles(folder, "*.*", SearchOption.AllDirectories)
                    .Where(file => {
                        return IsNeededExt(file, Ext);
                    })
                    .Count();
                    return (count > 0) ? true : false;
                })
                .ToList();

            var files = Directory.GetFiles(path)
                    .Where(file => {
                        return IsNeededExt(file, Ext);
                    })
                    .ToList();
            //составляем список папок
            foreach(var dir in dirs) {
                var d = new Tree_t();
                d.text = new DirectoryInfo(dir).Name;
                d.state = "{\"opened\": false}";
                d.icon = "";
                d.children = DirList(Path.GetFullPath(dir), Ext);
                tree.Add(d);
            }
            //составляем список файлов
            foreach(var file in files) {
                var f = new Tree_t();
                f.text = Path.GetFileName(file);
                f.state = "{\"opened\": false}";
                f.icon = "jstree-file";
                tree.Add(f);
            }
            return tree;
        }
        [HttpPost]
        public string GetContent(string path) {
            var dir = new Dir_t();
            var defaultSeparator = '/';
            path = path.Replace(defaultSeparator, Path.DirectorySeparatorChar);// меняем разделители в строке на те, что в ОС
            var pathArr = path.Split(Path.DirectorySeparatorChar);// разделяем путь по разделителю текущей ОС
            //составляем путь к папке на сервере
            var currentConfigSrc = config.Find(i => i.Name == pathArr[0]);          //конфиг источника данных
            var fullPath = Path.Join(currentConfigSrc.Dir, Path.Join(pathArr[1..])); // вместо псевдонима подставляем полный путь (полный путь к корневой директории, путь к нужной папке)
            //проверка наличия директории
            if(System.IO.File.Exists(Path.Join(fullPath))) {//если pathArr указывает на файл
                pathArr = pathArr[..^1];         //удаляем последний элемент
                fullPath = Path.Join(currentConfigSrc.Dir, Path.Join(pathArr[1..]));// вместо псевдонима подставляем полный путь (полный путь к корневой директории, путь к нужной папке)
            }
            if(!Directory.Exists(Path.Join(fullPath))) { //директория не существует
                return null;
            }
            //подготавливаем структуру данных для ответа
            dir.Path = fullPath.Replace(currentConfigSrc.Dir, currentConfigSrc.Name).Replace(Path.DirectorySeparatorChar, defaultSeparator);//заменяем путь к каталогу на псевдоним, заменяем \ на /
            //составляем список папок из текущей папки, в которых есть файлы с нужным расширением
            var folders = Directory.GetDirectories(fullPath)
                    .Where(folder => {
                        int count = Directory.GetFiles(folder, "*.*", SearchOption.AllDirectories)
                        .Where(file => {
                            return IsNeededExt(file, currentConfigSrc.Ext);
                        })
                        .Count();
                        return (count > 0) ? true : false;
                    })
                    .ToList();
            //составляем список файлов
            var files = Directory.GetFiles(fullPath)
                    .Where(file => {
                        return IsNeededExt(file, currentConfigSrc.Ext);
                    })
                    .ToList();
            //упаковываем в структуру
            dir.Files = new List<File_t>(folders.Count + files.Count);
            foreach(var filePath in folders) {    //добавляем папки
                dir.Files.Add(new File_t() { Name = Path.GetFileName(filePath), IsDir = true, Body = "" }); ;
            }
            foreach(var filePath in files) {    //добавляем файлы
                dir.Files.Add(new File_t() { Name = Path.GetFileName(filePath), IsDir = false, Body = System.IO.File.ReadAllText(filePath) }); ;
            }
            //конвертируем в json
            var json = JsonConvert.SerializeObject(dir);
            return json;
        }
        public string GetHLStyles() {
            var CssExt = ".css";
            var HLStylesPath = Path.Join(env.WebRootPath, "lib", "highlight", "styles");
            //поиск стилей в HLStylesPath
            var HLStylesFiles = Directory.GetFiles(HLStylesPath)
                    .Where(file => {
                        return IsNeededExt(file, new string[] { CssExt });
                    })
                    .ToList();
            //меняем путь для отображения на сайте
            for(int i = 0; i < HLStylesFiles.Count; i++) {
                HLStylesFiles[i] = HLStylesFiles[i].Replace(HLStylesPath, Path.Join("lib", "highlight", "styles")).Replace(@"\", "/");
            }
            //конвертируем в json
            var json = JsonConvert.SerializeObject(HLStylesFiles);
            return json;
        }


        private bool IsNeededExt(string file, string[] Ext) {
            foreach(var ext in Ext) {
                if(file.ToLower().EndsWith(ext))
                    return true;
            }
            return false;
        }
    }
}
