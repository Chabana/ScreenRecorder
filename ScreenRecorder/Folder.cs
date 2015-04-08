using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using System.Windows.Controls;

namespace ScreenRecorder
{
    public class Folder
    {
        private DirectoryInfo _folder;
        private ObservableCollection<Folder> _subFolders;
        private ObservableCollection<FileInfo> _files;

        public Folder()
        {
            


        }

        public string Name
        {
            get { return this._folder.Name; }
        }

        public string FullPath
        {
            get { return this._folder.FullName; }

            set
            {
                if (Directory.Exists(value))
                {
                    this._folder = new DirectoryInfo(value);
                }
                else
                {
                    Directory.CreateDirectory(value);
                    throw new ArgumentException(@"must exist", "fullPath");
                }
            }
        }

        public ObservableCollection<FileInfo> Files
        {
            get
            {
                if (this._files == null)
                {
                    
                    this._files = new ObservableCollection<FileInfo>();
                    string[] extensions = { "*.jpg", "*.txt", "*.asp", "*.css", "*.avi", "*.xml" };

                    foreach (String extension in extensions)
                    {
                        FileInfo[] fi = this._folder.GetFiles(extension, SearchOption.AllDirectories);

                        for (int i = 0; i < fi.Length; i++)
                        {
                            this._files.Add(fi[i]);
                        }
                    }
                }

                return this._files;
            }
        }

        public ObservableCollection<Folder> SubFolders
        {
            get
            {
                if (this._subFolders == null)
                {
                    this._subFolders = new ObservableCollection<Folder>();

                    DirectoryInfo[] di = this._folder.GetDirectories();

                    for (int i = 0; i < di.Length; i++)
                    {
                        Folder newFolder = new Folder();
                        newFolder.FullPath = di[i].FullName;
                        this._subFolders.Add(newFolder);
                    }
                }

                return this._subFolders;
            }
        }
    }
}
