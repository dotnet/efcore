// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Metadata.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public class Sequence : IMutableSequence, IConventionSequence
    {
        private readonly IModel _model;
        private readonly string _annotationName;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public static readonly Type DefaultClrType = typeof(long);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public const int DefaultIncrementBy = 1;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public const int DefaultStartValue = 1;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public static readonly long? DefaultMaxValue = default;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public static readonly long? DefaultMinValue = default;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public static readonly bool DefaultIsCyclic = default;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public Sequence(
            [NotNull] IMutableModel model,
            [NotNull] string name,
            [CanBeNull] string schema,
            ConfigurationSource configurationSource)
        {
            Check.NotEmpty(name, nameof(name));
            Check.NullButNotEmpty(schema, nameof(schema));

            _model = model;
            _annotationName = BuildSequenceAnnotationName(RelationalAnnotationNames.SequencePrefix, name, schema);

            var data = new SequenceData
            {
                Name = name,
                Schema = schema,
                ClrType = DefaultClrType,
                IncrementBy = DefaultIncrementBy,
                StartValue = DefaultStartValue
            }.Serialize();

            if (configurationSource == ConfigurationSource.Explicit)
            {
                model.AddAnnotation(_annotationName, data);
            }
            else
            {
                ((IConventionModel)model).AddAnnotation(
                    _annotationName,
                    data,
                    configurationSource == ConfigurationSource.DataAnnotation);
            }
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public Sequence([NotNull] IModel model, [NotNull] string annotationName)
        {
            Check.NotNull(model, nameof(model));
            Check.NotEmpty(annotationName, nameof(annotationName));

            _model = model;
            _annotationName = annotationName;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
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
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual IMutableModel Model => (IMutableModel)_model;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        IConventionSequenceBuilder IConventionSequence.Builder => new SequenceBuilder(this);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual string Name => GetData().Name;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual string Schema => GetData().Schema ?? Model.GetDefaultSchema();

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual long StartValue
        {
            get => GetData().StartValue;
            set => SetStartValue(value, ConfigurationSource.Explicit);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual void SetStartValue(long? startValue, ConfigurationSource configurationSource)
        {
            var data = GetData();
            data.StartValue = startValue ?? DefaultStartValue;
            SetData(data);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual ConfigurationSource? GetStartValueConfigurationSource()
            => ConfigurationSource.Explicit;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual int IncrementBy
        {
            get => GetData().IncrementBy;
            set => SetIncrementBy(value, ConfigurationSource.Explicit);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual void SetIncrementBy(int? incrementBy, ConfigurationSource configurationSource)
        {
            var data = GetData();
            data.IncrementBy = incrementBy ?? DefaultIncrementBy;
            SetData(data);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual ConfigurationSource? GetIncrementByConfigurationSource()
            => ConfigurationSource.Explicit;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual long? MinValue
        {
            get => GetData().MinValue;
            set => SetMinValue(value, ConfigurationSource.Explicit);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual void SetMinValue(long? minValue, ConfigurationSource configurationSource)
        {
            var data = GetData();
            data.MinValue = minValue ?? DefaultMinValue;
            SetData(data);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual ConfigurationSource? GetMinValueConfigurationSource()
            => ConfigurationSource.Explicit;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual long? MaxValue
        {
            get => GetData().MaxValue;
            set => SetMaxValue(value, ConfigurationSource.Explicit);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual void SetMaxValue(long? maxValue, ConfigurationSource configurationSource)
        {
            var data = GetData();
            data.MaxValue = maxValue ?? DefaultMaxValue;
            SetData(data);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual ConfigurationSource? GetMaxValueConfigurationSource()
            => ConfigurationSource.Explicit;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public static IReadOnlyCollection<Type> SupportedTypes { get; }
            = new[]
            {
                typeof(byte), typeof(long), typeof(int), typeof(short), typeof(decimal)
            };

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual Type ClrType
        {
            get => GetData().ClrType;
            set => SetClrType(value, ConfigurationSource.Explicit);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual void SetClrType(Type clrType, ConfigurationSource configurationSource)
        {
            clrType ??= DefaultClrType;
            if (!SupportedTypes.Contains(clrType))
            {
                throw new ArgumentException(RelationalStrings.BadSequenceType);
            }

            var data = GetData();
            data.ClrType = clrType;
            SetData(data);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual ConfigurationSource? GetClrTypeConfigurationSource()
            => ConfigurationSource.Explicit;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual bool IsCyclic
        {
            get => GetData().IsCyclic;
            set => SetIsCyclic(value, ConfigurationSource.Explicit);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual void SetIsCyclic(bool? cyclic, ConfigurationSource configurationSource)
        {
            var data = GetData();
            data.IsCyclic = cyclic ?? DefaultIsCyclic;
            SetData(data);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual ConfigurationSource? GetIsCyclicConfigurationSource()
            => ConfigurationSource.Explicit;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual ConfigurationSource GetConfigurationSource()
            => ((IConventionModel)Model).FindAnnotation(_annotationName).GetConfigurationSource();

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual void UpdateConfigurationSource(ConfigurationSource configurationSource)
            => ((Model)Model).FindAnnotation(_annotationName).UpdateConfigurationSource(configurationSource);

        private SequenceData GetData() => SequenceData.Deserialize((string)Model[_annotationName]);

        private void SetData(SequenceData data)
        {
            Model[_annotationName] = data.Serialize();
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        IConventionModel IConventionSequence.Model => (IConventionModel)Model;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public static ISequence FindSequence([NotNull] IModel model, [NotNull] string name, [CanBeNull] string schema)
        {
            var annotationName = BuildSequenceAnnotationName(RelationalAnnotationNames.SequencePrefix, name, schema);

            return model[annotationName] == null ? null : new Sequence(model, annotationName);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public static ISequence RemoveSequence([NotNull] IMutableModel model, [NotNull] string name, [CanBeNull] string schema = null)
        {
            Check.NotEmpty(name, nameof(name));
            Check.NullButNotEmpty(schema, nameof(schema));

            var annotationName = BuildSequenceAnnotationName(RelationalAnnotationNames.SequencePrefix, name, schema);
            return model.RemoveAnnotation(annotationName) == null ? null : new Sequence(model, annotationName);
        }

        private static string BuildSequenceAnnotationName(string annotationPrefix, string name, string schema)
            => annotationPrefix + schema + "." + name;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        IModel ISequence.Model => _model;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        void IConventionSequence.SetStartValue(long? startValue, bool fromDataAnnotation)
            => SetStartValue(startValue, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        void IConventionSequence.SetIncrementBy(int? incrementBy, bool fromDataAnnotation)
            => SetIncrementBy(incrementBy, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        void IConventionSequence.SetMinValue(long? minValue, bool fromDataAnnotation)
            => SetMinValue(minValue, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        void IConventionSequence.SetMaxValue(long? maxValue, bool fromDataAnnotation)
            => SetMaxValue(maxValue, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        void IConventionSequence.SetClrType(Type clrType, bool fromDataAnnotation)
            => SetClrType(clrType, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        void IConventionSequence.SetIsCyclic(bool? cyclic, bool fromDataAnnotation)
            => SetIsCyclic(cyclic, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

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

                var extracted = value[position..end].Replace("''", "'");
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
