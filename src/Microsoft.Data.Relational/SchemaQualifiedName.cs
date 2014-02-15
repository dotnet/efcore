// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Globalization;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using Microsoft.Data.Relational.Utilities;

namespace Microsoft.Data.Relational
{
    public struct SchemaQualifiedName : IEquatable<SchemaQualifiedName>
    {
        private const string NamePartRegex
            = @"(?:(?:\[(?<part{0}>(?:(?:\]\])|[^\]])+)\])|(?<part{0}>[^\.\[\]]+))";

        private static readonly Regex _partExtractor
            = new Regex(
                string.Format(
                    CultureInfo.InvariantCulture,
                    @"^{0}(?:\.{1})?$",
                    string.Format(CultureInfo.InvariantCulture, NamePartRegex, 1),
                    string.Format(CultureInfo.InvariantCulture, NamePartRegex, 2)));

        public static SchemaQualifiedName Parse([NotNull] string name)
        {
            Check.NotNull(name, "name");

            var match = _partExtractor.Match(name.Trim());

            if (!match.Success)
            {
                throw new ArgumentException(Strings.InvalidSchemaQualifiedName(name));
            }

            var part1 = match.Groups["part1"].Value.Replace("]]", "]");
            var part2 = match.Groups["part2"].Value.Replace("]]", "]");

            return !string.IsNullOrEmpty(part2)
                ? new SchemaQualifiedName(part2, part1)
                : new SchemaQualifiedName(part1);
        }

        public static implicit operator string(SchemaQualifiedName schemaQualifiedName)
        {
            return schemaQualifiedName.ToString();
        }

        public static implicit operator SchemaQualifiedName([NotNull] string s)
        {
            return Parse(s);
        }

        public static bool operator ==(SchemaQualifiedName left, SchemaQualifiedName right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(SchemaQualifiedName left, SchemaQualifiedName right)
        {
            return !left.Equals(right);
        }

        private readonly string _name;
        private readonly string _schema;

        public SchemaQualifiedName([NotNull] string name)
            : this(Check.NotEmpty(name, "name"), null)
        {
            _name = name;
        }

        public SchemaQualifiedName([NotNull] string name, [CanBeNull] string schema)
        {
            Check.NotEmpty(name, "name");

            _name = name;
            _schema = schema;
        }

        public string Name
        {
            get { return _name; }
        }

        public string Schema
        {
            get { return _schema; }
        }

        public bool IsSchemaQualified
        {
            get { return _schema != null; }
        }

        public override string ToString()
        {
            var s = Escape(_name);

            if (_schema != null)
            {
                s = Escape(_schema) + "." + s;
            }

            return s;
        }

        private static string Escape(string name)
        {
            return name.IndexOfAny(new[] { ']', '[', '.' }) != -1
                ? "[" + name.Replace("]", "]]") + "]"
                : name;
        }

        public bool Equals(SchemaQualifiedName other)
        {
            return string.Equals(_name, other._name)
                   && string.Equals(_schema, other._schema);
        }

        public override bool Equals([CanBeNull] object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            return obj is SchemaQualifiedName
                   && Equals((SchemaQualifiedName)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (_name.GetHashCode() * 397)
                       ^ (_schema != null ? _schema.GetHashCode() : 0);
            }
        }
    }
}
