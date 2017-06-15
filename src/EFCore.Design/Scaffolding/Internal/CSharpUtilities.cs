// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Scaffolding.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class CSharpUtilities : ICSharpUtilities
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

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual string DelimitString(string value)
        {
            Check.NotNull(value, nameof(value));

            return value.Contains(Environment.NewLine)
                ? "@\"" + EscapeVerbatimString(value) + "\""
                : "\"" + EscapeString(value) + "\"";
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual string EscapeString(string str)
        {
            Check.NotNull(str, nameof(str));

            return str.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\t", "\\t");
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual string EscapeVerbatimString(string str)
        {
            Check.NotEmpty(str, nameof(str));

            return str.Replace("\"", "\"\"");
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual string GenerateLiteral(byte[] value)
        {
            Check.NotNull(value, nameof(value));

            return "new byte[] {" + string.Join(", ", value) + "}";
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual string GenerateLiteral(bool value)
            => value ? "true" : "false";

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual string GenerateLiteral(int value)
            => value.ToString(CultureInfo.InvariantCulture);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual string GenerateLiteral(long value)
            => value.ToString(CultureInfo.InvariantCulture) + "L";

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual string GenerateLiteral(decimal value)
            => value.ToString(CultureInfo.InvariantCulture) + "m";

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual string GenerateLiteral(float value)
            => value.ToString(CultureInfo.InvariantCulture) + "f";

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual string GenerateLiteral(double value)
            => value.ToString(CultureInfo.InvariantCulture) + "D";

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual string GenerateLiteral(TimeSpan value)
            => "new TimeSpan(" + value.Ticks + ")";

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual string GenerateLiteral(DateTime value)
            => "new DateTime(" + value.Ticks + ", DateTimeKind."
               + Enum.GetName(typeof(DateTimeKind), value.Kind) + ")";

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual string GenerateLiteral(DateTimeOffset value)
            => "new DateTimeOffset(" + value.Ticks + ", "
               + GenerateLiteral(value.Offset) + ")";

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual string GenerateLiteral(Guid value)
            => "new Guid(" + GenerateLiteral(value.ToString()) + ")";

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual string GenerateLiteral(string value)
        {
            Check.NotNull(value, nameof(value));

            return "\"" + EscapeString(value) + "\"";
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual string GenerateVerbatimStringLiteral(string value)
        {
            Check.NotNull(value, nameof(value));

            return "@\"" + EscapeVerbatimString(value) + "\"";
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual string GenerateLiteral(object value)
        {
            Check.NotNull(value, nameof(value));

            if (value.GetType().GetTypeInfo().IsEnum)
            {
                return value.GetType().Name + "." + Enum.Format(value.GetType(), value, "G");
            }

            return string.Format(CultureInfo.InvariantCulture, "{0}", value);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual bool IsCSharpKeyword(string identifier)
            => _cSharpKeywords.Contains(identifier);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual string GenerateCSharpIdentifier(
            string identifier,
            [CanBeNull] ICollection<string> existingIdentifiers,
            [CanBeNull] Func<string, string> singularizePluralizer)
            => GenerateCSharpIdentifier(identifier, existingIdentifiers, singularizePluralizer, Uniquifier);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual string GenerateCSharpIdentifier(
            string identifier,
            [CanBeNull] ICollection<string> existingIdentifiers,
            [CanBeNull] Func<string, string> singularizePluralizer,
            Func<string, ICollection<string>, string> uniquifier)
        {
            Check.NotNull(identifier, nameof(identifier));
            Check.NotNull(uniquifier, nameof(uniquifier));

            var proposedIdentifier =
                identifier.Length > 1 && identifier[0] == '@'
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
            else if (IsCSharpKeyword(proposedIdentifier))
            {
                proposedIdentifier = "_" + proposedIdentifier;
            }

            if (singularizePluralizer != null)
            {
                proposedIdentifier = singularizePluralizer(proposedIdentifier);
            }

            return uniquifier(proposedIdentifier, existingIdentifiers);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual string Uniquifier(
            string proposedIdentifier, [CanBeNull] ICollection<string> existingIdentifiers)
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

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual string GetTypeName(Type type)
        {
            Check.NotNull(type, nameof(type));

            if (type.IsArray)
            {
                return GetTypeName(type.GetElementType()) + "[]";
            }

            if (type.GetTypeInfo().IsGenericType)
            {
                if (type.GetGenericTypeDefinition() == typeof(Nullable<>))
                {
                    return GetTypeName(Nullable.GetUnderlyingType(type)) + '?';
                }

                var genericTypeDefName = type.Name.Substring(0, type.Name.IndexOf('`'));
                var genericTypeArguments = string.Join(", ", type.GenericTypeArguments.Select(GetTypeName));
                return $"{genericTypeDefName}<{genericTypeArguments}>";
            }

            string typeName;
            return _primitiveTypeNames.TryGetValue(type, out typeName)
                ? typeName
                : type.Name;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual bool IsValidIdentifier([CanBeNull] string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return false;
            }
            if (!IsIdentifierStartCharacter(name[0]))
            {
                return false;
            }

            var nameLength = name.Length;
            for (var i = 1; i < nameLength; i++)
            {
                if (!IsIdentifierPartCharacter(name[i]))
                {
                    return false;
                }
            }

            return true;
        }

        private static bool IsIdentifierStartCharacter(char ch)
        {
            if (ch < 'a')
            {
                if (ch < 'A')
                {
                    return false;
                }

                return ch <= 'Z'
                       || ch == '_';
            }
            if (ch <= 'z')
            {
                return true;
            }
            if (ch <= '\u007F') // max ASCII
            {
                return false;
            }

            return IsLetterChar(CharUnicodeInfo.GetUnicodeCategory(ch));
        }

        private static bool IsIdentifierPartCharacter(char ch)
        {
            if (ch < 'a')
            {
                if (ch < 'A')
                {
                    return ch >= '0'
                           && ch <= '9';
                }

                return ch <= 'Z'
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
