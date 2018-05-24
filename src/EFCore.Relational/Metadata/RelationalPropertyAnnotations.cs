// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Globalization;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Metadata
{
    /// <summary>
    ///     Properties for relational-specific annotations accessed through
    ///     <see cref="RelationalMetadataExtensions.Relational(IMutableProperty)" />.
    /// </summary>
    public class RelationalPropertyAnnotations : IRelationalPropertyAnnotations
    {
        /// <summary>
        ///     Constructs an instance for annotations of the given <see cref="IProperty" />.
        /// </summary>
        /// <param name="property"> The <see cref="IProperty" /> to use. </param>
        public RelationalPropertyAnnotations([NotNull] IProperty property)
            : this(new RelationalAnnotations(property))
        {
        }

        /// <summary>
        ///     Constructs an instance for annotations of the <see cref="IProperty" />
        ///     represented by the given annotation helper.
        /// </summary>
        /// <param name="annotations">
        ///     The <see cref="RelationalAnnotations" /> helper representing the <see cref="IProperty" /> to annotate.
        /// </param>
        protected RelationalPropertyAnnotations([NotNull] RelationalAnnotations annotations)
            => Annotations = annotations;

        /// <summary>
        ///     The <see cref="RelationalAnnotations" /> helper representing the <see cref="IProperty" /> to annotate.
        /// </summary>
        protected virtual RelationalAnnotations Annotations { get; }

        /// <summary>
        ///     The <see cref="IProperty" /> to annotate.
        /// </summary>
        protected virtual IProperty Property => (IProperty)Annotations.Metadata;

        /// <summary>
        ///     Indicates whether or not an exception should be thrown if conflicting configuration is set.
        ///     This is typically overridden when building using a fluent API to implement last call wins semantics.
        /// </summary>
        protected virtual bool ShouldThrowOnConflict => true;

        /// <summary>
        ///     Indicates whether or not an exception should be thrown if invalid configuration is set.
        /// </summary>
        protected virtual bool ShouldThrowOnInvalidConfiguration => true;

        /// <summary>
        ///     Gets a <see cref="RelationalEntityTypeAnnotations" /> instance for the given <see cref="IEntityType" />
        ///     maintaining the <see cref="RelationalAnnotations" /> semantics being used by this instance to
        ///     control setting annotations by convention.
        /// </summary>
        /// <param name="entityType"> The <see cref="IEntityType" /> to annotate. </param>
        /// <returns> A new <see cref="RelationalEntityTypeAnnotations" /> instance. </returns>
        protected virtual RelationalEntityTypeAnnotations GetAnnotations([NotNull] IEntityType entityType)
            => new RelationalEntityTypeAnnotations(entityType);

        /// <summary>
        ///     Gets a <see cref="RelationalPropertyAnnotations" /> instance for the given <see cref="IProperty" />
        ///     maintaining the <see cref="RelationalAnnotations" /> semantics being used by this instance to
        ///     control setting annotations by convention.
        /// </summary>
        /// <param name="property"> The <see cref="IProperty" /> to annotate. </param>
        /// <returns> A new <see cref="RelationalPropertyAnnotations" /> instance. </returns>
        protected virtual RelationalPropertyAnnotations GetAnnotations([NotNull] IProperty property)
            => new RelationalPropertyAnnotations(property);

        /// <summary>
        ///     The name of the column to which the property is mapped.
        /// </summary>
        public virtual string ColumnName
        {
            get => (string)Annotations.Metadata[RelationalAnnotationNames.ColumnName]
                   ?? ConstraintNamer.GetDefaultName(Property);

            [param: CanBeNull] set => SetColumnName(value);
        }

        /// <summary>
        ///     Attempts to set the <see cref="ColumnName" /> using the semantics of
        ///     the <see cref="RelationalAnnotations" /> in use.
        /// </summary>
        /// <param name="value"> The value to set. </param>
        /// <returns> <c>True</c> if the annotation was set; <c>false</c> otherwise. </returns>
        protected virtual bool SetColumnName([CanBeNull] string value)
            => Annotations.SetAnnotation(
                RelationalAnnotationNames.ColumnName,
                Check.NullButNotEmpty(value, nameof(value)));

        /// <summary>
        ///     The database type of the column to which the property is mapped.
        /// </summary>
        public virtual string ColumnType
        {
            get
            {
                var columnType = (string)Annotations.Metadata[RelationalAnnotationNames.ColumnType];
                if (columnType != null)
                {
                    return columnType;
                }

                var sharedTablePrincipalPrimaryKeyProperty = Property.FindSharedTableRootPrimaryKeyProperty();
                return sharedTablePrincipalPrimaryKeyProperty != null
                    ? sharedTablePrincipalPrimaryKeyProperty.Relational().ColumnType
                    : Property.FindRelationalMapping()?.StoreType;
            }

            [param: CanBeNull] set => SetColumnType(value);
        }

        /// <summary>
        ///     Attempts to set the <see cref="ColumnType" /> using the semantics of
        ///     the <see cref="RelationalAnnotations" /> in use.
        /// </summary>
        /// <param name="value"> The value to set. </param>
        /// <returns> <c>True</c> if the annotation was set; <c>false</c> otherwise. </returns>
        protected virtual bool SetColumnType([CanBeNull] string value)
            => Annotations.SetAnnotation(
                RelationalAnnotationNames.ColumnType,
                Check.NullButNotEmpty(value, nameof(value)));

        /// <summary>
        ///     The default constraint SQL expression that should be used when creating a column for this property.
        /// </summary>
        public virtual string DefaultValueSql
        {
            get => GetDefaultValueSql(true);
            [param: CanBeNull] set => SetDefaultValueSql(value);
        }

        /// <summary>
        ///     Gets the default constraint SQL expression that should be used when creating a column for this property.
        /// </summary>
        /// <param name="fallback">
        ///     If <c>true</c>, then a non-null value will only be returned if neither of <see cref="DefaultValue" />
        ///     or <see cref="ComputedColumnSql" /> are set for this property.
        /// </param>
        /// <returns> The default constraint SQL expression that should be used when creating a column for this property. </returns>
        protected virtual string GetDefaultValueSql(bool fallback)
        {
            if (fallback
                && (GetDefaultValue(false) != null
                    || GetComputedColumnSql(false) != null))
            {
                return null;
            }

            var sharedTablePrincipalPrimaryKeyProperty = Property.FindSharedTableRootPrimaryKeyProperty();
            return sharedTablePrincipalPrimaryKeyProperty != null
                ? ((RelationalPropertyAnnotations)sharedTablePrincipalPrimaryKeyProperty.Relational()).GetDefaultValueSql(fallback)
                : (string)Annotations.Metadata[RelationalAnnotationNames.DefaultValueSql];
        }

        /// <summary>
        ///     Attempts to set the <see cref="DefaultValueSql" /> using the semantics of
        ///     the <see cref="RelationalAnnotations" /> in use.
        /// </summary>
        /// <param name="value"> The value to set. </param>
        /// <returns> <c>True</c> if the annotation was set; <c>false</c> otherwise. </returns>
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

        /// <summary>
        ///     <para>
        ///         Determines whether or not <see cref="DefaultValueSql" /> can be set without conflict.
        ///     </para>
        ///     <para>
        ///         This method may throw if <see cref="ShouldThrowOnConflict" /> returns <c>true</c>.
        ///     </para>
        /// </summary>
        /// <param name="value"> The value to set. </param>
        /// <returns> <c>True</c> if the value can be set; <c>false</c> otherwise. </returns>
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

        /// <summary>
        ///     The computed constraint SQL expression that should be used when creating a column for this property.
        /// </summary>
        public virtual string ComputedColumnSql
        {
            get => GetComputedColumnSql(true);
            [param: CanBeNull] set => SetComputedColumnSql(value);
        }

        /// <summary>
        ///     Gets the computed constraint SQL expression that should be used when creating a column for this property.
        /// </summary>
        /// <param name="fallback">
        ///     If <c>true</c>, then a non-null value will only be returned if neither of <see cref="DefaultValue" />
        ///     or <see cref="DefaultValueSql" /> are set for this property.
        /// </param>
        /// <returns> The computed constraint SQL expression that should be used when creating a column for this property. </returns>
        protected virtual string GetComputedColumnSql(bool fallback)
        {
            if (fallback
                && (GetDefaultValue(false) != null
                    || GetDefaultValueSql(false) != null))
            {
                return null;
            }

            var sharedTablePrincipalPrimaryKeyProperty = Property.FindSharedTableRootPrimaryKeyProperty();
            return sharedTablePrincipalPrimaryKeyProperty != null
                ? ((RelationalPropertyAnnotations)sharedTablePrincipalPrimaryKeyProperty.Relational()).GetComputedColumnSql(fallback)
                : (string)Annotations.Metadata[RelationalAnnotationNames.ComputedColumnSql];
        }

        /// <summary>
        ///     Attempts to set the <see cref="ComputedColumnSql" /> using the semantics of
        ///     the <see cref="RelationalAnnotations" /> in use.
        /// </summary>
        /// <param name="value"> The value to set. </param>
        /// <returns> <c>True</c> if the annotation was set; <c>false</c> otherwise. </returns>
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

        /// <summary>
        ///     <para>
        ///         Determines whether or not <see cref="ComputedColumnSql" /> can be set without conflict.
        ///     </para>
        ///     <para>
        ///         This method may throw if <see cref="ShouldThrowOnConflict" /> returns <c>true</c>.
        ///     </para>
        /// </summary>
        /// <param name="value"> The value to set. </param>
        /// <returns> <c>True</c> if the value can be set; <c>false</c> otherwise. </returns>
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

        /// <summary>
        ///     The default value to use in the definition of the column when creating a column for this property.
        /// </summary>
        public virtual object DefaultValue
        {
            get => GetDefaultValue(true);
            [param: CanBeNull] set => SetDefaultValue(value);
        }

        /// <summary>
        ///     Gets the default value to use in the definition of the column when creating a column for this property.
        /// </summary>
        /// <param name="fallback">
        ///     If <c>true</c>, then a non-null value will only be returned if neither of <see cref="ComputedColumnSql" />
        ///     or <see cref="DefaultValueSql" /> are set for this property.
        /// </param>
        /// <returns> The default value to use in the definition of the column when creating a column for this property. </returns>
        protected virtual object GetDefaultValue(bool fallback)
        {
            if (fallback
                && (GetDefaultValueSql(false) != null
                    || GetComputedColumnSql(false) != null))
            {
                return null;
            }

            var sharedTablePrincipalPrimaryKeyProperty = Property.FindSharedTableRootPrimaryKeyProperty();
            return sharedTablePrincipalPrimaryKeyProperty != null
                ? ((RelationalPropertyAnnotations)sharedTablePrincipalPrimaryKeyProperty.Relational()).GetDefaultValue(fallback)
                : Annotations.Metadata[RelationalAnnotationNames.DefaultValue];
        }

        /// <summary>
        ///     Attempts to set the <see cref="DefaultValue" /> using the semantics of
        ///     the <see cref="RelationalAnnotations" /> in use.
        /// </summary>
        /// <param name="value"> The value to set. </param>
        /// <returns> <c>True</c> if the annotation was set; <c>false</c> otherwise. </returns>
        protected virtual bool SetDefaultValue([CanBeNull] object value)
        {
            if (value != null
                && value != DBNull.Value)
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

        /// <summary>
        ///     <para>
        ///         Determines whether or not <see cref="DefaultValue" /> can be set without conflict.
        ///     </para>
        ///     <para>
        ///         This method may throw if <see cref="ShouldThrowOnConflict" /> returns <c>true</c>.
        ///     </para>
        /// </summary>
        /// <param name="value"> The value to set. </param>
        /// <returns> <c>True</c> if the value can be set; <c>false</c> otherwise. </returns>
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

        /// <summary>
        ///     Clears any values set for <see cref="DefaultValue" />, <see cref="DefaultValueSql" />, and
        ///     <see cref="ComputedColumnSql" />.
        /// </summary>
        protected virtual void ClearAllServerGeneratedValues()
        {
            SetDefaultValue(null);
            SetDefaultValueSql(null);
            SetComputedColumnSql(null);
        }

        /// <summary>
        ///     A flag indicating if the property as capable of storing only fixed-length data, such as strings.
        /// </summary>
        public virtual bool IsFixedLength
        {
            get
            {
                var fixedLength = Annotations.Metadata[RelationalAnnotationNames.IsFixedLength];
                return fixedLength != null && (bool)fixedLength;
            }

            set => SetFixedLength(value);
        }

        /// <summary>
        ///     Configures the property as capable of storing only fixed-length data, such as strings.
        /// </summary>
        /// <param name="fixedLength"> A value indicating whether the property is constrained to fixed length values. </param>
        /// <returns> <c>True</c> if the value can be set; <c>false</c> otherwise. </returns>
        protected virtual bool SetFixedLength(bool fixedLength)
            => Annotations.SetAnnotation(
                RelationalAnnotationNames.IsFixedLength,
                fixedLength);
    }
}
