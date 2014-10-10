// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.SqlServer.Utilities;

namespace Microsoft.Data.Entity.SqlServer.Metadata
{
    public class SqlServerPropertyExtensions : ReadOnlySqlServerPropertyExtensions
    {
        public SqlServerPropertyExtensions([NotNull] Property property)
            : base(property)
        {
        }

        public new virtual string Column
        {
            get { return base.Column; }
            [param: CanBeNull]
            set
            {
                Check.NullButNotEmpty(value, "value");

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
                Check.NullButNotEmpty(value, "value");

                ((Property)Property)[SqlServerColumnTypeAnnotation] = value;
            }
        }

        [CanBeNull]
        public new virtual string DefaultExpression
        {
            get { return base.DefaultExpression; }
            [param: CanBeNull]
            set
            {
                Check.NullButNotEmpty(value, "value");

                ((Property)Property)[SqlServerDefaultExpressionAnnotation] = value;
            }
        }

        [CanBeNull]
        public new virtual string SequenceName
        {
            get { return base.SequenceName; }
            [param: CanBeNull]
            set
            {
                Check.NullButNotEmpty(value, "value");

                ((Property)Property)[SqlServerSequenceNameAnnotation] = value;
            }
        }

        [CanBeNull]
        public new virtual string SequenceSchema
        {
            get { return base.SequenceSchema; }
            [param: CanBeNull]
            set
            {
                Check.NullButNotEmpty(value, "value");

                ((Property)Property)[SqlServerSequenceSchemaAnnotation] = value;
            }
        }

        [CanBeNull]
        public new virtual SqlServerValueGenerationStrategy? ValueGenerationStrategy
        {
            get { return base.ValueGenerationStrategy; }
            [param: CanBeNull]
            set
            {
                var property = ((Property)Property);

                if (value == null)
                {
                    property[SqlServerValueGenerationAnnotation] = null;
                    property.ValueGeneration = ValueGeneration.None;
                }
                else
                {
                    var propertyType = Property.PropertyType;

                    if (value == SqlServerValueGenerationStrategy.Identity
                        && (!propertyType.IsInteger()
                            || propertyType == typeof(byte)))
                    {
                        throw new ArgumentException(Strings.FormatIdentityBadType(
                            Property.Name, Property.EntityType.Name, propertyType.Name));
                    }

                    if (value == SqlServerValueGenerationStrategy.Sequence
                        && !propertyType.IsInteger())
                    {
                        throw new ArgumentException(Strings.FormatSequenceBadType(
                            Property.Name, Property.EntityType.Name, propertyType.Name));
                    }

                    // TODO: Issue #777: Non-string annotations
                    property[SqlServerValueGenerationAnnotation] = value.ToString();
                    property.ValueGeneration = ValueGeneration.OnAdd;
                }
            }
        }
    }
}
