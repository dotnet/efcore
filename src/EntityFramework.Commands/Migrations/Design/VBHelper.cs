// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Internal;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Migrations.Design
{
    public class VBHelper
    {
        private static readonly IReadOnlyDictionary<Type, string> _builtInTypes = new Dictionary<Type, string>
        {
            { typeof(bool), "Boolean" },
            { typeof(byte), "Byte" },
            { typeof(sbyte), "SByte" },
            { typeof(char), "Char" },
            { typeof(short), "Short" },
            { typeof(int), "Integer" },
            { typeof(long), "Long" },
            { typeof(ushort), "UShort" },
            { typeof(uint), "UInteger" },
            { typeof(ulong), "ULong" },
            { typeof(decimal), "Decimal" },
            { typeof(float), "Single" },
            { typeof(double), "Double" },
            { typeof(string), "String" },
            { typeof(object), "Object" }
        };

        //TODO: CONFIRM if VB dont have any keyword starting with "___" like C#
        private static readonly IReadOnlyCollection<string> _keywords = new[]
        {
            "AddHandler",
            "AddressOf",
            "Alias",
            "And",
            "AndAlso",
            "As",
            "Boolean",
            "ByRef",
            "Byte",
            "ByVal",
            "Call",
            "Case",
            "Catch",
            "CBool",
            "CByte",
            "CChar",
            "CDate",
            "CDbl",
            "CDec",
            "Char",
            "CInt",
            "Class",
            "CLng",
            "CObj",
            "Const",
            "Continue",
            "CSByte",
            "CShort",
            "CSng",
            "CStr",
            "CType",
            "CUInt",
            "CULng",
            "CUShort",
            "Date",
            "Decimal",
            "Declare",
            "Default",
            "Delegate",
            "Dim",
            "DirectCast",
            "Do",
            "Double",
            "Each",
            "Else",
            "ElseIf",
            "End",
            "EndIf",
            "Enum",
            "Erase",
            "Error",
            "Event",
            "Exit",
            "False",
            "Finally",
            "For",
            "Friend",
            "Function",
            "Get",
            "GetType",
            "GetXMLNamespace",
            "Global",
            "GoSub",
            "GoTo",
            "Handles",
            "If",
            "Implements",
            "Imports",
            "In",
            "Inherits",
            "Integer",
            "Interface",
            "Is",
            "IsNot",
            "Let",
            "Lib",
            "Like",
            "Long",
            "Loop",
            "Me",
            "Mod",
            "Module",
            "MustInherit",
            "MustOverride",
            "MyBase",
            "MyClass",
            "Namespace",
            "Narrowing",
            "New",
            "Next",
            "Not",
            "Nothing",
            "NotInheritable",
            "NotOverridable",
            "Object",
            "Of",
            "On",
            "Operator",
            "Option",
            "Optional",
            "Or",
            "OrElse",
            "Out",
            "Overloads",
            "Overridable",
            "Overrides",
            "ParamArray",
            "Partial",
            "Private",
            "Property",
            "Protected",
            "Public",
            "RaiseEvent",
            "ReadOnly",
            "ReDim",
            "REM",
            "RemoveHandler",
            "Resume",
            "Return",
            "SByte",
            "Select",
            "Set",
            "Shadows",
            "Shared",
            "Short",
            "Single",
            "Static",
            "Step",
            "Stop",
            "String",
            "Structure",
            "Sub",
            "SyncLock",
            "Then",
            "Throw",
            "To",
            "True",
            "Try",
            "TryCast",
            "TypeOf",
            "UInteger",
            "ULong",
            "UShort",
            "Using",
            "Variant",
            "Wend",
            "When",
            "While",
            "Widening",
            "With",
            "WithEvents",
            "WriteOnly",
            "Xor"
        };

        private static readonly IReadOnlyDictionary<Type, Func<VBHelper, object, string>> _literalFuncs =
            new Dictionary<Type, Func<VBHelper, object, string>>
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

        //TODO: you are trying to create a lambda here?...
        public virtual string Lambda([NotNull] IReadOnlyList<string> properties)
        {
            Check.NotNull(properties, nameof(properties));

            var builder = new StringBuilder();
            builder.Append("Function(x) ");

            if (properties.Count == 1)
            {
                builder.Append(Lambda(properties[0], "x"));
            }
            else
            {
                builder.Append("New From { ");
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

            string builtInType;
            if (_builtInTypes.TryGetValue(type, out builtInType))
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
                    .Append("(");

                var rank = type.GetArrayRank();
                for (int i = 1; i < rank; i++)
                {
                    builder.Append(",");
                }

                builder.Append(")");

                return builder.ToString();
            }

            if (type.IsNested)
            {
                builder
                    .Append(Reference(type.DeclaringType))
                    .Append(".");
            }

            builder.Append(type.DisplayName(fullName: false));

            return builder.ToString();
        }

        public virtual string Identifier([NotNull] string name, [CanBeNull] ICollection<string> scope = null)
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

            if (builder.Length == 0 || !IsIdentifierStartCharacter(builder[0]))
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

            if (_keywords.Contains(identifier))
            {
                return "[" + identifier + "]";
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

        //TODO: replace NewLine with vbCRLF? ex: value.Replace(NewLine, """ + vbCrLf + """) ???
        public virtual string Literal([NotNull] string value) => value.ToString();
            //value.Contains(Environment.NewLine)
            //    ? "@\"" + value.Replace("\"", "\"\"") + "\""
            //    : "\"" + value.Replace("\\", "\\\\").Replace("\"", "\\\"") + "\"";

        public virtual string Literal(bool value) => value ? "True" : "False";
        public virtual string Literal(byte value) => "CType(" + value + ", byte)";

        public virtual string Literal([NotNull] byte[] values) =>
            "New Byte() { " + string.Join(", ", values) + " }";

        public virtual string Literal(char value) => value.ToString(); // "\'" + (value == '\'' ? "\\'" : value.ToString()) + "\'";

        public virtual string Literal(DateTime value) =>
            String.Format(
                "New DateTime({0}, {1}, {2}, {3}, {4}, {5}, {6}, DateTimeKind.{7})",
                value.Year, value.Month, value.Day, value.Hour, value.Minute, value.Second, value.Millisecond, value.Kind);

        public virtual string Literal(DateTimeOffset value) =>
            "New DateTimeOffset(" + Literal(value.DateTime) + ", " + Literal(value.Offset) + ")";

        public virtual string Literal(decimal value) => value.ToString(CultureInfo.InvariantCulture) + "D";
        public virtual string Literal(double value) => value.ToString(CultureInfo.InvariantCulture) + "R";
        public virtual string Literal(float value) => value.ToString(CultureInfo.InvariantCulture) + "F";
        public virtual string Literal(Guid value) => "New Guid(" + value + ")";
        public virtual string Literal(int value) => value.ToString();
        public virtual string Literal(long value) => value + "L";
        public virtual string Literal(sbyte value) => "CType(" + value + ", sbyte)";
        public virtual string Literal(short value) => value + "S";

        public virtual string Literal(TimeSpan value) =>
            String.Format(
                "New TimeSpan({0}, {1}, {2}, {3}, {4})",
                value.Days, value.Hours, value.Minutes, value.Seconds, value.Milliseconds);

        public virtual string Literal(uint value) => value + "UI";
        public virtual string Literal(ulong value) => value + "UL";
        public virtual string Literal(ushort value) => value + "US";

        public virtual string Literal<T>([NotNull] T? value) where T : struct =>
            UnknownLiteral(value.Value);


        //TODO: VB dont have New[]. New String()? New From {}?
        public virtual string Literal([NotNull] IReadOnlyList<string> values) =>
            values.Count == 1
                ? Literal(values[0])
                : "New String() { " + string.Join(", ", values.Select(Literal)) + " }";

        public virtual string Literal([NotNull] Enum value) => Reference(value.GetType()) + "." + value;

        public virtual string UnknownLiteral([CanBeNull] object value)
        {
            if (value == null)
            {
                return "Nothing";
            }

            var type = value.GetType().UnwrapNullableType();

            Func<VBHelper, object, string> literalFunc;
            if (_literalFuncs.TryGetValue(type, out literalFunc))
            {
                return literalFunc(this, value);
            }

            var enumValue = value as Enum;
            if (enumValue != null)
            {
                return Literal(enumValue);
            }

            throw new InvalidOperationException(CommandsStrings.UnknownLiteral(value.GetType()));
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
