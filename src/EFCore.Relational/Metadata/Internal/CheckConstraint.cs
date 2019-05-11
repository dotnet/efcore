// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Metadata.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public class CheckConstraint : IConventionCheckConstraint
    {
        private readonly string _keyName;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public CheckConstraint(
            [NotNull] IMutableModel model,
            [NotNull] string name,
            [NotNull] string sql,
            [NotNull] string table,
            [CanBeNull] string schema,
            ConfigurationSource configurationSource)
            : this(model, GetKeyName(name, table, schema))
        {
            Check.NotEmpty(name, nameof(name));
            Check.NotEmpty(sql, nameof(sql));
            Check.NotEmpty(table, nameof(table));
            Check.NullButNotEmpty(schema, nameof(schema));

            var dataDictionary = GetAnnotationsDictionary(model);
            if (dataDictionary == null)
            {
                dataDictionary = new Dictionary<string, string>();
                model[RelationalAnnotationNames.CheckConstraints] = dataDictionary;
            }

            var data = new CheckConstraintData
            {
                Name = name,
                Sql = sql,
                Table = table,
                Schema = schema,
                ConfigurationSource = configurationSource
            }.Serialize();

            dataDictionary.Add(_keyName, data);
        }

        private CheckConstraint([NotNull] IModel model, [NotNull] string keyName)
        {
            Check.NotNull(model, nameof(model));

            Model = model;
            _keyName = keyName;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public static IReadOnlyList<CheckConstraint> GetCheckConstraints([NotNull] IModel model)
        {
            Check.NotNull(model, nameof(model));

            return GetAnnotationsDictionary(model)?
                       .Select(a => new CheckConstraint(model, a.Key)).ToList() ?? new List<CheckConstraint>();
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public static ICheckConstraint FindCheckConstraint(
            [NotNull] IModel model, [NotNull] string name, [NotNull] string table, [CanBeNull] string schema)
        {
            var dataDictionary = GetAnnotationsDictionary(model);
            var annotationKey = GetKeyName(name, table, schema);

            return dataDictionary?.ContainsKey(annotationKey) == true ? new CheckConstraint(model, annotationKey) : null;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public static ICheckConstraint RemoveCheckConstraint(
            [NotNull] IMutableModel model, [NotNull] string name, [NotNull] string table, [CanBeNull] string schema)
        {
            var dataDictionary = GetAnnotationsDictionary(model);
            var annotationKey = GetKeyName(name, table, schema);

            return dataDictionary.Remove(annotationKey) ? new CheckConstraint(model, annotationKey) : null;
        }

        private static string GetKeyName(string name, string table, string schema = null)
            => $"{schema}.{table}:{name}";

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual IModel Model { get; }

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
        public virtual string Table => GetData().Table;

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
        public virtual string Sql => GetData().Sql;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual ConfigurationSource GetConfigurationSource() => GetData().ConfigurationSource;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual void UpdateConfigurationSource(ConfigurationSource configurationSource)
        {
            var data = GetData();
            data.ConfigurationSource = configurationSource.Max(data.ConfigurationSource);
            SetData(data);
        }

        private CheckConstraintData GetData() => CheckConstraintData.Deserialize(GetAnnotationsDictionary(Model)?[_keyName]);

        private void SetData(CheckConstraintData data)
        {
            var dataDictionary = GetAnnotationsDictionary(Model);
            if (dataDictionary == null)
            {
                dataDictionary = new Dictionary<string, string>();
                ((IMutableModel)Model)[RelationalAnnotationNames.CheckConstraints] = dataDictionary;
            }

            dataDictionary[_keyName] = data.Serialize();
        }

        private static Dictionary<string, string> GetAnnotationsDictionary(IModel model) =>
            (Dictionary<string, string>)model[RelationalAnnotationNames.CheckConstraints];

        /// <inheritdoc />
        IConventionModel IConventionCheckConstraint.Model => (IConventionModel)Model;

        private class CheckConstraintData
        {
            public string Name { get; set; }

            public string Table { get; set; }

            public string Schema { get; set; }

            public string Sql { get; set; }

            public ConfigurationSource ConfigurationSource { get; set; }

            public string Serialize()
            {
                var builder = new StringBuilder();

                EscapeAndQuote(builder, Name);
                builder.Append(", ");
                EscapeAndQuote(builder, Table);
                builder.Append(", ");
                EscapeAndQuote(builder, Schema);
                builder.Append(", ");
                EscapeAndQuote(builder, Sql);
                builder.Append(", ");
                EscapeAndQuote(builder, ConfigurationSource);

                return builder.ToString();
            }

            public static CheckConstraintData Deserialize(string value)
            {
                try
                {
                    var data = new CheckConstraintData();

                    // ReSharper disable PossibleInvalidOperationException
                    var position = 0;
                    data.Name = ExtractValue(value, ref position);
                    data.Table = ExtractValue(value, ref position);
                    data.Schema = ExtractValue(value, ref position);
                    data.Sql = ExtractValue(value, ref position);
                    data.ConfigurationSource =
                        (ConfigurationSource)Enum.Parse(typeof(ConfigurationSource), ExtractValue(value, ref position));
                    // ReSharper restore PossibleInvalidOperationException

                    return data;
                }
                catch (Exception ex)
                {
                    throw new ArgumentException(RelationalStrings.BadCheckConstraintString, ex);
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
