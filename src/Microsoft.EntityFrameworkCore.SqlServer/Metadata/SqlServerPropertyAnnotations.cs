// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Metadata
{
    public class SqlServerPropertyAnnotations : RelationalPropertyAnnotations, ISqlServerPropertyAnnotations
    {
        public SqlServerPropertyAnnotations([NotNull] IProperty property)
            : base(property, SqlServerFullAnnotationNames.Instance)
        {
        }

        protected SqlServerPropertyAnnotations([NotNull] RelationalAnnotations annotations)
            : base(annotations, SqlServerFullAnnotationNames.Instance)
        {
        }

        public virtual string HiLoSequenceName
        {
            get { return (string)Annotations.GetAnnotation(SqlServerFullAnnotationNames.Instance.HiLoSequenceName, null); }
            [param: CanBeNull] set { SetHiLoSequenceName(value); }
        }

        protected virtual bool SetHiLoSequenceName([CanBeNull] string value)
            => Annotations.SetAnnotation(
                SqlServerFullAnnotationNames.Instance.HiLoSequenceName,
                null,
                Check.NullButNotEmpty(value, nameof(value)));

        public virtual string HiLoSequenceSchema
        {
            get { return (string)Annotations.GetAnnotation(SqlServerFullAnnotationNames.Instance.HiLoSequenceSchema, null); }
            [param: CanBeNull] set { SetHiLoSequenceSchema(value); }
        }

        protected virtual bool SetHiLoSequenceSchema([CanBeNull] string value)
            => Annotations.SetAnnotation(
                SqlServerFullAnnotationNames.Instance.HiLoSequenceSchema,
                null,
                Check.NullButNotEmpty(value, nameof(value)));

        public virtual SqlServerValueGenerationStrategy? ValueGenerationStrategy
        {
            get
            {
                if (Property.ValueGenerated != ValueGenerated.OnAdd
                    || !Property.ClrType.UnwrapNullableType().IsInteger()
                    || Property.SqlServer().DefaultValueSql != null)
                {
                    return null;
                }

                var value = (SqlServerValueGenerationStrategy?)Annotations.GetAnnotation(
                    SqlServerFullAnnotationNames.Instance.ValueGenerationStrategy,
                    null);

                return value ?? Property.DeclaringEntityType.Model.SqlServer().ValueGenerationStrategy;
            }
            [param: CanBeNull] set { SetValueGenerationStrategy(value); }
        }

        protected virtual bool SetValueGenerationStrategy(SqlServerValueGenerationStrategy? value)
        {
            if (value != null)
            {
                var propertyType = Property.ClrType;

                if (value == SqlServerValueGenerationStrategy.IdentityColumn
                    && (!propertyType.IsInteger()
                        || propertyType == typeof(byte)
                        || propertyType == typeof(byte?)))
                {
                    throw new ArgumentException(SqlServerStrings.IdentityBadType(
                        Property.Name, Property.DeclaringEntityType.Name, propertyType.Name));
                }

                if ((value == SqlServerValueGenerationStrategy.SequenceHiLo)
                    && !propertyType.IsInteger())
                {
                    throw new ArgumentException(SqlServerStrings.SequenceBadType(
                        Property.Name, Property.DeclaringEntityType.Name, propertyType.Name));
                }
            }

            return Annotations.SetAnnotation(SqlServerFullAnnotationNames.Instance.ValueGenerationStrategy, null, value);
        }

        public virtual ISequence FindHiLoSequence()
        {
            var modelExtensions = Property.DeclaringEntityType.Model.SqlServer();

            if (ValueGenerationStrategy != SqlServerValueGenerationStrategy.SequenceHiLo)
            {
                return null;
            }

            var sequenceName = HiLoSequenceName
                               ?? modelExtensions.HiLoSequenceName
                               ?? SqlServerModelAnnotations.DefaultHiLoSequenceName;

            var sequenceSchema = HiLoSequenceSchema
                                 ?? modelExtensions.HiLoSequenceSchema;

            return modelExtensions.FindSequence(sequenceName, sequenceSchema);
        }
    }
}
