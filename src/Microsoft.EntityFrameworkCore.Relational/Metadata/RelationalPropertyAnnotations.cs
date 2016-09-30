// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Globalization;
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
        protected virtual bool ShouldThrowOnConflict => true;

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
            get { return GetDefaultValueSql(true); }
            [param: CanBeNull] set { SetDefaultValueSql(value); }
        }

        protected virtual string GetDefaultValueSql(bool fallback)
        {
            if (ProviderFullAnnotationNames != null)
            {
                if (fallback
                    && (GetDefaultValue(false) != null
                        || GetComputedColumnSql(false) != null))
                {
                    return null;
                }

                return (string)Annotations.GetAnnotation(
                    fallback ? RelationalFullAnnotationNames.Instance.DefaultValueSql : null,
                    ProviderFullAnnotationNames?.DefaultValueSql);
            }
            return (string)Annotations.GetAnnotation(RelationalFullAnnotationNames.Instance.DefaultValueSql, null);
        }

        protected virtual bool SetDefaultValueSql([CanBeNull] string value)
        {
            if (!CanSetDefaultValueSql(value))
            {
                return false;
            }

            if (!ShouldThrowOnConflict
                && DefaultValueSql != value
                && value != null)
            {
                ClearAllServerGeneratedValues();
            }

            return Annotations.SetAnnotation(
                RelationalFullAnnotationNames.Instance.DefaultValueSql,
                ProviderFullAnnotationNames?.DefaultValueSql,
                Check.NullButNotEmpty(value, nameof(value)));
        }

        protected virtual bool CanSetDefaultValueSql([CanBeNull] string value)
        {
            if (GetDefaultValueSql(false) == value)
            {
                return true;
            }

            if (!Annotations.CanSetAnnotation(
                RelationalFullAnnotationNames.Instance.DefaultValueSql,
                ProviderFullAnnotationNames?.DefaultValueSql,
                Check.NullButNotEmpty(value, nameof(value))))
            {
                return false;
            }

            if (ShouldThrowOnConflict)
            {
                if (GetDefaultValue(false) != null)
                {
                    throw new InvalidOperationException(
                        RelationalStrings.ConflictingColumnServerGeneration(nameof(DefaultValueSql), Property.Name, nameof(DefaultValue)));
                }
                if (GetComputedColumnSql(false) != null)
                {
                    throw new InvalidOperationException(
                        RelationalStrings.ConflictingColumnServerGeneration(nameof(DefaultValueSql), Property.Name, nameof(ComputedColumnSql)));
                }
            }
            else if (value != null
                     && (!CanSetDefaultValue(null)
                         || !CanSetComputedColumnSql(null)))
            {
                return false;
            }

            return true;
        }

        public virtual string ComputedColumnSql
        {
            get { return GetComputedColumnSql(true); }
            [param: CanBeNull] set { SetComputedColumnSql(value); }
        }

        protected virtual string GetComputedColumnSql(bool fallback)
        {
            if (ProviderFullAnnotationNames != null)
            {
                if (fallback
                    && (GetDefaultValue(false) != null
                        || GetDefaultValueSql(false) != null))
                {
                    return null;
                }

                return (string)Annotations.GetAnnotation(
                    fallback ? RelationalFullAnnotationNames.Instance.ComputedColumnSql : null,
                    ProviderFullAnnotationNames?.ComputedColumnSql);
            }
            return (string)Annotations.GetAnnotation(RelationalFullAnnotationNames.Instance.ComputedColumnSql, null);
        }

        protected virtual bool SetComputedColumnSql([CanBeNull] string value)
        {
            if (!CanSetComputedColumnSql(value))
            {
                return false;
            }

            if (!ShouldThrowOnConflict
                && ComputedColumnSql != value
                && value != null)
            {
                ClearAllServerGeneratedValues();
            }

            return Annotations.SetAnnotation(
                RelationalFullAnnotationNames.Instance.ComputedColumnSql,
                ProviderFullAnnotationNames?.ComputedColumnSql,
                Check.NullButNotEmpty(value, nameof(value)));
        }

        protected virtual bool CanSetComputedColumnSql([CanBeNull] string value)
        {
            if (GetComputedColumnSql(false) == value)
            {
                return true;
            }

            if (!Annotations.CanSetAnnotation(
                RelationalFullAnnotationNames.Instance.ComputedColumnSql,
                ProviderFullAnnotationNames?.ComputedColumnSql,
                Check.NullButNotEmpty(value, nameof(value))))
            {
                return false;
            }

            if (ShouldThrowOnConflict)
            {
                if (GetDefaultValue(false) != null)
                {
                    throw new InvalidOperationException(
                        RelationalStrings.ConflictingColumnServerGeneration(nameof(ComputedColumnSql), Property.Name, nameof(DefaultValue)));
                }
                if (GetDefaultValueSql(false) != null)
                {
                    throw new InvalidOperationException(
                        RelationalStrings.ConflictingColumnServerGeneration(nameof(ComputedColumnSql), Property.Name, nameof(DefaultValueSql)));
                }
            }
            else if (value != null
                     && (!CanSetDefaultValue(null)
                         || !CanSetDefaultValueSql(null)))
            {
                return false;
            }

            return true;
        }

        public virtual object DefaultValue
        {
            get { return GetDefaultValue(true); }
            [param: CanBeNull] set { SetDefaultValue(value); }
        }

        protected virtual object GetDefaultValue(bool fallback)
        {
            if (ProviderFullAnnotationNames != null)
            {
                if (fallback
                    && (GetDefaultValueSql(false) != null
                        || GetComputedColumnSql(false) != null))
                {
                    return null;
                }

                return Annotations.GetAnnotation(
                    fallback ? RelationalFullAnnotationNames.Instance.DefaultValue : null,
                    ProviderFullAnnotationNames?.DefaultValue);
            }
            return Annotations.GetAnnotation(RelationalFullAnnotationNames.Instance.DefaultValue, null);
        }

        protected virtual bool SetDefaultValue([CanBeNull] object value)
        {
            if (value != null)
            {
                var valueType = value.GetType();
                if (Property.ClrType.UnwrapNullableType() != valueType)
                {
                    try
                    {
                        value = Convert.ChangeType(value, Property.ClrType, CultureInfo.InvariantCulture);
                    }
                    catch (Exception)
                    {
                        throw new InvalidOperationException(
                            RelationalStrings.IncorrectDefaultValueType(
                                value, valueType, Property.Name, Property.ClrType, Property.DeclaringEntityType.DisplayName()));
                    }
                }

                if (valueType.GetTypeInfo().IsEnum)
                {
                    value = Convert.ChangeType(value, valueType.UnwrapEnumType(), CultureInfo.InvariantCulture);
                }
            }

            if (!CanSetDefaultValue(value))
            {
                return false;
            }

            if (!ShouldThrowOnConflict
                && DefaultValue != value
                && value != null)
            {
                ClearAllServerGeneratedValues();
            }

            return Annotations.SetAnnotation(
                RelationalFullAnnotationNames.Instance.DefaultValue,
                ProviderFullAnnotationNames?.DefaultValue,
                value);
        }

        protected virtual bool CanSetDefaultValue([CanBeNull] object value)
        {
            if (GetDefaultValue(false) == value)
            {
                return true;
            }

            if (!Annotations.CanSetAnnotation(
                RelationalFullAnnotationNames.Instance.DefaultValue,
                ProviderFullAnnotationNames?.DefaultValue,
                value))
            {
                return false;
            }

            if (ShouldThrowOnConflict)
            {
                if (GetDefaultValueSql(false) != null)
                {
                    throw new InvalidOperationException(
                        RelationalStrings.ConflictingColumnServerGeneration(nameof(DefaultValue), Property.Name, nameof(DefaultValueSql)));
                }
                if (GetComputedColumnSql(false) != null)
                {
                    throw new InvalidOperationException(
                        RelationalStrings.ConflictingColumnServerGeneration(nameof(DefaultValue), Property.Name, nameof(ComputedColumnSql)));
                }
            }
            else if (value != null
                     && (!CanSetDefaultValueSql(null)
                         || !CanSetComputedColumnSql(null)))
            {
                return false;
            }

            return true;
        }

        protected virtual void ClearAllServerGeneratedValues()
        {
            SetDefaultValue(null);
            SetDefaultValueSql(null);
            SetComputedColumnSql(null);
        }
    }
}
