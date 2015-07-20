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
        protected const string SqlServerGeneratedValueSqlAnnotation = SqlServerAnnotationNames.Prefix + RelationalAnnotationNames.GeneratedValueSql;
        protected const string SqlServerValueGenerationAnnotation = SqlServerAnnotationNames.Prefix + SqlServerAnnotationNames.ValueGenerationStrategy;
        protected const string SqlServerHiLoSequenceNameAnnotation = SqlServerAnnotationNames.Prefix + SqlServerAnnotationNames.HiLoSequenceName;
        protected const string SqlServerHiLoSequenceSchemaAnnotation = SqlServerAnnotationNames.Prefix + SqlServerAnnotationNames.HiLoSequenceSchema;
        protected const string SqlServerHiLoSequencePoolSizeAnnotation = SqlServerAnnotationNames.Prefix + SqlServerAnnotationNames.HiLoSequencePoolSize;
        protected const string SqlServerDefaultValueAnnotation = SqlServerAnnotationNames.Prefix + RelationalAnnotationNames.DefaultValue;
        protected const string SqlServerDefaultValueTypeAnnotation = SqlServerAnnotationNames.Prefix + RelationalAnnotationNames.ColumnDefaultValueType;

        public ReadOnlySqlServerPropertyAnnotations([NotNull] IProperty property)
            : base(property)
        {
        }

        public override string ColumnName
            => Property[SqlServerNameAnnotation] as string
               ?? base.ColumnName;

        public override string ColumnType
            => Property[SqlServerColumnTypeAnnotation] as string
               ?? base.ColumnType;

        public override string GeneratedValueSql
            => Property[SqlServerGeneratedValueSqlAnnotation] as string
               ?? base.GeneratedValueSql;

        public override object DefaultValue
            => new TypedAnnotation(
                Property[SqlServerDefaultValueTypeAnnotation] as string,
                Property[SqlServerDefaultValueAnnotation] as string).Value
               ?? base.DefaultValue;

        public virtual SqlServerIdentityStrategy? IdentityStrategy
        {
            get
            {
                if (Property.ValueGenerated != ValueGenerated.OnAdd
                    || !Property.ClrType.UnwrapNullableType().IsInteger()
                    || Property.SqlServer().GeneratedValueSql != null)
                {
                    return null;
                }

                // TODO: Issue #777: Non-string annotations
                var value = Property[SqlServerValueGenerationAnnotation] as string;

                var strategy = value == null
                    ? null
                    : (SqlServerIdentityStrategy?)Enum.Parse(typeof(SqlServerIdentityStrategy), value);

                return strategy ?? Property.DeclaringEntityType.Model.SqlServer().IdentityStrategy;
            }
        }

        public virtual string HiLoSequenceName => Property[SqlServerHiLoSequenceNameAnnotation] as string;
        public virtual string HiLoSequenceSchema => Property[SqlServerHiLoSequenceSchemaAnnotation] as string;
        public virtual int? HiLoSequencePoolSize => Property[SqlServerHiLoSequencePoolSizeAnnotation] as int?;

        public virtual ISequence FindHiLoSequence()
        {
            var modelExtensions = Property.DeclaringEntityType.Model.SqlServer();

            if (IdentityStrategy != SqlServerIdentityStrategy.SequenceHiLo)
            {
                return null;
            }

            var sequenceName = HiLoSequenceName
                               ?? modelExtensions.HiLoSequenceName
                               ?? SqlServerAnnotationNames.DefaultHiLoSequenceName;

            var sequenceSchema = HiLoSequenceSchema
                                 ?? modelExtensions.HiLoSequenceSchema;

            return modelExtensions.FindSequence(sequenceName, sequenceSchema);
        }
    }
}
