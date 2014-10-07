// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Relational.Metadata;
using Microsoft.Data.Entity.SqlServer.Utilities;

namespace Microsoft.Data.Entity.SqlServer.Metadata
{
    public class ReadOnlySqlServerModelExtensions : ReadOnlyRelationalModelExtensions, ISqlServerModelExtensions
    {
        protected const string SqlServerValueGenerationAnnotation = SqlServerAnnotationNames.Prefix + SqlServerAnnotationNames.ValueGeneration;
        protected const string SqlServerSequenceAnnotation = SqlServerAnnotationNames.Prefix + RelationalAnnotationNames.Sequence;
        protected const string SqlServerDefaultSequenceNameAnnotation = SqlServerAnnotationNames.Prefix + SqlServerAnnotationNames.DefaultSequenceName;

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

        public override Sequence TryGetSequence(string name, string schema = null)
        {
            Check.NotEmpty(name, "name");
            Check.NullButNotEmpty(schema, "schema");

            return FindSequence(SqlServerSequenceAnnotation + schema + "." + name)
                   ?? base.TryGetSequence(name, schema);
        }
    }
}
