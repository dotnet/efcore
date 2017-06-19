// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Metadata
{
    public class RelationalPropertyAnnotations : IRelationalPropertyAnnotations
    {
        public RelationalPropertyAnnotations([NotNull] IProperty property)
            : this(new RelationalAnnotations(property))
        {
        }

        protected RelationalPropertyAnnotations([NotNull] RelationalAnnotations annotations)
            => Annotations = annotations;

        protected virtual RelationalAnnotations Annotations { get; }

        protected virtual IProperty Property => (IProperty)Annotations.Metadata;

        protected virtual bool ShouldThrowOnConflict => true;

        protected virtual bool ShouldThrowOnInvalidConfiguration => true;

        protected virtual RelationalEntityTypeAnnotations GetAnnotations([NotNull] IEntityType entityType)
            => new RelationalEntityTypeAnnotations(entityType);

        protected virtual RelationalPropertyAnnotations GetAnnotations([NotNull] IProperty property)
            => new RelationalPropertyAnnotations(property);

        public virtual string ColumnName
        {
            get => (string)Annotations.Metadata[RelationalAnnotationNames.ColumnName]
                   ?? GetDefaultColumnName();

            [param: CanBeNull] set => SetColumnName(value);
        }

        private string GetDefaultColumnName()
        {
            var pk = Property.GetContainingPrimaryKey();
            if (pk != null)
            {
                var entityType = Property.DeclaringEntityType;
                var ownership = entityType.GetForeignKeys().SingleOrDefault(fk => fk.IsOwnership);
                if (ownership != null)
                {
                    var ownerType = ownership.PrincipalEntityType;
                    var entityTypeAnnotations = GetAnnotations(entityType);
                    var ownerTypeAnnotations = GetAnnotations(ownerType);
                    if (entityTypeAnnotations.TableName == ownerTypeAnnotations.TableName
                        && entityTypeAnnotations.Schema == ownerTypeAnnotations.Schema)
                    {
                        var index = -1;
                        for (var i = 0; i < pk.Properties.Count; i++)
                        {
                            if (pk.Properties[i] == Property)
                            {
                                index = i;
                                break;
                            }
                        }

                        return GetAnnotations(ownerType.FindPrimaryKey().Properties[index]).ColumnName;
                    }
                }
            }
            else
            {
                var entityType = Property.DeclaringEntityType;
                StringBuilder builder = null;
                do
                {
                    var ownership = entityType.GetForeignKeys().SingleOrDefault(fk => fk.IsOwnership);
                    if (ownership == null)
                    {
                        entityType = null;
                    }
                    else
                    {
                        var ownerType = ownership.PrincipalEntityType;
                        var entityTypeAnnotations = GetAnnotations(entityType);
                        var ownerTypeAnnotations = GetAnnotations(ownerType);
                        if (entityTypeAnnotations.TableName == ownerTypeAnnotations.TableName
                            && entityTypeAnnotations.Schema == ownerTypeAnnotations.Schema)
                        {
                            if (builder == null)
                            {
                                builder = new StringBuilder();
                            }
                            builder.Insert(0, "_");
                            builder.Insert(0, ownership.PrincipalToDependent.Name);
                            entityType = ownerType;
                        }
                        else
                        {
                            entityType = null;
                        }
                    }
                }
                while (entityType != null);

                if (builder != null)
                {
                    builder.Append(Property.Name);
                    return builder.ToString();
                }
            }

            return Property.Name;
        }

        protected virtual bool SetColumnName([CanBeNull] string value)
            => Annotations.SetAnnotation(
                RelationalAnnotationNames.ColumnName,
                Check.NullButNotEmpty(value, nameof(value)));

        public virtual string ColumnType
        {
            get => (string)Annotations.Metadata[RelationalAnnotationNames.ColumnType]
                ?? ((RelationalTypeMapping)Annotations.Metadata[RelationalAnnotationNames.TypeMapping])?.StoreType;
            [param: CanBeNull] set => SetColumnType(value);
        }

        protected virtual bool SetColumnType([CanBeNull] string value)
            => Annotations.SetAnnotation(
                RelationalAnnotationNames.ColumnType,
                Check.NullButNotEmpty(value, nameof(value)));

        public virtual string DefaultValueSql
        {
            get => GetDefaultValueSql(true);
            [param: CanBeNull] set => SetDefaultValueSql(value);
        }

        protected virtual string GetDefaultValueSql(bool fallback)
            => fallback
               && (GetDefaultValue(false) != null
                   || GetComputedColumnSql(false) != null)
                ? null
                : (string)Annotations.Metadata[RelationalAnnotationNames.DefaultValueSql];

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
                RelationalAnnotationNames.DefaultValueSql,
                Check.NullButNotEmpty(value, nameof(value)));
        }

        protected virtual bool CanSetDefaultValueSql([CanBeNull] string value)
        {
            if (GetDefaultValueSql(false) == value)
            {
                return true;
            }

            if (!Annotations.CanSetAnnotation(
                RelationalAnnotationNames.DefaultValueSql,
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
            get => GetComputedColumnSql(true);
            [param: CanBeNull] set => SetComputedColumnSql(value);
        }

        protected virtual string GetComputedColumnSql(bool fallback)
            => fallback
               && (GetDefaultValue(false) != null
                   || GetDefaultValueSql(false) != null)
                ? null
                : (string)Annotations.Metadata[RelationalAnnotationNames.ComputedColumnSql];

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
                RelationalAnnotationNames.ComputedColumnSql,
                Check.NullButNotEmpty(value, nameof(value)));
        }

        protected virtual bool CanSetComputedColumnSql([CanBeNull] string value)
        {
            if (GetComputedColumnSql(false) == value)
            {
                return true;
            }

            if (!Annotations.CanSetAnnotation(
                RelationalAnnotationNames.ComputedColumnSql,
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
            get => GetDefaultValue(true);
            [param: CanBeNull] set => SetDefaultValue(value);
        }

        protected virtual object GetDefaultValue(bool fallback)
            => fallback
               && (GetDefaultValueSql(false) != null
                   || GetComputedColumnSql(false) != null)
                ? null
                : Annotations.Metadata[RelationalAnnotationNames.DefaultValue];

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
                RelationalAnnotationNames.DefaultValue,
                value);
        }

        protected virtual bool CanSetDefaultValue([CanBeNull] object value)
        {
            if (GetDefaultValue(false) == value)
            {
                return true;
            }

            if (!Annotations.CanSetAnnotation(
                RelationalAnnotationNames.DefaultValue,
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
