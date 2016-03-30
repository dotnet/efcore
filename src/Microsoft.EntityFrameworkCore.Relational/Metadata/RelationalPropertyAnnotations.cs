// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Metadata
{
    public class RelationalPropertyAnnotations : IRelationalPropertyAnnotations
    {
        protected readonly RelationalFullAnnotationNames ProviderFullAnnotationNames;

        public RelationalPropertyAnnotations([NotNull] IProperty property,
            [CanBeNull] RelationalFullAnnotationNames providerFullAnnotationNames)
            : this(new RelationalAnnotations(property), providerFullAnnotationNames)
        {
        }

        protected RelationalPropertyAnnotations([NotNull] RelationalAnnotations annotations,
            [CanBeNull] RelationalFullAnnotationNames providerFullAnnotationNames)
        {
            Annotations = annotations;
            ProviderFullAnnotationNames = providerFullAnnotationNames;
        }

        protected virtual RelationalAnnotations Annotations { get; }
        protected virtual IProperty Property => (IProperty)Annotations.Metadata;

        public virtual string ColumnName
        {
            get
            {
                return (string)Annotations.GetAnnotation(
                    RelationalFullAnnotationNames.Instance.ColumnName,
                    ProviderFullAnnotationNames?.ColumnName)
                       ?? Property.Name;
            }
            [param: CanBeNull] set { SetColumnName(value); }
        }

        protected virtual bool SetColumnName([CanBeNull] string value)
            => Annotations.SetAnnotation(
                RelationalFullAnnotationNames.Instance.ColumnName,
                ProviderFullAnnotationNames?.ColumnName,
                Check.NullButNotEmpty(value, nameof(value)));

        public virtual string ColumnType
        {
            get
            {
                return (string)Annotations.GetAnnotation(
                    RelationalFullAnnotationNames.Instance.ColumnType,
                    ProviderFullAnnotationNames?.ColumnType);
            }
            [param: CanBeNull] set { SetColumnType(value); }
        }

        protected virtual bool SetColumnType([CanBeNull] string value)
            => Annotations.SetAnnotation(
                RelationalFullAnnotationNames.Instance.ColumnType,
                ProviderFullAnnotationNames?.ColumnType,
                Check.NullButNotEmpty(value, nameof(value)));

        public virtual string DefaultValueSql
        {
            get
            {
                return (string)Annotations.GetAnnotation(
                    RelationalFullAnnotationNames.Instance.DefaultValueSql,
                    ProviderFullAnnotationNames?.DefaultValueSql);
            }
            [param: CanBeNull] set { SetDefaultValueSql(value); }
        }

        protected virtual bool SetDefaultValueSql([CanBeNull] string value)
            => Annotations.SetAnnotation(
                RelationalFullAnnotationNames.Instance.DefaultValueSql,
                ProviderFullAnnotationNames?.DefaultValueSql,
                Check.NullButNotEmpty(value, nameof(value)));

        public virtual string ComputedValueSql
        {
            get
            {
                return (string)Annotations.GetAnnotation(
                    RelationalFullAnnotationNames.Instance.ComputedValueSql,
                    ProviderFullAnnotationNames?.ComputedValueSql);
            }
            [param: CanBeNull] set { SetComputedValueSql(value); }
        }

        protected virtual bool SetComputedValueSql([CanBeNull] string value)
            => Annotations.SetAnnotation(
                RelationalFullAnnotationNames.Instance.ComputedValueSql,
                ProviderFullAnnotationNames?.ComputedValueSql,
                Check.NullButNotEmpty(value, nameof(value)));

        public virtual object DefaultValue
        {
            get
            {
                return Annotations.GetAnnotation(
                    RelationalFullAnnotationNames.Instance.DefaultValue,
                    ProviderFullAnnotationNames?.DefaultValue);
            }
            [param: CanBeNull] set { SetDefaultValue(value); }
        }

        protected virtual bool SetDefaultValue([CanBeNull] object value)
        {
            if (value != null)
            {
                var valueType = value.GetType();
                if (Property.ClrType.UnwrapNullableType() != valueType)
                {
                    throw new InvalidOperationException(
                        RelationalStrings.IncorrectDefaultValueType(
                            value, valueType, Property.Name, Property.ClrType, Property.DeclaringEntityType.DisplayName()));
                }

                if (valueType.GetTypeInfo().IsEnum)
                {
                    value = Convert.ChangeType(value, valueType.UnwrapEnumType());
                }
            }

            return Annotations.SetAnnotation(
                RelationalFullAnnotationNames.Instance.DefaultValue,
                ProviderFullAnnotationNames?.DefaultValue,
                value);
        }
    }
}
