// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Metadata
{
    public class Sequence : ISequence
    {
        private readonly IModel _model;
        private readonly string _annotationName;

        public static readonly Type DefaultClrType = typeof(long);
        public const int DefaultIncrementBy = 1;
        public const int DefaultStartValue = 1;

        public static readonly long? DefaultMaxValue = default(long?);
        public static readonly long? DefaultMinValue = default(long?);
        public static readonly bool DefaultIsCyclic = default(bool);

        private Sequence(
            [NotNull] IMutableModel model,
            [NotNull] string annotationPrefix,
            [NotNull] string name,
            [CanBeNull] string schema = null)
            : this(model, BuildAnnotationName(annotationPrefix, name, schema))
        {
            Check.NotNull(model, nameof(model));
            Check.NotEmpty(annotationPrefix, nameof(annotationPrefix));
            Check.NotEmpty(name, nameof(name));
            Check.NullButNotEmpty(schema, nameof(schema));

            SetData(new SequenceData
            {
                Name = name,
                Schema = schema,
                ClrType = DefaultClrType,
                IncrementBy = DefaultIncrementBy,
                StartValue = DefaultStartValue
            });
        }

        private Sequence(IModel model, string annotationName)
        {
            _model = model;
            _annotationName = annotationName;
        }

        public static Sequence GetOrAddSequence(
            [NotNull] IMutableModel model,
            [NotNull] string annotationPrefix,
            [NotNull] string name,
            [CanBeNull] string schema = null)
            => FindSequence(model, annotationPrefix, name, schema) ?? new Sequence(model, annotationPrefix, name, schema);

        private static string BuildAnnotationName(string annotationPrefix, string name, string schema)
            => annotationPrefix + schema + "." + name;

        public static Sequence FindSequence(
            [NotNull] IMutableModel model,
            [NotNull] string annotationPrefix,
            [NotNull] string name,
            [CanBeNull] string schema = null)
            => (Sequence)FindSequence((IModel)model, annotationPrefix, name, schema);

        public static ISequence FindSequence(
            [NotNull] IModel model,
            [NotNull] string annotationPrefix,
            [NotNull] string name,
            [CanBeNull] string schema = null)
        {
            Check.NotNull(model, nameof(model));
            Check.NotEmpty(name, nameof(name));
            Check.NullButNotEmpty(schema, nameof(schema));

            var annotationName = BuildAnnotationName(annotationPrefix, name, schema);

            return model[annotationName] == null ? null : new Sequence(model, annotationName);
        }

        public static IEnumerable<ISequence> GetSequences([NotNull] IModel model, [NotNull] string annotationPrefix)
        {
            Check.NotNull(model, nameof(model));
            Check.NotEmpty(annotationPrefix, nameof(annotationPrefix));

            return model.GetAnnotations()
                .Where(a => a.Name.StartsWith(annotationPrefix, StringComparison.Ordinal))
                .Select(a => new Sequence(model, a.Name));
        }

        public virtual Model Model => (Model)_model;

        public virtual string Name => GetData().Name;

        public virtual string Schema => GetData().Schema ?? Model.Relational().DefaultSchema;

        public virtual long StartValue
        {
            get { return GetData().StartValue; }
            set
            {
                var data = GetData();
                data.StartValue = value;
                SetData(data);
            }
        }

        public virtual int IncrementBy
        {
            get { return GetData().IncrementBy; }
            set
            {
                var data = GetData();
                data.IncrementBy = value;
                SetData(data);
            }
        }

        public virtual long? MinValue
        {
            get { return GetData().MinValue; }
            set
            {
                var data = GetData();
                data.MinValue = value;
                SetData(data);
            }
        }

        public virtual long? MaxValue
        {
            get { return GetData().MaxValue; }
            set
            {
                var data = GetData();
                data.MaxValue = value;
                SetData(data);
            }
        }

        public static IReadOnlyCollection<Type> SupportedTypes { get; } = new[]
        {
            typeof(byte),
            typeof(long),
            typeof(int),
            typeof(short)
        };

        public virtual Type ClrType
        {
            get { return GetData().ClrType; }
            [param: NotNull]
            set
            {
                if (!SupportedTypes.Contains(value))
                {
                    throw new ArgumentException(RelationalStrings.BadSequenceType);
                }

                var data = GetData();
                data.ClrType = value;
                SetData(data);
            }
        }

        public virtual bool IsCyclic
        {
            get { return GetData().IsCyclic; }
            set
            {
                var data = GetData();
                data.IsCyclic = value;
                SetData(data);
            }
        }

        private SequenceData GetData() => SequenceData.Deserialize((string)Model[_annotationName]);

        private void SetData(SequenceData data)
        {
            Model[_annotationName] = data.Serialize();
        }

        IModel ISequence.Model => _model;

        long ISequence.StartValue => StartValue;

        int ISequence.IncrementBy => IncrementBy;

        long? ISequence.MinValue => MinValue;

        long? ISequence.MaxValue => MaxValue;

        Type ISequence.ClrType => ClrType;

        bool ISequence.IsCyclic => IsCyclic;

        private class SequenceData
        {
            public string Name { get; set; }

            public string Schema { get; set; }

            public long StartValue { get; set; }

            public int IncrementBy { get; set; }

            public long? MinValue { get; set; }

            public long? MaxValue { get; set; }

            public Type ClrType { get; set; }

            public bool IsCyclic { get; set; }

            public string Serialize()
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
                EscapeAndQuote(builder, ClrType.Name);
                builder.Append(", ");
                EscapeAndQuote(builder, IsCyclic);

                return builder.ToString();
            }

            public static SequenceData Deserialize([NotNull] string value)
            {
                Check.NotEmpty(value, nameof(value));

                try
                {
                    var data = new SequenceData();

                    // ReSharper disable PossibleInvalidOperationException
                    var position = 0;
                    data.Name = ExtractValue(value, ref position);
                    data.Schema = ExtractValue(value, ref position);
                    data.StartValue = (long)AsLong(ExtractValue(value, ref position));
                    data.IncrementBy = (int)AsLong(ExtractValue(value, ref position));
                    data.MinValue = AsLong(ExtractValue(value, ref position));
                    data.MaxValue = AsLong(ExtractValue(value, ref position));
                    data.ClrType = AsType(ExtractValue(value, ref position));
                    data.IsCyclic = AsBool(ExtractValue(value, ref position));
                    // ReSharper restore PossibleInvalidOperationException

                    return data;
                }
                catch (Exception ex)
                {
                    throw new ArgumentException(RelationalStrings.BadSequenceString, ex);
                }
            }

            private static string ExtractValue(string value, ref int position)
            {
                position = value.IndexOf('\'', position) + 1;

                var end = value.IndexOf('\'', position);

                while ((end + 1 < value.Length)
                       && (value[end + 1] == '\''))
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
                => (value != null) && bool.Parse(value);

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
}
