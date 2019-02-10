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
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class CheckConstraint : ICheckConstraint
    {
        private readonly IModel _model;
        private readonly string _annotationName;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public CheckConstraint(
            [NotNull] IMutableModel model,
            [NotNull] string name,
            [NotNull] string constraintSql,
            [NotNull] string table,
            [CanBeNull] string schema = null)
            : this(model, GetAnnotationKey(name, table, schema))
        {
            Check.NotEmpty(name, nameof(name));
            Check.NotEmpty(constraintSql, nameof(constraintSql));
            Check.NotEmpty(table, nameof(table));
            Check.NullButNotEmpty(schema, nameof(schema));

            SetData(
                new CheckContraintData
                {
                    Name = name,
                    ConstraintSql = constraintSql,
                    Table = table,
                    Schema = schema
                });
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public CheckConstraint([NotNull] IModel model, [NotNull] string annotationName)
        {
            Check.NotNull(model, nameof(model));

            _model = model;
            _annotationName = annotationName;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static IEnumerable<CheckConstraint> GetCheckConstraints([NotNull] IModel model)
        {
            Check.NotNull(model, nameof(model));

            return GetAnnotationsDictionary(model)?
                .Select(a => new CheckConstraint(model, a.Key)) ?? Enumerable.Empty<CheckConstraint>();
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static ICheckConstraint FindCheckConstraint([NotNull] IModel model, [NotNull] string name, [NotNull] string table, [CanBeNull] string schema = null)
        {
            Check.NotNull(model, nameof(model));
            Check.NotEmpty(name, nameof(name));
            Check.NotEmpty(table, nameof(table));
            Check.NullButNotEmpty(schema, nameof(schema));

            var dataDictionary = GetAnnotationsDictionary(model);
            var annotationKey = GetAnnotationKey(name, table, schema);

            return dataDictionary?.ContainsKey(annotationKey) == true ? new CheckConstraint(model, annotationKey) : null;
        }

        private static string GetAnnotationKey(string name, string table, string schema = null)
            => $"{schema}.{table}:{name}";

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
        public virtual string Table => GetData().Table;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual string Schema => GetData().Schema ?? Model.Relational().DefaultSchema;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual string ConstraintSql
        {
            get => GetData().ConstraintSql;
            set
            {
                var data = GetData();
                data.ConstraintSql = value;
                SetData(data);
            }
        }

        private CheckContraintData GetData()
        {
            var dataDictionary = GetAnnotationsDictionary(Model);
            return CheckContraintData.Deserialize(dataDictionary?[_annotationName]);
        }

        private void SetData(CheckContraintData data)
        {
            var dataDictionary = GetAnnotationsDictionary(Model);
            if (dataDictionary == null)
            {
                dataDictionary = new Dictionary<string, string>();
                Model[RelationalAnnotationNames.CheckConstraints] = dataDictionary;
            }
            dataDictionary[_annotationName] = data.Serialize();
        }

        internal static Dictionary<string, string> GetAnnotationsDictionary(IModel model) =>
            (Dictionary<string, string>)model[RelationalAnnotationNames.CheckConstraints];

        IModel ICheckConstraint.Model => _model;

        private class CheckContraintData
        {
            public string Name { get; set; }

            public string Table { get; set; }

            public string Schema { get; set; }

            public string ConstraintSql { get; set; }

            public string Serialize()
            {
                var builder = new StringBuilder();

                EscapeAndQuote(builder, Name);
                builder.Append(", ");
                EscapeAndQuote(builder, Table);
                builder.Append(", ");
                EscapeAndQuote(builder, Schema);
                builder.Append(", ");
                EscapeAndQuote(builder, ConstraintSql);

                return builder.ToString();
            }

            public static CheckContraintData Deserialize([NotNull] string value)
            {
                Check.NotEmpty(value, nameof(value));

                try
                {
                    var data = new CheckContraintData();

                    // ReSharper disable PossibleInvalidOperationException
                    var position = 0;
                    data.Name = ExtractValue(value, ref position);
                    data.Table = ExtractValue(value, ref position);
                    data.Schema = ExtractValue(value, ref position);
                    data.ConstraintSql = ExtractValue(value, ref position);
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
