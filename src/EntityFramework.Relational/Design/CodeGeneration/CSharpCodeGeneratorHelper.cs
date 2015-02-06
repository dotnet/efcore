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

        public virtual void SingleLineComment(IndentedStringBuilder sb, string comment)
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

        public virtual void BeginClass(IndentedStringBuilder sb, AccessModifier accessModifier,
            string className, bool isPartial, ICollection<string> inheritsFrom = null)
        {
            AppendAccessModifier(sb, accessModifier);
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

        public virtual void AddProperty(IndentedStringBuilder sb, AccessModifier accessModifier,
            VirtualModifier virtualModifier, string propertyTypeName, string propertyName)
        {
            AppendAccessModifier(sb, accessModifier);
            AppendVirtualModifier(sb, virtualModifier);
            sb.Append(propertyTypeName);
            sb.Append(" ");
            sb.Append(propertyName);
            sb.AppendLine(" { get; set; }");
        }

        public virtual void AddProperty(IndentedStringBuilder sb, AccessModifier accessModifier,
            VirtualModifier virtualModifier, Type propertyType, string propertyName)
        {
            AddProperty(sb, accessModifier, virtualModifier, GetTypeName(propertyType), propertyName);
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
            //TODO workaround for property.PropertyType.IsGenericType being missing in ASPNETCORE50
            bool isNullableType;
            try
            {
                isNullableType = (typeof(Nullable<>) == propertyType.GetGenericTypeDefinition());
            }
            catch (InvalidOperationException)
            {
                isNullableType = false;
            }

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

        public virtual void BeginMethod(IndentedStringBuilder sb, AccessModifier accessModifier,
            VirtualModifier virtualModifier, string returnType, string methodName, ICollection<Tuple<string, string>> parameters = null)
        {
            AppendAccessModifier(sb, accessModifier);
            AppendVirtualModifier(sb, virtualModifier);
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

        public void AppendAccessModifier(IndentedStringBuilder sb, AccessModifier accessModifier)
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

        public void AppendVirtualModifier(IndentedStringBuilder sb, VirtualModifier virtualModifier)
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