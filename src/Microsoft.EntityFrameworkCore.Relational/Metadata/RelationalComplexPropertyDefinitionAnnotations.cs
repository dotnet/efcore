// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Metadata
{
    public class RelationalComplexPropertyDefinitionAnnotations : IRelationalComplexPropertyDefinitionAnnotations
    {
        protected readonly RelationalFullAnnotationNames ProviderFullAnnotationNames;

        public RelationalComplexPropertyDefinitionAnnotations([NotNull] IComplexPropertyDefinition propertyDefinition,
            [CanBeNull] RelationalFullAnnotationNames providerFullAnnotationNames)
            : this(new RelationalAnnotations(propertyDefinition), providerFullAnnotationNames)
        {
        }

        protected RelationalComplexPropertyDefinitionAnnotations([NotNull] RelationalAnnotations annotations,
            [CanBeNull] RelationalFullAnnotationNames providerFullAnnotationNames)
        {
            Annotations = annotations;
            ProviderFullAnnotationNames = providerFullAnnotationNames;
        }

        protected virtual RelationalAnnotations Annotations { get; }

        protected virtual IComplexPropertyDefinition PropertyDefinition => (IComplexPropertyDefinition)Annotations.Metadata;

        public virtual string ColumnNameDefault
        {
            get
            {
                return (string)Annotations.GetAnnotation(
                    RelationalFullAnnotationNames.Instance.ColumnName,
                    ProviderFullAnnotationNames?.ColumnName);
            }
            [param: CanBeNull] set { SetColumnNameDefault(value); }
        }

        protected virtual bool SetColumnNameDefault([CanBeNull] string value)
            => Annotations.SetAnnotation(
                RelationalFullAnnotationNames.Instance.ColumnName,
                ProviderFullAnnotationNames?.ColumnName,
                Check.NullButNotEmpty(value, nameof(value)));

        public virtual string ColumnTypeDefault
        {
            get
            {
                return (string)Annotations.GetAnnotation(
                    RelationalFullAnnotationNames.Instance.ColumnType,
                    ProviderFullAnnotationNames?.ColumnType);
            }
            [param: CanBeNull] set { SetColumnTypeDefault(value); }
        }

        protected virtual bool SetColumnTypeDefault([CanBeNull] string value)
            => Annotations.SetAnnotation(
                RelationalFullAnnotationNames.Instance.ColumnType,
                ProviderFullAnnotationNames?.ColumnType,
                Check.NullButNotEmpty(value, nameof(value)));

        public virtual string DefaultValueSqlDefault
        {
            get
            {
                return (string)Annotations.GetAnnotation(
                    RelationalFullAnnotationNames.Instance.DefaultValueSql,
                    ProviderFullAnnotationNames?.DefaultValueSql);
            }
            [param: CanBeNull] set { SetDefaultValueSqlDefault(value); }
        }

        protected virtual bool SetDefaultValueSqlDefault([CanBeNull] string value)
            => Annotations.SetAnnotation(
                RelationalFullAnnotationNames.Instance.DefaultValueSql,
                ProviderFullAnnotationNames?.DefaultValueSql,
                Check.NullButNotEmpty(value, nameof(value)));

        public virtual string ComputedColumnSqlDefault
        {
            get
            {
                return (string)Annotations.GetAnnotation(
                    RelationalFullAnnotationNames.Instance.ComputedColumnSql,
                    ProviderFullAnnotationNames?.ComputedColumnSql);
            }
            [param: CanBeNull] set { SetComputedColumnSqlDefault(value); }
        }

        protected virtual bool SetComputedColumnSqlDefault([CanBeNull] string value)
            => Annotations.SetAnnotation(
                RelationalFullAnnotationNames.Instance.ComputedColumnSql,
                ProviderFullAnnotationNames?.ComputedColumnSql,
                Check.NullButNotEmpty(value, nameof(value)));

        public virtual object DefaultValueDefault
        {
            get
            {
                return Annotations.GetAnnotation(
                    RelationalFullAnnotationNames.Instance.DefaultValue,
                    ProviderFullAnnotationNames?.DefaultValue);
            }
            [param: CanBeNull] set { SetDefaultValueDefault(value); }
        }

        protected virtual bool SetDefaultValueDefault([CanBeNull] object value)
            => Annotations.SetAnnotation(
                RelationalFullAnnotationNames.Instance.DefaultValue,
                ProviderFullAnnotationNames?.DefaultValue,
                value);
    }
}
