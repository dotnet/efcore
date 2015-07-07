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
        protected const string SqlServerHiLoSequenceNameAnnotation = SqlServerAnnotationNames.Prefix + SqlServerAnnotationNames.HiLoSequenceName;
        protected const string SqlServerHiLoSequenceSchemaAnnotation = SqlServerAnnotationNames.Prefix + SqlServerAnnotationNames.HiLoSequenceSchema;
        protected const string SqlServerHiLoSequencePoolSizeAnnotation = SqlServerAnnotationNames.Prefix + SqlServerAnnotationNames.HiLoSequencePoolSize;

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

        public virtual string HiLoSequenceName => Model[SqlServerHiLoSequenceNameAnnotation] as string;
        public virtual string HiLoSequenceSchema => Model[SqlServerHiLoSequenceSchemaAnnotation] as string;
        public virtual int? HiLoSequencePoolSize => Model[SqlServerHiLoSequencePoolSizeAnnotation] as int?;

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
