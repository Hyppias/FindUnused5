#region FileHeader

// Solution FindUnused
// Project FindUnused
// Filename Program.cs
// User Erik Terwiel
// Created 2021 03 01  17:00
// 
// <copyright file="Program.cs" company="Terwiel">
// Copyright (c) E.H. Terwiel. All rights reserved.
// </copyright>
//
// Unauthorized copying of this file, via any medium and/or use it in any way
// is strictly prohibited, proprietary and confidential , unless you find it usefull.
// That's because I shamelessly cut-and-pasted (and adapted) various pieces of code from internet sources.
// Written by E.H. Terwiel <info@terwiel.com>, 2021

#endregion

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;

using System.Resources;
using System.Resources.Extensions;

namespace FindUnused
{
    class Program
    {
        public static List<Key> KeyList = new List<Key>();

        public static string root = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) +
            "/Visual Studio 2019/Projects/CableCalc5/CableCalc";

        public static string keyFile = root + "/Properties/Resources.resx";

        public static bool addQuotes;

        static void Main(string[] args)
        {
            KeyList.Clear();

            ReadKeys(keyFile);

            //foreach (string line in File.ReadLines(keyFile))
            //{
            //    KeyList.Add(new Key(line));
            //}

            addQuotes = true;
            WalkDirectoryTree(new DirectoryInfo(root), ".cs");
            addQuotes = false;
            WalkDirectoryTree(new DirectoryInfo(root), ".xaml");

            KeyList.Sort();
            var sb = new StringBuilder();
            foreach (var d in KeyList)
            {
                sb.Append(d.ToString());
                Debug.WriteLine(d.ToString());
            }
            File.WriteAllText(root + "/Occurences.txt", sb.ToString());
        }

        static void ReadKeys(string resource)
        {
            var rr = new ResourceReader(keyFile);
            IDictionaryEnumerator dict = rr.GetEnumerator();
            while (dict.MoveNext())
                Debug.WriteLine("{0}: {1}", dict.Key, dict.Value);   

            var xmlStr = File.ReadAllText(resource);

            var str = XElement.Parse(xmlStr);
            var r = str.Elements("data");

            var result = str.Elements("data").Where(b => b.LastAttribute.Value.Equals("preserve"));
            foreach (var e in result)
            {
                KeyList.Add(new Key(e.FirstAttribute.Value));
            }
        }

        public static int CountStringOccurrences(string text, string pattern)
        {
            // Loop through all instances of the string 'text'.
            int count = 0;
            int i = 0;

            while ((i = text.IndexOf(pattern, i, StringComparison.CurrentCulture)) != -1)
            {
                i += pattern.Length;
                count++;
            }

            return count;
        }

        private static List<string> _excludedDir = new List<string>()
            {"Properties", "bin", "obj", "CableData"};

        private static List<string> _excludedFile = new List<string>()
        {
            "ActivationVM.cs", "Converter.cs", "Converter.cs", "MathHelper.cs", "BuildHTML.cs",
            "Resources.Designer.cs"
        };

        static void WalkDirectoryTree(DirectoryInfo root, string extension)
        {
            IEnumerable<string> files = null;
            IEnumerable<DirectoryInfo> subDirs;

            // First, process all the files directly under this folder
            try
            {
                files = Directory.GetFiles(root.FullName).Where(b =>
                    !_excludedFile.Contains(b) && (b.EndsWith(extension)));
            }

            // This is thrown if even one of the files requires permissions greater
            // than the application provides.
            catch (UnauthorizedAccessException e)
            {
                // This code just writes out the message and continues to recurse.
                // You may decide to do something different here. For example, you
                // can try to elevate your privileges and access the file again.
                Debug.WriteLine(e.Message);
            }

            catch (DirectoryNotFoundException e)
            {
                Console.WriteLine(e.Message);
            }

            if (files.Any())
            {
                foreach (var fi in files)
                {
                    string content = File.ReadAllText(fi);
                    foreach (var k in KeyList)
                    {
                        int n = CountStringOccurrences(content,
                            addQuotes ? $"\"{k.Name}\"" : k.Name);
                        if (n > 0)
                        {
                            k.Occurrences += n;
                            k.FileNames.Add(new Uri(fi));
                        }
                    }
                }

                // Now find all the subdirectories under this directory.
                subDirs = root.GetDirectories().Where(b => !_excludedDir.Contains(b.Name));

                foreach (System.IO.DirectoryInfo dirInfo in subDirs)
                {
                    // Resursive call for each subdirectory.
                    WalkDirectoryTree(dirInfo, extension);
                }
            }
        }
    }
}
