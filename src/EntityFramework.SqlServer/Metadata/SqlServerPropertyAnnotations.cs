// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.SqlServer.Metadata
{
    public class SqlServerPropertyAnnotations : ReadOnlySqlServerPropertyAnnotations
    {
        public SqlServerPropertyAnnotations([NotNull] Property property)
            : base(property)
        {
        }

        public new virtual string Column
        {
            get { return base.Column; }
            [param: CanBeNull]
            set
            {
                Check.NullButNotEmpty(value, nameof(value));

                ((Property)Property)[SqlServerNameAnnotation] = value;
            }
        }

        [CanBeNull]
        public new virtual string ColumnType
        {
            get { return base.ColumnType; }
            [param: CanBeNull]
            set
            {
                Check.NullButNotEmpty(value, nameof(value));

                ((Property)Property)[SqlServerColumnTypeAnnotation] = value;
            }
        }

        [CanBeNull]
        public new virtual string GeneratedValueSql
        {
            get { return base.GeneratedValueSql; }
            [param: CanBeNull]
            set
            {
                Check.NullButNotEmpty(value, nameof(value));

                ((Property)Property)[SqlServerGeneratedValueSqlAnnotation] = value;
            }
        }

        public new virtual object DefaultValue
        {
            get { return base.DefaultValue; }
            [param: CanBeNull]
            set
            {
                var typedAnnotation = new TypedAnnotation(value);

                ((Property)Property)[SqlServerDefaultValueTypeAnnotation] = typedAnnotation.TypeString;
                ((Property)Property)[SqlServerDefaultValueAnnotation] = typedAnnotation.ValueString;
            }
        }

        [CanBeNull]
        public new virtual string HiLoSequenceName
        {
            get { return base.HiLoSequenceName; }
            [param: CanBeNull]
            set
            {
                Check.NullButNotEmpty(value, nameof(value));

                ((Property)Property)[SqlServerHiLoSequenceNameAnnotation] = value;
            }
        }

        [CanBeNull]
        public new virtual string HiLoSequenceSchema
        {
            get { return base.HiLoSequenceSchema; }
            [param: CanBeNull]
            set
            {
                Check.NullButNotEmpty(value, nameof(value));

                ((Property)Property)[SqlServerHiLoSequenceSchemaAnnotation] = value;
            }
        }

        [CanBeNull]
        public new virtual int? HiLoSequencePoolSize
        {
            get { return base.HiLoSequencePoolSize; }
            [param: CanBeNull]
            set
            {
                if (value <= 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(value));
                }

                ((Property)Property)[SqlServerHiLoSequencePoolSizeAnnotation] = value;
            }
        }

        [CanBeNull]
        public new virtual SqlServerIdentityStrategy? IdentityStrategy
        {
            get { return base.IdentityStrategy; }
            [param: CanBeNull]
            set
            {
                var property = ((Property)Property);

                if (value == null)
                {
                    property[SqlServerValueGenerationAnnotation] = null;
                }
                else
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

                    // TODO: Issue #777: Non-string annotations
                    property[SqlServerValueGenerationAnnotation] = value.ToString();
                }
            }
        }
    }
}
