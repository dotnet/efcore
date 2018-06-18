// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Design.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class CSharpHelper : ICSharpHelper
    {
        private static readonly IReadOnlyDictionary<Type, string> _builtInTypes = new Dictionary<Type, string>
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

        private static readonly IReadOnlyCollection<string> _keywords = new[]
        {
            "__arglist",
            "__makeref",
            "__reftype",
            "__refvalue",
            "abstract",
            "as",
            "base",
            "bool",
            "break",
            "byte",
            "case",
            "catch",
            "char",
            "checked",
            "class",
            "const",
            "continue",
            "decimal",
            "default",
            "delegate",
            "do",
            "double",
            "else",
            "enum",
            "event",
            "explicit",
            "extern",
            "false",
            "finally",
            "fixed",
            "float",
            "for",
            "foreach",
            "goto",
            "if",
            "implicit",
            "in",
            "int",
            "interface",
            "internal",
            "is",
            "lock",
            "long",
            "namespace",
            "new",
            "null",
            "object",
            "operator",
            "out",
            "override",
            "params",
            "private",
            "protected",
            "public",
            "readonly",
            "ref",
            "return",
            "sbyte",
            "sealed",
            "short",
            "sizeof",
            "stackalloc",
            "static",
            "string",
            "struct",
            "switch",
            "this",
            "throw",
            "true",
            "try",
            "typeof",
            "uint",
            "ulong",
            "unchecked",
            "unsafe",
            "ushort",
            "using",
            "virtual",
            "void",
            "volatile",
            "while"
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
                { typeof(NestedClosureCodeFragment), (c, v) => c.Fragment((NestedClosureCodeFragment)v) },
                { typeof(object[]), (c, v) => c.Literal((object[])v) },
                { typeof(object[,]), (c, v) => c.Literal((object[,])v) },
                { typeof(sbyte), (c, v) => c.Literal((sbyte)v) },
                { typeof(short), (c, v) => c.Literal((short)v) },
                { typeof(string), (c, v) => c.Literal((string)v) },
                { typeof(TimeSpan), (c, v) => c.Literal((TimeSpan)v) },
                { typeof(uint), (c, v) => c.Literal((uint)v) },
                { typeof(ulong), (c, v) => c.Literal((ulong)v) },
                { typeof(ushort), (c, v) => c.Literal((ushort)v) }
            };

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual string Lambda(IReadOnlyList<string> properties)
        {
            Check.NotNull(properties, nameof(properties));

            var builder = new StringBuilder();
            builder.Append("x => ");

            if (properties.Count == 1)
            {
                builder
                    .Append("x.")
                    .Append(properties[0]);
            }
            else
            {
                builder.Append("new { ");
                builder.Append(string.Join(", ", properties.Select(p => "x." + p)));
                builder.Append(" }");
            }

            return builder.ToString();
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual string Reference(Type type)
        {
            Check.NotNull(type, nameof(type));

            if (_builtInTypes.TryGetValue(type, out var builtInType))
            {
                return builtInType;
            }

            if (type.IsConstructedGenericType
                && type.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                return Reference(type.UnwrapNullableType()) + "?";
            }

            var builder = new StringBuilder();

            if (type.IsArray)
            {
                builder
                    .Append(Reference(type.GetElementType()))
                    .Append("[");

                var rank = type.GetArrayRank();
                for (var i = 1; i < rank; i++)
                {
                    builder.Append(",");
                }

                builder.Append("]");

                return builder.ToString();
            }

            if (type.IsNested)
            {
                Debug.Assert(type.DeclaringType != null);
                builder
                    .Append(Reference(type.DeclaringType))
                    .Append(".");
            }

            builder.Append(type.ShortDisplayName());

            return builder.ToString();
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual string Identifier(string name, ICollection<string> scope = null)
        {
            Check.NotEmpty(name, nameof(name));

            var builder = new StringBuilder();
            var partStart = 0;

            for (var i = 0; i < name.Length; i++)
            {
                if (!IsIdentifierPartCharacter(name[i]))
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

            if (builder.Length == 0
                || !IsIdentifierStartCharacter(builder[0]))
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

            return _keywords.Contains(identifier) ? "@" + identifier : identifier;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual string Namespace(params string[] name)
        {
            Check.NotNull(name, nameof(name));

            var @namespace = new StringBuilder();
            foreach (var piece in name.Where(p => !string.IsNullOrEmpty(p)).SelectMany(p => p.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries)))
            {
                var identifier = Identifier(piece);
                if (!string.IsNullOrEmpty(identifier))
                {
                    @namespace.Append(identifier)
                        .Append('.');
                }
            }

            return @namespace.Length > 0 ? @namespace.Remove(@namespace.Length - 1, 1).ToString() : "_";
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual string Literal(string value) =>
            value.Contains(Environment.NewLine)
                ? "@\"" + value.Replace("\"", "\"\"") + "\""
                : "\"" + value.Replace("\\", "\\\\").Replace("\"", "\\\"") + "\"";

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual string Literal(bool value) => value ? "true" : "false";

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual string Literal(byte value) => "(byte)" + value;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual string Literal(byte[] values) =>
            "new byte[] { " + string.Join(", ", values) + " }";

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual string Literal(char value) => "\'" + (value == '\'' ? "\\'" : value.ToString()) + "\'";

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual string Literal(DateTime value)
            => string.Format(
                CultureInfo.InvariantCulture,
                "new DateTime({0}, {1}, {2}, {3}, {4}, {5}, {6}, DateTimeKind.{7})",
                value.Year,
                value.Month,
                value.Day,
                value.Hour,
                value.Minute,
                value.Second,
                value.Millisecond,
                value.Kind);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual string Literal(DateTimeOffset value) =>
            "new DateTimeOffset(" + Literal(value.DateTime) + ", " + Literal(value.Offset) + ")";

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual string Literal(decimal value) => value.ToString(CultureInfo.InvariantCulture) + "m";

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual string Literal(double value) => EnsureDecimalPlaces(value.ToString("R", CultureInfo.InvariantCulture));

        private static string EnsureDecimalPlaces(string number) => number.IndexOf('.') >= 0 ? number : number + ".0";

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual string Literal(float value) => value.ToString(CultureInfo.InvariantCulture) + "f";

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual string Literal(Guid value) => "new Guid(\"" + value + "\")";

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual string Literal(int value) => value.ToString(CultureInfo.InvariantCulture);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual string Literal(long value) => value.ToString(CultureInfo.InvariantCulture) + "L";

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual string Literal(sbyte value) => "(sbyte)" + value;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual string Literal(short value) => "(short)" + value;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual string Literal(TimeSpan value)
            => string.Format(
                CultureInfo.InvariantCulture,
                "new TimeSpan({0}, {1}, {2}, {3}, {4})",
                value.Days,
                value.Hours,
                value.Minutes,
                value.Seconds,
                value.Milliseconds);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual string Literal(uint value) => value + "u";

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual string Literal(ulong value) => value + "ul";

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual string Literal(ushort value) => "(ushort)" + value;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual string Literal<T>(T? value)
            where T : struct =>
            UnknownLiteral(value);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual string Literal<T>(IReadOnlyList<T> values) =>
            Array(values);

        private string Array(IEnumerable values) =>
            "new[] { " + string.Join(", ", values.Cast<object>().Select(UnknownLiteral)) + " }";

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual string Literal(IReadOnlyList<object> values)
            => Literal(values, vertical: false);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual string Literal(IReadOnlyList<object> values, bool vertical)
        {
            if (!vertical)
            {
                return "new object[] { " + string.Join(", ", values.Select(UnknownLiteral)) + " }";
            }

            var builder = new IndentedStringBuilder();

            builder
                .AppendLine("new object[]")
                .AppendLine("{");

            using (builder.Indent())
            {
                for (var i = 0; i < values.Count; i++)
                {
                    if (i != 0)
                    {
                        builder.AppendLine(",");
                    }

                    builder.Append(UnknownLiteral(values[i]));
                }
            }

            builder
                .AppendLine()
                .Append("}");

            return builder.ToString();
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual string Literal(object[,] values)
        {
            var builder = new IndentedStringBuilder();

            builder
                .AppendLine("new object[,]")
                .AppendLine("{");

            using (builder.Indent())
            {
                var rowCount = values.GetLength(0);
                var valueCount = values.GetLength(1);
                for (var i = 0; i < rowCount; i++)
                {
                    if (i != 0)
                    {
                        builder.AppendLine(",");
                    }

                    builder.Append("{ ");

                    for (var j = 0; j < valueCount; j++)
                    {
                        if (j != 0)
                        {
                            builder.Append(", ");
                        }

                        builder.Append(UnknownLiteral(values[i, j]));
                    }

                    builder.Append(" }");
                }
            }

            builder
                .AppendLine()
                .Append("}");

            return builder.ToString();
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual string Literal(Enum value) => Reference(value.GetType()) + "." + value;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual string UnknownLiteral(object value)
        {
            if (value == null
                || value == DBNull.Value)
            {
                return "null";
            }

            var type = value.GetType().UnwrapNullableType();

            if (_literalFuncs.TryGetValue(type, out var literalFunc))
            {
                return literalFunc(this, value);
            }

            if (value is Enum enumValue)
            {
                return Literal(enumValue);
            }

            if (value is Array array)
            {
                return Array(array);
            }

            throw new InvalidOperationException(DesignStrings.UnknownLiteral(value.GetType()));
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual string Fragment(MethodCallCodeFragment fragment)
        {
            var builder = new StringBuilder();

            var current = fragment;
            while (current != null)
            {
                builder
                    .Append(".")
                    .Append(current.Method)
                    .Append("(");

                for (var i = 0; i < current.Arguments.Count; i++)
                {
                    if (i != 0)
                    {
                        builder.Append(", ");
                    }

                    builder.Append(UnknownLiteral(current.Arguments[i]));
                }

                builder.Append(")");

                current = current.ChainedCall;
            }

            return builder.ToString();
        }

        private string Fragment(NestedClosureCodeFragment fragment)
            => fragment.Parameter + " => " + fragment.Parameter + Fragment(fragment.MethodCall);

        private static bool IsIdentifierStartCharacter(char ch)
        {
            if (ch < 'a')
            {
                return ch < 'A'
                    ? false
                    : ch <= 'Z'
                       || ch == '_';
            }

            if (ch <= 'z')
            {
                return true;
            }

            return ch <= '\u007F' ? false : IsLetterChar(CharUnicodeInfo.GetUnicodeCategory(ch));
        }

        private static bool IsIdentifierPartCharacter(char ch)
        {
            if (ch < 'a')
            {
                return ch < 'A'
                    ? ch >= '0'
                           && ch <= '9'
                    : ch <= 'Z'
                       || ch == '_';
            }

            if (ch <= 'z')
            {
                return true;
            }

            if (ch <= '\u007F')
            {
                return false;
            }

            var cat = CharUnicodeInfo.GetUnicodeCategory(ch);
            if (IsLetterChar(cat))
            {
                return true;
            }

            switch (cat)
            {
                case UnicodeCategory.DecimalDigitNumber:
                case UnicodeCategory.ConnectorPunctuation:
                case UnicodeCategory.NonSpacingMark:
                case UnicodeCategory.SpacingCombiningMark:
                case UnicodeCategory.Format:
                    return true;
            }

            return false;
        }

        private static bool IsLetterChar(UnicodeCategory cat)
        {
            switch (cat)
            {
                case UnicodeCategory.UppercaseLetter:
                case UnicodeCategory.LowercaseLetter:
                case UnicodeCategory.TitlecaseLetter:
                case UnicodeCategory.ModifierLetter:
                case UnicodeCategory.OtherLetter:
                case UnicodeCategory.LetterNumber:
                    return true;
            }

            return false;
        }
    }
}
