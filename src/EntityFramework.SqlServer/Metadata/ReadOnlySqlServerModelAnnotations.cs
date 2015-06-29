// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.SqlServer.Metadata
{
    public class ReadOnlySqlServerModelAnnotations : ReadOnlyRelationalModelAnnotations, ISqlServerModelAnnotations
    {
        protected const string SqlServerValueGenerationAnnotation = SqlServerAnnotationNames.Prefix + SqlServerAnnotationNames.ItentityStrategy;
        protected const string SqlServerSequenceAnnotation = SqlServerAnnotationNames.Prefix + RelationalAnnotationNames.Sequence;
        protected const string SqlServerDefaultSequenceNameAnnotation = SqlServerAnnotationNames.Prefix + SqlServerAnnotationNames.DefaultSequenceName;
        protected const string SqlServerDefaultSequenceSchemaAnnotation = SqlServerAnnotationNames.Prefix + SqlServerAnnotationNames.DefaultSequenceSchema;

        public ReadOnlySqlServerModelAnnotations([NotNull] IModel model)
            : base(model)
        {
        }

        public virtual SqlServerIdentityStrategy? IdentityStrategy
        {
            get
            {
                // TODO: Issue #777: Non-string annotations
                var value = Model[SqlServerValueGenerationAnnotation] as string;

                return value == null
                    ? null
                    : (SqlServerIdentityStrategy?)Enum.Parse(typeof(SqlServerIdentityStrategy), value);
            }
        }

        public virtual string DefaultSequenceName => Model[SqlServerDefaultSequenceNameAnnotation] as string;
        public virtual string DefaultSequenceSchema => Model[SqlServerDefaultSequenceSchemaAnnotation] as string;

        public override IReadOnlyList<Sequence> Sequences
        {
            get
            {
                var sqlServerSequences = (
                    from a in Model.Annotations
                    where a.Name.StartsWith(SqlServerSequenceAnnotation)
                    select Sequence.Deserialize((string)a.Value))
                    .ToList();

                return base.Sequences
                    .Where(rs => !sqlServerSequences.Any(ss => ss.Name == rs.Name && ss.Schema == rs.Schema))
                    .Concat(sqlServerSequences)
                    .ToList();
            }
        }

        public override Sequence TryGetSequence(string name, string schema = null)
        {
            Check.NotEmpty(name, nameof(name));
            Check.NullButNotEmpty(schema, nameof(schema));

            return FindSequence(SqlServerSequenceAnnotation + schema + "." + name)
                   ?? base.TryGetSequence(name, schema);
        }
    }
}
