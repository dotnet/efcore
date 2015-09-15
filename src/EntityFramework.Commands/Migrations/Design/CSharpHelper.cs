// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.Data.Entity.Design.Internal;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Migrations.Design
{
    public class CSharpHelper
    {
        private static readonly IDictionary<Type, string> _builtInTypes = new Dictionary<Type, string>
        {
            { typeof(bool), "bool" },
            { typeof(byte), "byte" },
            { typeof(sbyte), "sbyte" },
            { typeof(char), "char" },
            { typeof(short), "short" },
            { typeof(int), "int" },
            { typeof(long), "long" },
            { typeof(ushort), "ushort" },
            { typeof(uint), "uint" },
            { typeof(ulong), "ulong" },
            { typeof(decimal), "decimal" },
            { typeof(float), "float" },
            { typeof(double), "double" },
            { typeof(string), "string" },
            { typeof(object), "object" }
        };

        private static readonly IReadOnlyDictionary<Type, Func<CSharpHelper, object, string>> _literalFuncs =
            new Dictionary<Type, Func<CSharpHelper, object, string>>
            {
                { typeof(bool), (c, v) => c.Literal((bool)v) },
                { typeof(byte), (c, v) => c.Literal((byte)v) },
                { typeof(byte[]), (c, v) => c.Literal((byte[])v) },
                { typeof(char), (c, v) => c.Literal((char)v) },
                { typeof(DateTime), (c, v) => c.Literal((DateTime)v) },
                { typeof(DateTimeOffset), (c, v) => c.Literal((DateTimeOffset)v) },
                { typeof(decimal), (c, v) => c.Literal((decimal)v) },
                { typeof(double), (c, v) => c.Literal((double)v) },
                { typeof(float), (c, v) => c.Literal((float)v) },
                { typeof(Guid), (c, v) => c.Literal((Guid)v) },
                { typeof(int), (c, v) => c.Literal((int)v) },
                { typeof(long), (c, v) => c.Literal((long)v) },
                { typeof(sbyte), (c, v) => c.Literal((sbyte)v) },
                { typeof(short), (c, v) => c.Literal((short)v) },
                { typeof(string), (c, v) => c.Literal((string)v) },
                { typeof(TimeSpan), (c, v) => c.Literal((TimeSpan)v) },
                { typeof(uint), (c, v) => c.Literal((uint)v) },
                { typeof(ulong), (c, v) => c.Literal((ulong)v) },
                { typeof(ushort), (c, v) => c.Literal((ushort)v) }
            };

        public virtual string Lambda([NotNull] IReadOnlyList<string> properties)
        {
            Check.NotNull(properties, nameof(properties));

            var builder = new StringBuilder();
            builder.Append("x => ");

            if (properties.Count == 1)
            {
                builder.Append(Lambda(properties[0], "x"));
            }
            else
            {
                builder.Append("new { ");
                builder.Append(string.Join(", ", properties.Select(p => Lambda(p, "x"))));
                builder.Append(" }");
            }

            return builder.ToString();
        }

        public virtual string Lambda([NotNull] string property, [NotNull] string variable)
        {
            Check.NotEmpty(property, nameof(property));
            Check.NotEmpty(variable, nameof(variable));

            return variable + "." + property;
        }

        public virtual string Reference([NotNull] Type type)
        {
            Check.NotNull(type, nameof(type));

            if (type.IsConstructedGenericType
                && type.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                return Reference(type.UnwrapNullableType()) + "?";
            }
            if (type.IsArray)
            {
                return Reference(type.GetElementType()) + "[]";
            }
            if (type.IsNested)
            {
                return Reference(type.DeclaringType) + "." + type.Name;
            }

            string builtInType;
            if (_builtInTypes.TryGetValue(type, out builtInType))
            {
                return builtInType;
            }

            return type.Name;
        }

        public virtual string Identifier([NotNull] string name, [CanBeNull] ICollection<string> scope = null)
        {
            Check.NotEmpty(name, nameof(name));

            var builder = new StringBuilder();
            var partStart = 0;

            for (var i = 0; i < name.Length; i++)
            {
                if (!SyntaxFacts.IsIdentifierPartCharacter(name[i]))
                {
                    if (partStart != i)
                    {
                        builder.Append(name.Substring(partStart, i - partStart));
                    }

                    partStart = i + 1;
                }
            }

            if (partStart != name.Length)
            {
                builder.Append(name.Substring(partStart));
            }

            if (builder.Length == 0 || !SyntaxFacts.IsIdentifierStartCharacter(builder[0]))
            {
                builder.Insert(0, "_");
            }

            var identifier = builder.ToString();
            if (scope != null)
            {
                var uniqueIdentifier = identifier;
                var qualifier = 0;
                while (scope.Contains(uniqueIdentifier))
                {
                    uniqueIdentifier = identifier + qualifier++;
                }
                scope.Add(uniqueIdentifier);
                identifier = uniqueIdentifier;
            }

            if (SyntaxFacts.GetKeywordKind(identifier) != SyntaxKind.None
                || SyntaxFacts.GetPreprocessorKeywordKind(identifier) != SyntaxKind.None
                || SyntaxFacts.GetContextualKeywordKind(identifier) != SyntaxKind.None)
            {
                return "@" + identifier;
            }

            return identifier;
        }

        public virtual string Namespace([NotNull] params string[] name)
        {
            Check.NotNull(name, nameof(name));

            var @namespace = new StringBuilder();
            foreach (var piece in name.Where(p => !string.IsNullOrEmpty(p)).SelectMany(p => p.Split('.')))
            {
                var identifier = Identifier(piece);
                if (!string.IsNullOrEmpty(identifier))
                {
                    @namespace.Append(identifier)
                        .Append('.');
                }
            }
            return (@namespace.Length > 0) ? @namespace.Remove(@namespace.Length - 1, 1).ToString() : "_";
        }

        public virtual string Literal([NotNull] string value) =>
            value.Contains(Environment.NewLine)
                ? "@\"" + value.Replace("\"", "\"\"") + "\""
                : "\"" + value.Replace("\\", "\\\\").Replace("\"", "\\\"") + "\"";

        public virtual string Literal(bool value) => value ? "true" : "false";
        public virtual string Literal(byte value) => "(byte)" + value;

        public virtual string Literal([NotNull] byte[] values) =>
            "new byte[] { " + string.Join(", ", values) + " }";

        public virtual string Literal(char value) => "\'" + (value == '\'' ? "\\'" : value.ToString()) + "\'";

        public virtual string Literal(DateTime value) =>
            String.Format(
                "new DateTime({0}, {1}, {2}, {3}, {4}, {5}, {6}, DateTimeKind.{7})",
                value.Year, value.Month, value.Day, value.Hour, value.Minute, value.Second, value.Millisecond, value.Kind);

        public virtual string Literal(DateTimeOffset value) =>
            "new DateTimeOffset(" + Literal(value.DateTime) + ", " + Literal(value.Offset) + ")";

        public virtual string Literal(decimal value) => value.ToString(CultureInfo.InvariantCulture) + "m";
        public virtual string Literal(double value) => value.ToString("R", CultureInfo.InvariantCulture);
        public virtual string Literal(float value) => value.ToString(CultureInfo.InvariantCulture) + "f";
        public virtual string Literal(Guid value) => "new Guid(\"" + value + "\")";
        public virtual string Literal(int value) => value.ToString();
        public virtual string Literal(long value) => value + "L";
        public virtual string Literal(sbyte value) => "(sbyte)" + value;
        public virtual string Literal(short value) => "(short)" + value;

        public virtual string Literal(TimeSpan value) =>
            String.Format(
                "new TimeSpan({0}, {1}, {2}, {3}, {4})",
                value.Days, value.Hours, value.Minutes, value.Seconds, value.Milliseconds);

        public virtual string Literal(uint value) => value + "u";
        public virtual string Literal(ulong value) => value + "ul";
        public virtual string Literal(ushort value) => "(ushort)" + value;

        public virtual string Literal<T>([NotNull] T? value) where T : struct =>
            UnknownLiteral(value.Value);

        public virtual string Literal([NotNull] IReadOnlyList<string> values) =>
            values.Count == 1
                ? Literal(values[0])
                : "new[] { " + string.Join(", ", values.Select(Literal)) + " }";

        public virtual string Literal([NotNull] Enum value) => Reference(value.GetType()) + "." + value;

        public virtual string UnknownLiteral([CanBeNull] object value)
        {
            if (value == null)
            {
                return "null";
            }

            var type = value.GetType().UnwrapNullableType();

            Func<CSharpHelper, object, string> literalFunc;
            if (_literalFuncs.TryGetValue(type, out literalFunc))
            {
                return literalFunc(this, value);
            }

            var enumValue = value as Enum;
            if (enumValue != null)
            {
                return Literal(enumValue);
            }

            throw new InvalidOperationException(Strings.UnknownLiteral(value.GetType()));
        }
    }
}
