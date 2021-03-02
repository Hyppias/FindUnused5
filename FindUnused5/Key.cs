#region FileHeader

// Solution FindUnused
// Project FindUnused
// Filename Keys.cs
// User Erik Terwiel
// Created 2021 03 01  17:00
// 
// <copyright file="Keys.cs" company="Terwiel">
// Copyright (c) E.H. Terwiel. All rights reserved.
// </copyright>
//
// Unauthorized copying of this file, via any medium and/or use it in any way
// is strictly prohibited, proprietary and confidential, unless you find it usefull.
// That's because I shamelessly cut-and-pasted (and adapted) various pieces of code from internet sources.
// Written by E.H. Terwiel <info@terwiel.com>, 2021

#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FindUnused
{
    public class Key : IEquatable<Key> , IComparable<Key>
    {
        public Key(string name)
        {
            Name = name;
            FileNames = new List<Uri>();
        }

        public List<Uri> FileNames;

        public string Name { get; set; }

        public int Occurrences { get; set; }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendLine($"{Occurrences} {Name}");
            foreach (var s in FileNames)
            {
                sb.AppendLine($"   {s.Segments.Last()}");
            }

            return sb.ToString();
        }

        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            Key objAsPart = obj as Key;
            if (objAsPart == null) return false;
            else return Equals(objAsPart);
        }
      
        // Default comparer for Part type.
        public int CompareTo( Key comparePart)
        {
            // A null value means that this object is greater.
            if (comparePart == null)
                return 1;

            else
                return this.Occurrences.CompareTo(comparePart.Occurrences);
        }
        public override int GetHashCode()
        {
            return Occurrences;
        }

        public bool Equals(Key other)
        {
            if (other == null) return false;
            return (this.Occurrences.Equals(other.Occurrences));
        }
    }
}
