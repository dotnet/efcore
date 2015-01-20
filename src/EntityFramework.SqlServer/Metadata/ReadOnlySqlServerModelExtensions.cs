// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Relational.Metadata;
using Microsoft.Data.Entity.Utilities;
using System.Linq;

namespace Microsoft.Data.Entity.SqlServer.Metadata
{
    public class ReadOnlySqlServerModelExtensions : ReadOnlyRelationalModelExtensions, ISqlServerModelExtensions
    {
        protected const string SqlServerValueGenerationAnnotation = SqlServerAnnotationNames.Prefix + SqlServerAnnotationNames.ValueGeneration;
        protected const string SqlServerSequenceAnnotation = SqlServerAnnotationNames.Prefix + RelationalAnnotationNames.Sequence;
        protected const string SqlServerDefaultSequenceNameAnnotation = SqlServerAnnotationNames.Prefix + SqlServerAnnotationNames.DefaultSequenceName;
        protected const string SqlServerDefaultSequenceSchemaAnnotation = SqlServerAnnotationNames.Prefix + SqlServerAnnotationNames.DefaultSequenceSchema;

        public ReadOnlySqlServerModelExtensions([NotNull] IModel model)
            : base(model)
        {
        }

        public virtual SqlServerValueGenerationStrategy? ValueGenerationStrategy
        {
            get
            {
                // TODO: Issue #777: Non-string annotations
                var value = Model[SqlServerValueGenerationAnnotation];
                return value == null ? null : (SqlServerValueGenerationStrategy?)Enum.Parse(typeof(SqlServerValueGenerationStrategy), value);
            }
        }

        public virtual string DefaultSequenceName
        {
            get { return Model[SqlServerDefaultSequenceNameAnnotation]; }
        }

        public virtual string DefaultSequenceSchema
        {
            get { return Model[SqlServerDefaultSequenceSchemaAnnotation]; }
        }

        public override IReadOnlyList<Sequence> Sequences
        {
            get
            {
                var sqlServerSequences = (
                        from a in Model.Annotations
                        where a.Name.StartsWith(SqlServerSequenceAnnotation)
                        select Sequence.Deserialize(a.Value))
                    .ToList();

                return base.Sequences
                    .Where(rs => !sqlServerSequences.Any(ss => ss.Name == rs.Name && ss.Schema == rs.Schema))
                    .Concat(sqlServerSequences)
                    .ToList();
            }
        }

        public override Sequence TryGetSequence(string name, string schema = null)
        {
            Check.NotEmpty(name, "name");
            Check.NullButNotEmpty(schema, "schema");

            return FindSequence(SqlServerSequenceAnnotation + schema + "." + name)
                   ?? base.TryGetSequence(name, schema);
        }
    }
}
