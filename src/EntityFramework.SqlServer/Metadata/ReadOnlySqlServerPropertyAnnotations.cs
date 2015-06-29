// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;

namespace Microsoft.Data.Entity.SqlServer.Metadata
{
    public class ReadOnlySqlServerPropertyAnnotations : ReadOnlyRelationalPropertyAnnotations, ISqlServerPropertyAnnotations
    {
        protected const string SqlServerNameAnnotation = SqlServerAnnotationNames.Prefix + RelationalAnnotationNames.ColumnName;
        protected const string SqlServerColumnTypeAnnotation = SqlServerAnnotationNames.Prefix + RelationalAnnotationNames.ColumnType;
        protected const string SqlServerDefaultExpressionAnnotation = SqlServerAnnotationNames.Prefix + RelationalAnnotationNames.ColumnDefaultExpression;
        protected const string SqlServerValueGenerationAnnotation = SqlServerAnnotationNames.Prefix + SqlServerAnnotationNames.ItentityStrategy;
        protected const string SqlServerComputedExpressionAnnotation = SqlServerAnnotationNames.Prefix + SqlServerAnnotationNames.ColumnComputedExpression;
        protected const string SqlServerSequenceNameAnnotation = SqlServerAnnotationNames.Prefix + SqlServerAnnotationNames.SequenceName;
        protected const string SqlServerSequenceSchemaAnnotation = SqlServerAnnotationNames.Prefix + SqlServerAnnotationNames.SequenceSchema;
        protected const string SqlServerDefaultValueAnnotation = SqlServerAnnotationNames.Prefix + RelationalAnnotationNames.ColumnDefaultValue;
        protected const string SqlServerDefaultValueTypeAnnotation = SqlServerAnnotationNames.Prefix + RelationalAnnotationNames.ColumnDefaultValueType;

        public ReadOnlySqlServerPropertyAnnotations([NotNull] IProperty property)
            : base(property)
        {
        }

        public override string Column
            => Property[SqlServerNameAnnotation] as string
               ?? base.Column;

        public override string ColumnType
            => Property[SqlServerColumnTypeAnnotation] as string
               ?? base.ColumnType;

        public override string DefaultValueSql
            => Property[SqlServerDefaultExpressionAnnotation] as string
               ?? base.DefaultValueSql;

        public override object DefaultValue
            => new TypedAnnotation(
                Property[SqlServerDefaultValueTypeAnnotation] as string,
                Property[SqlServerDefaultValueAnnotation] as string).Value
               ?? base.DefaultValue;

        public virtual string ComputedExpression
            => Property[SqlServerComputedExpressionAnnotation] as string;

        public virtual SqlServerIdentityStrategy? IdentityStrategy
        {
            get
            {
                // TODO: Issue #777: Non-string annotations
                var value = Property[SqlServerValueGenerationAnnotation] as string;

                var strategy = value == null
                    ? null
                    : (SqlServerIdentityStrategy?)Enum.Parse(typeof(SqlServerIdentityStrategy), value);

                return strategy ?? Property.EntityType.Model.SqlServer().IdentityStrategy;
            }
        }

        public virtual string SequenceName => Property[SqlServerSequenceNameAnnotation] as string;
        public virtual string SequenceSchema => Property[SqlServerSequenceSchemaAnnotation] as string;

        public virtual Sequence TryGetSequence()
        {
            var modelExtensions = Property.EntityType.Model.SqlServer();

            if (IdentityStrategy != SqlServerIdentityStrategy.SequenceHiLo)
            {
                return null;
            }

            var sequenceName = SequenceName
                               ?? modelExtensions.DefaultSequenceName
                               ?? Sequence.DefaultName;

            var sequenceSchema = SequenceSchema
                                 ?? modelExtensions.DefaultSequenceSchema;

            return modelExtensions.TryGetSequence(sequenceName, sequenceSchema)
                   ?? new Sequence(Sequence.DefaultName);
        }
    }
}
