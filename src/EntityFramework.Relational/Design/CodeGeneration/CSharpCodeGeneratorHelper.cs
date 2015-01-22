// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Relational.Design.CodeGeneration
{
    public class CSharpCodeGeneratorHelper
    {
        private static readonly CSharpCodeGeneratorHelper _instance = new CSharpCodeGeneratorHelper();

        public static CSharpCodeGeneratorHelper Instance
        {
            get
            {
                return _instance;
            }
        }

        public virtual void Comment(IndentedStringBuilder sb, string comment)
        {
            sb.Append("// ");
            sb.AppendLine(comment);
        }

        public virtual void AddUsingStatement(IndentedStringBuilder sb, string @namespace)
        {
            sb.Append("using ");
            sb.Append(@namespace);
            sb.AppendLine(";");
        }

        public virtual void BeginNamespace(IndentedStringBuilder sb, string classNamespace)
        {
            sb.Append("namespace ");
            sb.AppendLine(classNamespace);
            sb.AppendLine("{");
            sb.IncrementIndent();
        }

        public virtual void EndNamespace(IndentedStringBuilder sb)
        {
            sb.DecrementIndent();
            sb.AppendLine("}");
        }

        public virtual void BeginPublicPartialClass(IndentedStringBuilder sb, string className, ICollection<string> inheritsFrom = null)
        {
            sb.Append("public partial class ");
            sb.Append(className);
            if (inheritsFrom != null && inheritsFrom.Count > 0)
            {
                sb.Append(" : ");
                sb.Append(string.Join(", ", inheritsFrom));
            }
            sb.AppendLine();
            sb.AppendLine("{");
            sb.IncrementIndent();
        }

        public virtual void EndClass(IndentedStringBuilder sb)
        {
            sb.DecrementIndent();
            sb.AppendLine("}");
        }

        public virtual void AddPublicVirtualProperty(IndentedStringBuilder sb, string propertyTypeName, string propertyName)
        {
            sb.Append("public virtual ");
            sb.Append(propertyTypeName);
            sb.Append(" ");
            sb.Append(propertyName);
            sb.AppendLine(" { get; set; }");
        }

        public virtual void BeginProtectedOverrideMethod(IndentedStringBuilder sb, string returnType, string methodName, ICollection<Tuple<string, string>> parameters = null)
        {
            sb.Append("protected override ");
            sb.Append(returnType);
            sb.Append(" ");
            sb.Append(methodName);
            sb.Append("(");
            if (parameters != null && parameters.Count > 0)
            {
                sb.Append(string.Join(", ", parameters.Select(tuple => tuple.Item1 + " " + tuple.Item2)));
            }
            sb.AppendLine(")");
            sb.AppendLine("{");
            sb.IncrementIndent();
        }

        public virtual void EndMethod(IndentedStringBuilder sb)
        {
            sb.DecrementIndent();
            sb.AppendLine("}");
        }
    }
}