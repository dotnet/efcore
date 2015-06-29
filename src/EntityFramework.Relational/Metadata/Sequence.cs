// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Globalization;
using System.Text;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Relational.Internal;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Metadata
{
    public class Sequence : ISequence
    {
        public const string DefaultName = "DefaultSequence";
        public const int DefaultIncrement = 10;
        public const long DefaultStartValue = 1;
        public static readonly Type DefaultType = typeof(long);

        private IModel _model;

        public Sequence(
            [NotNull] string name,
            [CanBeNull] string schema = null,
            long startValue = DefaultStartValue,
            int incrementBy = DefaultIncrement,
            [CanBeNull] long? minValue = null,
            [CanBeNull] long? maxValue = null,
            [CanBeNull] Type type = null,
            bool cycle = false)
        {
            Check.NotEmpty(name, nameof(name));
            Check.NullButNotEmpty(schema, nameof(schema));

            type = type ?? DefaultType;

            if (type != typeof(byte)
                && type != typeof(long)
                && type != typeof(int)
                && type != typeof(short))
            {
                // See Issue #242 for supporting all types
                throw new ArgumentException(Strings.BadSequenceType);
            }

            Name = name;
            Schema = schema;
            StartValue = startValue;
            IncrementBy = incrementBy;
            MinValue = minValue;
            MaxValue = maxValue;
            Type = type;
            Cycle = cycle;
        }

        public virtual string Name { get; }

        public virtual string Schema { get; }

        public virtual long StartValue { get; }

        public virtual int IncrementBy { get; }

        public virtual long? MinValue { get; }

        public virtual long? MaxValue { get; }

        public virtual Type Type { get; }

        public virtual bool Cycle { get; }

        public virtual IModel Model
        {
            get { return _model; }
            [param: NotNull]
            set
            {
                Check.NotNull(value, nameof(value));

                _model = value;
            }
        }

        public virtual string Serialize()
        {
            var builder = new StringBuilder();

            EscapeAndQuote(builder, Name);
            builder.Append(", ");
            EscapeAndQuote(builder, Schema);
            builder.Append(", ");
            EscapeAndQuote(builder, StartValue);
            builder.Append(", ");
            EscapeAndQuote(builder, IncrementBy);
            builder.Append(", ");
            EscapeAndQuote(builder, MinValue);
            builder.Append(", ");
            EscapeAndQuote(builder, MaxValue);
            builder.Append(", ");
            EscapeAndQuote(builder, Type.Name);
            builder.Append(", ");
            EscapeAndQuote(builder, Cycle);

            return builder.ToString();
        }

        public static Sequence Deserialize([NotNull] string value)
        {
            Check.NotEmpty(value, nameof(value));

            try
            {
                var position = 0;
                var name = ExtractValue(value, ref position);
                var schema = ExtractValue(value, ref position);
                var startValue = AsLong(ExtractValue(value, ref position));
                var incrementBy = AsLong(ExtractValue(value, ref position));
                var minValue = AsLong(ExtractValue(value, ref position));
                var maxValue = AsLong(ExtractValue(value, ref position));
                var type = AsType(ExtractValue(value, ref position));
                var cycle = AsBool(ExtractValue(value, ref position));

                return new Sequence(name, schema, (long)startValue, (int)incrementBy, minValue, maxValue, type, cycle);
            }
            catch (Exception ex)
            {
                throw new ArgumentException(Strings.BadSequenceString, ex);
            }
        }

        private static string ExtractValue(string value, ref int position)
        {
            position = value.IndexOf('\'', position) + 1;

            var end = value.IndexOf('\'', position);

            while (end + 1 < value.Length
                   && value[end + 1] == '\'')
            {
                end = value.IndexOf('\'', end + 2);
            }

            var extracted = value.Substring(position, end - position).Replace("''", "'");
            position = end + 1;

            return extracted.Length == 0 ? null : extracted;
        }

        private static long? AsLong(string value)
            => value == null ? null : (long?)long.Parse(value, CultureInfo.InvariantCulture);

        private static Type AsType(string value)
            => value == typeof(long).Name
                ? typeof(long)
                : value == typeof(int).Name
                    ? typeof(int)
                    : value == typeof(short).Name
                        ? typeof(short)
                        : typeof(byte);

        private static bool AsBool(string value)
            => value != null && bool.Parse(value);

        private static void EscapeAndQuote(StringBuilder builder, object value)
        {
            builder.Append("'");

            if (value != null)
            {
                builder.Append(value.ToString().Replace("'", "''"));
            }

            builder.Append("'");
        }
    }
}
