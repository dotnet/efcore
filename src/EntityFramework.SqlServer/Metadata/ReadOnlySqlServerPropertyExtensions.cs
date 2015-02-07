// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Relational.Metadata;

namespace Microsoft.Data.Entity.SqlServer.Metadata
{
    public class ReadOnlySqlServerPropertyExtensions : ReadOnlyRelationalPropertyExtensions, ISqlServerPropertyExtensions
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
            : base(property)
        {
        }

        public override string Column
        {
            get { return Property[SqlServerNameAnnotation] ?? base.Column; }
        }

        public override string ColumnType
        {
            get { return Property[SqlServerColumnTypeAnnotation] ?? base.ColumnType; }
        }

        public override string DefaultExpression
        {
            get { return Property[SqlServerDefaultExpressionAnnotation] ?? base.DefaultExpression; }
        }

        public override object DefaultValue
        {
            get
            {
                return new TypedAnnotation(Property[SqlServerDefaultValueTypeAnnotation], Property[SqlServerDefaultValueAnnotation]).Value
                       ?? base.DefaultValue;
            }
        }

        public virtual string ComputedExpression => Property[SqlServerComputedExpressionAnnotation];

        public virtual SqlServerValueGenerationStrategy? ValueGenerationStrategy
        {
            get
            {
                // TODO: Issue #777: Non-string annotations
                var value = Property[SqlServerValueGenerationAnnotation];
                return value == null ? null : (SqlServerValueGenerationStrategy?)Enum.Parse(typeof(SqlServerValueGenerationStrategy), value);
            }
        }

        public virtual string SequenceName
        {
            get { return Property[SqlServerSequenceNameAnnotation]; }
        }

        public virtual string SequenceSchema
        {
            get { return Property[SqlServerSequenceSchemaAnnotation]; }
        }

        public virtual Sequence TryGetSequence()
        {
            var modelExtensions = Property.EntityType.Model.SqlServer();

            if (ValueGenerationStrategy != SqlServerValueGenerationStrategy.Sequence
                && (ValueGenerationStrategy != null
                    || modelExtensions.ValueGenerationStrategy != SqlServerValueGenerationStrategy.Sequence))
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
