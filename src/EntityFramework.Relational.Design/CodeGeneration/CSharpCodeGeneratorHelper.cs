// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Relational.Design.CodeGeneration
{
    public class CSharpCodeGeneratorHelper
    {
        public virtual void SingleLineComment(string comment, IndentedStringBuilder sb)
        {
            sb.Append("// ");
            sb.AppendLine(comment);
        }

        public virtual void AddUsingStatement(string @namespace, IndentedStringBuilder sb)
        {
            sb.Append("using ");
            sb.Append(@namespace);
            sb.AppendLine(";");
        }

        public virtual void BeginNamespace(string classNamespace, IndentedStringBuilder sb)
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

        public virtual void BeginClass(AccessModifier accessModifier, string className,
            bool isPartial, IndentedStringBuilder sb, ICollection<string> inheritsFrom = null)
        {
            AppendAccessModifier(accessModifier, sb);
            if (isPartial)
            {
                sb.Append("partial ");
            }
            sb.Append("class ");
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

        public virtual void AddProperty(AccessModifier accessModifier, VirtualModifier virtualModifier,
            string propertyTypeName, string propertyName, IndentedStringBuilder sb)
        {
            AppendAccessModifier(accessModifier, sb);
            AppendVirtualModifier(virtualModifier, sb);
            sb.Append(propertyTypeName);
            sb.Append(" ");
            sb.Append(propertyName);
            sb.AppendLine(" { get; set; }");
        }

        public virtual void AddProperty(AccessModifier accessModifier, VirtualModifier virtualModifier,
            Type propertyType, string propertyName, IndentedStringBuilder sb)
        {
            AddProperty(accessModifier, virtualModifier, GetTypeName(propertyType), propertyName, sb);
        }

        private static Dictionary<Type, string> _primitiveTypeNames = new Dictionary<Type, string>()
            {
                { typeof(bool), "bool" },
                { typeof(byte), "byte" },
                { typeof(byte[]), "byte[]" },
                { typeof(sbyte), "sbyte" },
                { typeof(short), "short" },
                { typeof(ushort), "ushort" },
                { typeof(int), "int" },
                { typeof(uint), "uint" },
                { typeof(long), "long" },
                { typeof(ulong), "ulong" },
                { typeof(char), "char" },
                { typeof(float), "float" },
                { typeof(double), "double" },
                { typeof(string), "string" },
                { typeof(decimal), "decimal" },
            };
        private string GetTypeName(Type propertyType)
        {
            var isNullableType = propertyType.GetTypeInfo().IsGenericType
                && typeof(Nullable<>) == propertyType.GetGenericTypeDefinition();
            var type = isNullableType
                ? Nullable.GetUnderlyingType(propertyType)
                : propertyType;

            string typeName;
            if (!_primitiveTypeNames.TryGetValue(type, out typeName))
            {
                typeName = type.Name;
            }

            if (isNullableType)
            {
                typeName += "?";
            }

            return typeName;
        }

        public virtual void BeginConstructor(AccessModifier accessModifier, string className,
            IndentedStringBuilder sb, ICollection<Tuple<string, string>> parameters = null)
        {
            AppendAccessModifier(accessModifier, sb);
            sb.Append(className);
            sb.Append("(");
            if (parameters != null && parameters.Count > 0)
            {
                sb.Append(string.Join(", ", parameters.Select(tuple => tuple.Item1 + " " + tuple.Item2)));
            }
            sb.AppendLine(")");
            sb.AppendLine("{");
            sb.IncrementIndent();
        }

        public virtual void EndConstructor(IndentedStringBuilder sb)
        {
            sb.DecrementIndent();
            sb.AppendLine("}");
        }

        public virtual void BeginMethod(AccessModifier accessModifier, VirtualModifier virtualModifier,
            string returnType, string methodName, IndentedStringBuilder sb, ICollection<Tuple<string, string>> parameters = null)
        {
            AppendAccessModifier(accessModifier, sb);
            AppendVirtualModifier(virtualModifier, sb);
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

        public void AppendAccessModifier(AccessModifier accessModifier, IndentedStringBuilder sb)
        {
            switch (accessModifier)
            {
                case AccessModifier.Public:
                    sb.Append("public ");
                    break;
                case AccessModifier.Private:
                    sb.Append("private ");
                    break;
                case AccessModifier.Internal:
                    sb.Append("internal ");
                    break;
                case AccessModifier.Protected:
                    sb.Append("protected ");
                    break;
                case AccessModifier.ProtectedInternal:
                    sb.Append("protected internal ");
                    break;
            }
        }

        public void AppendVirtualModifier(VirtualModifier virtualModifier, IndentedStringBuilder sb)
        {
            switch (virtualModifier)
            {
                case VirtualModifier.Virtual:
                    sb.Append("virtual ");
                    break;
                case VirtualModifier.Override:
                    sb.Append("override ");
                    break;
                case VirtualModifier.New:
                    sb.Append("new ");
                    break;
            }
        }
    }

    public enum AccessModifier : int
    {
        Public,
        Private,
        Internal,
        Protected,
        ProtectedInternal
    }

    public enum VirtualModifier : int
    {
        Virtual,
        Override,
        New,
        None
    }
}