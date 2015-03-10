// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Commands.Utilities
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
                return type.Name.Replace('+', '.');
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

            if (!SyntaxFacts.IsIdentifierStartCharacter(builder[0]))
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

        public virtual string Literal([NotNull] string value) =>
            value.Contains(Environment.NewLine)
                ? "@\"" + value.Replace("\"", "\"\"") + "\""
                : "\"" + value.Replace("\\", "\\\\").Replace("\"", "\\\"") + "\"";

        public virtual string Literal(bool value) => value ? "true" : "false";
        public virtual string Literal(byte value) => "(byte)" + value;

        public virtual string Literal([NotNull] byte[] values) =>
            "new byte[] { " + string.Join(", ", values) + " }";

        public virtual string Literal(char value) => "\'" + (value == '\'' ? "\\'" : value.ToString()) + "\'";
        public virtual string Literal(DateTime value) => "DateTime.Parse(\"" + value + "\")";
        public virtual string Literal(DateTimeOffset value) => "DateTimeOffset.Parse(\"" + value + "\")";
        public virtual string Literal(decimal value) => value + "m";
        public virtual string Literal(double value) => value.ToString();
        public virtual string Literal(float value) => value + "f";
        public virtual string Literal(Guid value) => "new Guid(\"" + value + "\")";
        public virtual string Literal(int value) => value.ToString();
        public virtual string Literal(long value) => value + "L";
        public virtual string Literal(sbyte value) => "(sbyte)" + value;
        public virtual string Literal(short value) => "(short)" + value;
        public virtual string Literal(TimeSpan value) => "TimeSpan.Parse(\"" + value + "\")";
        public virtual string Literal(uint value) => value + "u";
        public virtual string Literal(ulong value) => value + "ul";
        public virtual string Literal(ushort value) => "(ushort)" + value;

        public virtual string Literal<T>([NotNull] T? value) where T : struct =>
            Literal((dynamic)value.Value);

        public virtual string Literal([NotNull] IReadOnlyList<string> values) =>
            values.Count == 1
                ? Literal(values[0])
                : "new[] { " + string.Join(", ", values.Select(Literal)) + " }";

        public virtual string Literal([NotNull] IDictionary<string, string> values) =>
            "new Dictionary<string, string> { " + string.Join(", ", values.Select(Literal)) + " }";

        public virtual string Literal([NotNull] KeyValuePair<string, string> value) =>
            "{ " + Literal(value.Key) + ", " + Literal(value.Value) + " }";

        public virtual string Literal([NotNull] object value)
        {
            Check.NotNull(value, nameof(value));

            throw new InvalidOperationException(Strings.UnknownLiteral(value.GetType()));
        }
    }
}
