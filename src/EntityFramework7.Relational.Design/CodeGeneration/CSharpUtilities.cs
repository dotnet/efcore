// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Relational.Design.CodeGeneration
{
    public class CSharpUtilities
    {
        private static readonly HashSet<string> _cSharpKeywords = new HashSet<string>
        {
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

        private static readonly Regex _invalidCharsRegex
            = new Regex(@"[^\p{Ll}\p{Lu}\p{Lt}\p{Lo}\p{Nd}\p{Nl}\p{Mn}\p{Mc}\p{Cf}\p{Pc}\p{Lm}]",
                default(RegexOptions),
                TimeSpan.FromMilliseconds(1000.0));

        public static CSharpUtilities Instance { get; } = new CSharpUtilities();

        public virtual string DelimitString([NotNull] string value)
        {
            Check.NotNull(value, nameof(value));

            return "\"" + EscapeString(value) + "\"";
        }

        public virtual string EscapeString([NotNull] string str)
        {
            Check.NotNull(str, nameof(str));

            return str.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\t", "\\t");
        }

        public virtual string EscapeVerbatimString([NotNull] string str)
        {
            Check.NotEmpty(str, nameof(str));

            return str.Replace("\"", "\"\"");
        }

        public virtual string GenerateLiteral([NotNull] byte[] value)
        {
            Check.NotNull(value, nameof(value));

            return "new byte[] {" + string.Join(", ", value) + "}";
        }

        public virtual string GenerateLiteral(bool value)
        {
            return value ? "true" : "false";
        }

        public virtual string GenerateLiteral(int value)
        {
            return value.ToString(CultureInfo.InvariantCulture);
        }

        public virtual string GenerateLiteral(long value)
        {
            return value.ToString(CultureInfo.InvariantCulture) + "L";
        }

        public virtual string GenerateLiteral(decimal value)
        {
            return value.ToString(CultureInfo.InvariantCulture) + "m";
        }

        public virtual string GenerateLiteral(float value)
        {
            return value.ToString(CultureInfo.InvariantCulture) + "f";
        }

        public virtual string GenerateLiteral(double value)
        {
            return value.ToString(CultureInfo.InvariantCulture) + "D";
        }

        public virtual string GenerateLiteral(TimeSpan value)
        {
            return "new TimeSpan(" + value.Ticks + ")";
        }

        public virtual string GenerateLiteral(DateTime value)
        {
            return "new DateTime(" + value.Ticks + ", DateTimeKind."
                   + Enum.GetName(typeof(DateTimeKind), value.Kind) + ")";
        }

        public virtual string GenerateLiteral(DateTimeOffset value)
        {
            return "new DateTimeOffset(" + value.Ticks + ", "
                   + GenerateLiteral(value.Offset) + ")";
        }

        public virtual string GenerateLiteral(Guid value)
        {
            return "new Guid(" + GenerateLiteral(value.ToString()) + ")";
        }

        public virtual string GenerateLiteral([NotNull] string value)
        {
            Check.NotNull(value, nameof(value));

            return "\"" + EscapeString(value) + "\"";
        }

        public virtual string GenerateVerbatimStringLiteral([NotNull] string value)
        {
            Check.NotNull(value, nameof(value));

            return "@\"" + EscapeVerbatimString(value) + "\"";
        }

        public virtual string GenerateLiteral([NotNull] object value)
        {
            Check.NotNull(value, nameof(value));

            if (value.GetType().GetTypeInfo().IsEnum)
            {
                return value.GetType().Name + "." + Enum.Format(value.GetType(), value, "G");
            }

            return string.Format(CultureInfo.InvariantCulture, "{0}", value);
        }

        public virtual string GenerateCSharpIdentifier(
            [NotNull] string identifier, [CanBeNull] ICollection<string> existingIdentifiers)
        {
            return GenerateCSharpIdentifier(identifier, existingIdentifiers, Uniquifier);
        }

        public virtual string GenerateCSharpIdentifier(
            [NotNull] string identifier, [CanBeNull] ICollection<string> existingIdentifiers,
            [NotNull] Func<string, ICollection<string>, string> uniquifier)
        {
            Check.NotEmpty(identifier, nameof(identifier));
            Check.NotNull(uniquifier, nameof(uniquifier));

            var proposedIdentifier =
                (identifier.Length > 1 && identifier[0] == '@')
                    ? "@" + _invalidCharsRegex.Replace(identifier.Substring(1), "_")
                    : _invalidCharsRegex.Replace(identifier, "_");
            if (string.IsNullOrEmpty(proposedIdentifier))
            {
                proposedIdentifier = "_";
            }

            var firstChar = proposedIdentifier[0];
            if (!char.IsLetter(firstChar)
                && firstChar != '_'
                && firstChar != '@')
            {
                proposedIdentifier = "_" + proposedIdentifier;
            }
            else if (_cSharpKeywords.Contains(proposedIdentifier))
            {
                proposedIdentifier = "_" + proposedIdentifier;
            }

            return uniquifier(proposedIdentifier, existingIdentifiers);
        }

        public virtual string Uniquifier(
            [NotNull] string proposedIdentifier, [CanBeNull] ICollection<string> existingIdentifiers)
        {
            Check.NotEmpty(proposedIdentifier, nameof(proposedIdentifier));

            if (existingIdentifiers == null)
            {
                return proposedIdentifier;
            }

            var finalIdentifier = proposedIdentifier;
            var suffix = 1;
            while (existingIdentifiers.Contains(finalIdentifier))
            {
                finalIdentifier = proposedIdentifier + suffix;
                suffix++;
            }

            return finalIdentifier;
        }
    }
}
