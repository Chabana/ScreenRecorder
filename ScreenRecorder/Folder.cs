﻿/*#################################################################################################
*Project : ScreenRecorder
*Developped by : Daniel de Carvalho Fernandes, Michael Caraccio & Khaled Chabbou
*Date : 27 April 2015
*#################################################################################################*/

using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Security;

namespace ScreenRecorder
{
    /// <summary>
    /// Class of the folder
    /// </summary>
    public class Folder 
    {
        private DirectoryInfo _folder;
        private ObservableCollection<Folder> _subFolders;
        private ObservableCollection<FileInfo> _files;

        /// <summary>
        /// Constructor
        /// </summary>
        public Folder()
        {
            FullPath = @"c:\ScreenRecorder";
        }

        /// <summary>
        /// Get name of the folder
        /// </summary>
        public string Name
        {
            get { return _folder.Name; }
        }

        /// <summary>
        /// Get/Set of the full path of the directory
        /// </summary>
        public string FullPath
        {
            get { return _folder.FullName; }

            set
            {
                //Create directory if doesn't exist
                if (Directory.Exists(value))
                {
                    try
                    {
                        _folder = new DirectoryInfo(value);
                    }
                    catch (SecurityException securityException)
                    {
                    }
                }
                else
                {
                    try
                    {
                        Directory.CreateDirectory(value);
                    }
                    catch (IOException ioException)
                    {
                    }
                    try
                    {
                        throw new ArgumentException(@"must exist", "fullPath");
                    }
                    catch (ArgumentException argumentException)
                    {
                    }
                }
            }
        }

        /// <summary>
        /// Get all the files
        /// </summary>
        public ObservableCollection<FileInfo> Files
        {
            get
            {
                if (_files == null)
                {
                    
                    _files = new ObservableCollection<FileInfo>();
                    string[] extensions = { "*.jpeg", "*.mp4", "*.wmv", "*.png", "*.bmp", "*.emf", "*.gif", "*.tiff", "*.exif" };

                    foreach (String extension in extensions)
                    {
                        FileInfo[] fi = _folder.GetFiles(extension, SearchOption.AllDirectories);

                        for (int i = 0; i < fi.Length; i++)
                        {
                            _files.Add(fi[i]);
                        }
                    }
                }

                return _files;
            }
        }

        /// <summary>
        /// If there is sub folders
        /// </summary>
        public ObservableCollection<Folder> SubFolders
        {
            get
            {
                if (_subFolders == null)
                {
                    _subFolders = new ObservableCollection<Folder>();

                    DirectoryInfo[] di = _folder.GetDirectories();

                    for (int i = 0; i < di.Length; i++)
                    {
                        Folder newFolder = new Folder();
                        newFolder.FullPath = di[i].FullName;
                        _subFolders.Add(newFolder);
                    }
                }

                return _subFolders;
            }
        }
    }
}