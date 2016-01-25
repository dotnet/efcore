// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.EntityFrameworkCore.Internal;


namespace Microsoft.EntityFrameworkCore.Metadata
{
    public class RelationalPropertyAnnotations : IRelationalPropertyAnnotations
    {
        public RelationalPropertyAnnotations([NotNull] IProperty property, [CanBeNull] string providerPrefix)
            : this(new RelationalAnnotations(property, providerPrefix))
        {
        }

        protected RelationalPropertyAnnotations([NotNull] RelationalAnnotations annotations)
        {
            Annotations = annotations;
        }

        protected virtual RelationalAnnotations Annotations { get; }

        protected virtual IProperty Property => (IProperty)Annotations.Metadata;

        public virtual string ColumnName
        {
            get { return (string)Annotations.GetAnnotation(RelationalAnnotationNames.ColumnName) ?? Property.Name; }
            [param: CanBeNull] set { SetColumnName(value); }
        }

        protected virtual bool SetColumnName([CanBeNull] string value)
            => Annotations.SetAnnotation(RelationalAnnotationNames.ColumnName, Check.NullButNotEmpty(value, nameof(value)));

        public virtual string ColumnType
        {
            get { return (string)Annotations.GetAnnotation(RelationalAnnotationNames.ColumnType); }
            [param: CanBeNull] set { SetColumnType(value); }
        }

        protected virtual bool SetColumnType([CanBeNull] string value)
            => Annotations.SetAnnotation(RelationalAnnotationNames.ColumnType, Check.NullButNotEmpty(value, nameof(value)));

        public virtual string GeneratedValueSql
        {
            get { return (string)Annotations.GetAnnotation(RelationalAnnotationNames.GeneratedValueSql); }
            [param: CanBeNull] set { SetGeneratedValueSql(value); }
        }

        protected virtual bool SetGeneratedValueSql([CanBeNull] string value)
            => Annotations.SetAnnotation(RelationalAnnotationNames.GeneratedValueSql, Check.NullButNotEmpty(value, nameof(value)));

        public virtual object DefaultValue
        {
            get
            {
                return Annotations.GetAnnotation(RelationalAnnotationNames.DefaultValue);
            }
            [param: CanBeNull] set { SetDefaultValue(value); }
        }

        protected virtual bool SetDefaultValue([CanBeNull] object value)
        {
            if ((value != null)
                && (Property.ClrType.UnwrapNullableType() != value.GetType()))
            {
                throw new InvalidOperationException(RelationalStrings.IncorrectDefaultValueType(value, value.GetType(), Property.Name, Property.ClrType, Property.DeclaringEntityType.DisplayName()));
            }

            return Annotations.SetAnnotation(RelationalAnnotationNames.DefaultValue, value);
        }
    }
}
