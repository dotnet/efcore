// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.SqlServer.Metadata
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

        protected virtual bool SetHiLoSequenceName(string value)
            => Annotations.SetAnnotation(SqlServerAnnotationNames.HiLoSequenceName, Check.NullButNotEmpty(value, nameof(value)));

        public virtual string HiLoSequenceSchema
        {
            get { return (string)Annotations.GetAnnotation(SqlServerAnnotationNames.HiLoSequenceSchema); }
            [param: CanBeNull] set { SetHiLoSequenceSchema(value); }
        }

        protected virtual bool SetHiLoSequenceSchema(string value)
            => Annotations.SetAnnotation(SqlServerAnnotationNames.HiLoSequenceSchema, Check.NullButNotEmpty(value, nameof(value)));

        public virtual int? HiLoSequencePoolSize
        {
            get { return (int?)Annotations.GetAnnotation(SqlServerAnnotationNames.HiLoSequencePoolSize); }
            [param: CanBeNull] set { SetHiLoSequencePoolSize(value); }
        }

        protected virtual bool SetHiLoSequencePoolSize(int? value)
        {
            if (value <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(value), Entity.Internal.Strings.HiLoBadPoolSize);
            }

            return Annotations.SetAnnotation(SqlServerAnnotationNames.HiLoSequencePoolSize, value);
        }

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

                var value = (SqlServerIdentityStrategy?)Annotations.GetAnnotation(SqlServerAnnotationNames.ValueGenerationStrategy);

                return value ?? Property.DeclaringEntityType.Model.SqlServer().IdentityStrategy;
            }
            [param: CanBeNull]
            set { SetIdentityStrategy(value); }
        }

        protected virtual bool SetIdentityStrategy(SqlServerIdentityStrategy? value)
        {
            if (value != null)
            {
                var propertyType = Property.ClrType;

                if (value == SqlServerIdentityStrategy.IdentityColumn
                    && (!propertyType.IsInteger()
                        || propertyType == typeof(byte)
                        || propertyType == typeof(byte?)))
                {
                    throw new ArgumentException(Strings.IdentityBadType(
                        Property.Name, Property.DeclaringEntityType.Name, propertyType.Name));
                }

                if (value == SqlServerIdentityStrategy.SequenceHiLo
                    && !propertyType.IsInteger())
                {
                    throw new ArgumentException(Strings.SequenceBadType(
                        Property.Name, Property.DeclaringEntityType.Name, propertyType.Name));
                }
            }

            return Annotations.SetAnnotation(SqlServerAnnotationNames.ValueGenerationStrategy, value);
        }

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
