// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Relational.Metadata;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.SqlServer.Metadata
{
    public class ReadOnlySqlServerPropertyExtensions : ISqlServerPropertyExtensions
    {
        protected const string SqlServerNameAnnotation = SqlServerAnnotationNames.Prefix + RelationalAnnotationNames.ColumnName;
        protected const string SqlServerColumnTypeAnnotation = SqlServerAnnotationNames.Prefix + RelationalAnnotationNames.ColumnType;
        protected const string SqlServerDefaultExpressionAnnotation = SqlServerAnnotationNames.Prefix + RelationalAnnotationNames.ColumnDefaultExpression;
        protected const string SqlServerValueGenerationAnnotation = SqlServerAnnotationNames.Prefix + SqlServerAnnotationNames.ValueGeneration;
        protected const string SqlServerComputedExpressionAnnotation = SqlServerAnnotationNames.Prefix + SqlServerAnnotationNames.ColumnComputedExpression;
        protected const string SqlServerSequenceNameAnnotation = SqlServerAnnotationNames.Prefix + SqlServerAnnotationNames.SequenceName;
        protected const string SqlServerSequenceSchemaAnnotation = SqlServerAnnotationNames.Prefix + SqlServerAnnotationNames.SequenceSchema;
        protected const string SqlServerDefaultValueAnnotation = SqlServerAnnotationNames.Prefix + RelationalAnnotationNames.ColumnDefaultValue;
        protected const string SqlServerDefaultValueTypeAnnotation = SqlServerAnnotationNames.Prefix + RelationalAnnotationNames.ColumnDefaultValueType;

        public ReadOnlySqlServerPropertyExtensions([NotNull] IProperty property)
        {
            Check.NotNull(property, nameof(property));

            Property = property;
        }

        public virtual string Column
            => Property[SqlServerNameAnnotation] as string
               ?? Property.Relational().Column;

        public virtual string ColumnType
            => Property[SqlServerColumnTypeAnnotation] as string
               ?? Property.Relational().ColumnType;

        public virtual string DefaultExpression
            => Property[SqlServerDefaultExpressionAnnotation] as string
               ?? Property.Relational().DefaultExpression;

        public virtual string ComputedExpression
            => Property[SqlServerComputedExpressionAnnotation] as string;

        public virtual SqlServerValueGenerationStrategy? ValueGenerationStrategy
        {
            get
            {
                // TODO: Issue #777: Non-string annotations
                var value = Property[SqlServerValueGenerationAnnotation] as string;

                var strategy = value == null
                    ? null
                    : (SqlServerValueGenerationStrategy?)Enum.Parse(typeof(SqlServerValueGenerationStrategy), value);

                return (strategy == null
                            && Property.StoreGeneratedPattern == StoreGeneratedPattern.Identity)
                        || strategy == SqlServerValueGenerationStrategy.Default
                    ? Property.EntityType.Model.SqlServer().ValueGenerationStrategy
                    : strategy;
            }
        }

        public virtual string SequenceName => Property[SqlServerSequenceNameAnnotation] as string;
        public virtual string SequenceSchema => Property[SqlServerSequenceSchemaAnnotation] as string;

        public virtual Sequence TryGetSequence()
        {
            var modelExtensions = Property.EntityType.Model.SqlServer();

            if (ValueGenerationStrategy != SqlServerValueGenerationStrategy.Sequence)
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

        protected virtual IProperty Property { get; }
    }
}
