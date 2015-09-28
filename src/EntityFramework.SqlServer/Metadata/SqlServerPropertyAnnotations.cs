// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Internal;
using Microsoft.Data.Entity.Metadata.Internal;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Metadata
{
    public class SqlServerPropertyAnnotations : RelationalPropertyAnnotations, ISqlServerPropertyAnnotations
    {
        public SqlServerPropertyAnnotations([NotNull] IProperty property)
            : base(property, SqlServerAnnotationNames.Prefix)
        {
        }

        protected SqlServerPropertyAnnotations([NotNull] RelationalAnnotations annotations)
            : base(annotations)
        {
        }

        public virtual string HiLoSequenceName
        {
            get { return (string)Annotations.GetAnnotation(SqlServerAnnotationNames.HiLoSequenceName); }
            [param: CanBeNull] set { SetHiLoSequenceName(value); }
        }

        protected virtual bool SetHiLoSequenceName([CanBeNull] string value)
            => Annotations.SetAnnotation(SqlServerAnnotationNames.HiLoSequenceName, Check.NullButNotEmpty(value, nameof(value)));

        public virtual string HiLoSequenceSchema
        {
            get { return (string)Annotations.GetAnnotation(SqlServerAnnotationNames.HiLoSequenceSchema); }
            [param: CanBeNull] set { SetHiLoSequenceSchema(value); }
        }

        protected virtual bool SetHiLoSequenceSchema([CanBeNull] string value)
            => Annotations.SetAnnotation(SqlServerAnnotationNames.HiLoSequenceSchema, Check.NullButNotEmpty(value, nameof(value)));

        public virtual SqlServerValueGenerationStrategy? ValueGenerationStrategy
        {
            get
            {
                if (Property.ValueGenerated != ValueGenerated.OnAdd
                    || !Property.ClrType.UnwrapNullableType().IsInteger()
                    || Property.SqlServer().GeneratedValueSql != null)
                {
                    return null;
                }

                var value = (SqlServerValueGenerationStrategy?)Annotations.GetAnnotation(SqlServerAnnotationNames.ValueGenerationStrategy);

                return value ?? Property.DeclaringEntityType.Model.SqlServer().ValueGenerationStrategy;
            }
            [param: CanBeNull]
            set { SetValueGenerationStrategy(value); }
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

                if (value == SqlServerValueGenerationStrategy.SequenceHiLo
                    && !propertyType.IsInteger())
                {
                    throw new ArgumentException(SqlServerStrings.SequenceBadType(
                        Property.Name, Property.DeclaringEntityType.Name, propertyType.Name));
                }
            }

            return Annotations.SetAnnotation(SqlServerAnnotationNames.ValueGenerationStrategy, value);
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
                               ?? SqlServerAnnotationNames.DefaultHiLoSequenceName;

            var sequenceSchema = HiLoSequenceSchema
                                 ?? modelExtensions.HiLoSequenceSchema;

            return modelExtensions.FindSequence(sequenceName, sequenceSchema);
        }
    }
}
