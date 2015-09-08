// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Internal;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Relational.Design.CodeGeneration
{
    public class CSharpCodeGeneratorHelper
    {
        public virtual void SingleLineComment([NotNull] string comment, [NotNull] IndentedStringBuilder sb)
        {
            Check.NotNull(comment, nameof(comment));
            Check.NotNull(sb, nameof(sb));

            sb.Append("// ");
            sb.AppendLine(comment);
        }

        public virtual void AddUsingStatement([NotNull] string @namespace, [NotNull] IndentedStringBuilder sb)
        {
            Check.NotEmpty(@namespace, nameof(@namespace));
            Check.NotNull(sb, nameof(sb));

            sb.Append("using ");
            sb.Append(@namespace);
            sb.AppendLine(";");
        }

        public virtual void BeginNamespace([NotNull] string classNamespace, [NotNull] IndentedStringBuilder sb)
        {
            Check.NotEmpty(classNamespace, nameof(classNamespace));
            Check.NotNull(sb, nameof(sb));

            sb.Append("namespace ");
            sb.AppendLine(classNamespace);
            sb.AppendLine("{");
            sb.IncrementIndent();
        }

        public virtual void EndNamespace([NotNull] IndentedStringBuilder sb)
        {
            Check.NotNull(sb, nameof(sb));

            sb.DecrementIndent();
            sb.AppendLine("}");
        }

        public virtual void BeginClass(AccessModifier accessModifier, [NotNull] string className,
            bool isPartial, [NotNull] IndentedStringBuilder sb, [CanBeNull] ICollection<string> inheritsFrom = null)
        {
            Check.NotEmpty(className, nameof(className));
            Check.NotNull(sb, nameof(sb));

            AppendAccessModifier(accessModifier, sb);
            if (isPartial)
            {
                sb.Append("partial ");
            }
            sb.Append("class ");
            sb.Append(className);
            if (inheritsFrom != null
                && inheritsFrom.Count > 0)
            {
                sb.Append(" : ");
                sb.Append(string.Join(", ", inheritsFrom));
            }
            sb.AppendLine();
            sb.AppendLine("{");
            sb.IncrementIndent();
        }

        public virtual void EndClass([NotNull] IndentedStringBuilder sb)
        {
            sb.DecrementIndent();
            sb.AppendLine("}");
        }

        public virtual void AddProperty(AccessModifier accessModifier, VirtualModifier virtualModifier,
            [NotNull] string propertyTypeName, [NotNull] string propertyName, [NotNull] IndentedStringBuilder sb)
        {
            Check.NotEmpty(propertyTypeName, nameof(propertyTypeName));
            Check.NotEmpty(propertyName, nameof(propertyName));
            Check.NotNull(sb, nameof(sb));

            AppendAccessModifier(accessModifier, sb);
            AppendVirtualModifier(virtualModifier, sb);
            sb.Append(propertyTypeName);
            sb.Append(" ");
            sb.Append(propertyName);
            sb.AppendLine(" { get; set; }");
        }

        public virtual void AddProperty(AccessModifier accessModifier, VirtualModifier virtualModifier,
            [NotNull] Type propertyType, [NotNull] string propertyName, [NotNull] IndentedStringBuilder sb)
        {
            Check.NotNull(propertyType, nameof(propertyType));
            Check.NotEmpty(propertyName, nameof(propertyName));
            Check.NotNull(sb, nameof(sb));

            AddProperty(accessModifier, virtualModifier, GetTypeName(propertyType), propertyName, sb);
        }

        private static readonly Dictionary<Type, string> _primitiveTypeNames = new Dictionary<Type, string>
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
            { typeof(decimal), "decimal" }
        };

        public virtual string GetTypeName([NotNull] Type propertyType)
        {
            Check.NotNull(propertyType, nameof(propertyType));

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

        public virtual void BeginConstructor(AccessModifier accessModifier, [NotNull] string className,
            [NotNull] IndentedStringBuilder sb, [CanBeNull] ICollection<Tuple<string, string>> parameters = null)
        {
            Check.NotEmpty(className, nameof(className));
            Check.NotNull(sb, nameof(sb));

            AppendAccessModifier(accessModifier, sb);
            sb.Append(className);
            sb.Append("(");
            if (parameters != null
                && parameters.Count > 0)
            {
                sb.Append(string.Join(", ", parameters.Select(tuple => tuple.Item1 + " " + tuple.Item2)));
            }
            sb.AppendLine(")");
            sb.AppendLine("{");
            sb.IncrementIndent();
        }

        public virtual void EndConstructor([NotNull] IndentedStringBuilder sb)
        {
            Check.NotNull(sb, nameof(sb));

            sb.DecrementIndent();
            sb.AppendLine("}");
        }

        public virtual void BeginMethod(AccessModifier accessModifier, VirtualModifier virtualModifier,
            [NotNull] string returnTypeName, [NotNull] string methodName, [NotNull] IndentedStringBuilder sb,
            [CanBeNull] ICollection<Tuple<string, string>> parameters = null)
        {
            Check.NotEmpty(returnTypeName, nameof(returnTypeName));
            Check.NotEmpty(methodName, nameof(methodName));
            Check.NotNull(sb, nameof(sb));

            AppendAccessModifier(accessModifier, sb);
            AppendVirtualModifier(virtualModifier, sb);
            sb.Append(returnTypeName);
            sb.Append(" ");
            sb.Append(methodName);
            sb.Append("(");
            if (parameters != null
                && parameters.Count > 0)
            {
                sb.Append(string.Join(", ", parameters.Select(tuple => tuple.Item1 + " " + tuple.Item2)));
            }
            sb.AppendLine(")");
            sb.AppendLine("{");
            sb.IncrementIndent();
        }

        public virtual void EndMethod([NotNull] IndentedStringBuilder sb)
        {
            Check.NotNull(sb, nameof(sb));

            sb.DecrementIndent();
            sb.AppendLine("}");
        }

        public virtual void AppendAccessModifier(
            AccessModifier accessModifier, [NotNull] IndentedStringBuilder sb)
        {
            Check.NotNull(sb, nameof(sb));

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

        public virtual void AppendVirtualModifier(
            VirtualModifier virtualModifier, [NotNull] IndentedStringBuilder sb)
        {
            Check.NotNull(sb, nameof(sb));

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

    public enum AccessModifier
    {
        Public,
        Private,
        Internal,
        Protected,
        ProtectedInternal
    }

    public enum VirtualModifier
    {
        Virtual,
        Override,
        New,
        None
    }
}
