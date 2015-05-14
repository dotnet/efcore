// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Relational.Metadata;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.SqlServer.Metadata
{
    public class ReadOnlySqlServerModelExtensions : ISqlServerModelExtensions
    {
        protected const string SqlServerValueGenerationAnnotation = SqlServerAnnotationNames.Prefix + SqlServerAnnotationNames.ValueGeneration;
        protected const string SqlServerSequenceAnnotation = SqlServerAnnotationNames.Prefix + RelationalAnnotationNames.Sequence;
        protected const string SqlServerDefaultSequenceNameAnnotation = SqlServerAnnotationNames.Prefix + SqlServerAnnotationNames.DefaultSequenceName;
        protected const string SqlServerDefaultSequenceSchemaAnnotation = SqlServerAnnotationNames.Prefix + SqlServerAnnotationNames.DefaultSequenceSchema;

        public ReadOnlySqlServerModelExtensions([NotNull] IModel model)
        {
            Check.NotNull(model, nameof(model));

            Model = model;
        }

        public virtual SqlServerValueGenerationStrategy? ValueGenerationStrategy
        {
            get
            {
                // TODO: Issue #777: Non-string annotations
                var value = Model[SqlServerValueGenerationAnnotation] as string;

                return value == null
                    ? null
                    : (SqlServerValueGenerationStrategy?)Enum.Parse(typeof(SqlServerValueGenerationStrategy), value);
            }
        }

        public virtual string DefaultSequenceName => Model[SqlServerDefaultSequenceNameAnnotation] as string;
        public virtual string DefaultSequenceSchema => Model[SqlServerDefaultSequenceSchemaAnnotation] as string;

        public virtual IReadOnlyList<Sequence> Sequences
        {
            get
            {
                var sqlServerSequences = (
                    from a in Model.Annotations
                    where a.Name.StartsWith(SqlServerSequenceAnnotation)
                    select Sequence.Deserialize((string)a.Value))
                    .ToList();

                return Model.Relational().Sequences
                    .Where(rs => !sqlServerSequences.Any(ss => ss.Name == rs.Name && ss.Schema == rs.Schema))
                    .Concat(sqlServerSequences)
                    .ToList();
            }
        }

        public virtual Sequence TryGetSequence(string name, string schema = null)
        {
            Check.NotEmpty(name, nameof(name));
            Check.NullButNotEmpty(schema, nameof(schema));

            return FindSequence(SqlServerSequenceAnnotation + schema + "." + name)
                   ?? Model.Relational().TryGetSequence(name, schema);
        }

        protected virtual Sequence FindSequence([NotNull] string annotationName)
        {
            Check.NotEmpty(annotationName, nameof(annotationName));

            var value = Model[annotationName];
            if (value == null)
            {
                return null;
            }

            var sequence = Sequence.Deserialize((string)value);
            sequence.Model = Model;
            return sequence;
        }

        protected virtual IModel Model { get; }
    }
}
