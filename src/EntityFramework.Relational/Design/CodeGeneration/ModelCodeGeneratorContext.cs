// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Relational.Design.CodeGeneration
{
    public abstract class ModelCodeGeneratorContext
    {
        public static readonly List<string> DefaultNamespaces = new List<string>()
                {
                    "System",
                    "Microsoft.Data.Entity",
                    "Microsoft.Data.Entity.Metadata"
                };


        public abstract string ClassNamespace { get; }

        public abstract string ClassName { get; }

        public string[] GetClassInheritsFrom()
        {
            return new string[0];
        }


        //
        // Generate Methods
        //

        public virtual void GenerateCommentHeader(IndentedStringBuilder sb)
        {
        }

        public virtual void GenerateClass(IndentedStringBuilder sb)
        {
            sb.Append("public class ");
            sb.Append(ClassName);
            GenerateClassInheritsFrom(sb);
            sb.AppendLine();
            sb.AppendLine("{");
            using (sb.Indent())
            {
                GenerateConstants(sb);
                GenerateConstructors(sb);
                GenerateFields(sb);
                GenerateProperties(sb);
                GenerateMethods(sb);
            }
            sb.AppendLine("}");
        }

        /// <summary>
        /// Appends a string of the form " : InheritsFrom1, InheritsFrom2 ..." if necessary
        /// </summary>
        public virtual void GenerateClassInheritsFrom(IndentedStringBuilder sb)
        {
        }

        /// <summary>
        /// Appends lines of the form:
        ///     private const string MyString = "";
        ///     public static readonly int MyInt = "";
        /// if necessary.
        /// </summary>
        public virtual void GenerateConstants(IndentedStringBuilder sb)
        {
        }

        public virtual void GenerateConstructors(IndentedStringBuilder sb)
        {
        }

        public virtual void GenerateFields(IndentedStringBuilder sb)
        {
        }

        public virtual void GenerateProperties(IndentedStringBuilder sb)
        {
        }

        public virtual void GenerateMethods(IndentedStringBuilder sb)
        {
        }


        //
        // helper methods
        //
        public virtual IEnumerable<string> GetUsedNamespaces()
        {
            return DefaultNamespaces;
        }
    }
}