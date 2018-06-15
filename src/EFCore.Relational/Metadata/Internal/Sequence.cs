// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Metadata.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class Sequence : IMutableSequence
    {
        private readonly IModel _model;
        private readonly string _annotationName;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static readonly Type DefaultClrType = typeof(long);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public const int DefaultIncrementBy = 1;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public const int DefaultStartValue = 1;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static readonly long? DefaultMaxValue = default;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static readonly long? DefaultMinValue = default;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static readonly bool DefaultIsCyclic = default;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public Sequence(
            [NotNull] IMutableModel model,
            [NotNull] string annotationName,
            [NotNull] string name,
            [CanBeNull] string schema = null)
            : this(model, annotationName)
        {
            Check.NotEmpty(name, nameof(name));
            Check.NullButNotEmpty(schema, nameof(schema));

            SetData(
                new SequenceData
                {
                    Name = name,
                    Schema = schema,
                    ClrType = DefaultClrType,
                    IncrementBy = DefaultIncrementBy,
                    StartValue = DefaultStartValue
                });
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public Sequence([NotNull] IModel model, [NotNull] string annotationName)
        {
            Check.NotNull(model, nameof(model));
            Check.NotEmpty(annotationName, nameof(annotationName));

            _model = model;
            _annotationName = annotationName;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static IEnumerable<Sequence> GetSequences([NotNull] IModel model, [NotNull] string annotationPrefix)
        {
            Check.NotNull(model, nameof(model));
            Check.NotEmpty(annotationPrefix, nameof(annotationPrefix));

            return model.GetAnnotations()
                .Where(a => a.Name.StartsWith(annotationPrefix, StringComparison.Ordinal))
                .Select(a => new Sequence(model, a.Name));
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual Model Model => (Model)_model;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual string Name => GetData().Name;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual string Schema => GetData().Schema ?? Model.Relational().DefaultSchema;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual long StartValue
        {
            get => GetData().StartValue;
            set
            {
                var data = GetData();
                data.StartValue = value;
                SetData(data);
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual int IncrementBy
        {
            get => GetData().IncrementBy;
            set
            {
                var data = GetData();
                data.IncrementBy = value;
                SetData(data);
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual long? MinValue
        {
            get => GetData().MinValue;
            set
            {
                var data = GetData();
                data.MinValue = value;
                SetData(data);
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual long? MaxValue
        {
            get => GetData().MaxValue;
            set
            {
                var data = GetData();
                data.MaxValue = value;
                SetData(data);
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static IReadOnlyCollection<Type> SupportedTypes { get; } = new[]
        {
            typeof(byte),
            typeof(long),
            typeof(int),
            typeof(short),
            typeof(decimal)
        };

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual Type ClrType
        {
            get => GetData().ClrType;
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

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual bool IsCyclic
        {
            get => GetData().IsCyclic;
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
                            : value == typeof(decimal).Name
                                ? typeof(decimal)
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
}
