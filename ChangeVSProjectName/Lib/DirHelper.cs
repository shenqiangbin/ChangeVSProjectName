using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ChangeVSProjectName.Lib
{
    public class DirReplaceHelper
    {
        private string _dirPath;
        private List<string> _extensions = new List<string>();

        public DirReplaceHelper(string dirPath)
        {
            _dirPath = dirPath;
            _extensions.AddRange(new string[] { ".doc", ".docx", ".xls", ".xlsx", ".ppt", ".pptx" });
        }

        public List<string> GetFileNames()
        {
            List<string> fileNames = new List<string>();

            DirectoryInfo dir = new DirectoryInfo(_dirPath);
            FileInfo[] files = dir.GetFiles("*.*", SearchOption.AllDirectories);
            foreach (var file in files)
            {
                if (_extensions.Contains(file.Extension.ToLower()))
                    fileNames.Add(file.Name);
            }

            return fileNames;
        }

    }
}
