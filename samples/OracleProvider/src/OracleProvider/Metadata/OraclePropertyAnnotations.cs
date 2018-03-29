// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Oracle.Internal;
using Microsoft.EntityFrameworkCore.Oracle.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Metadata
{
    public class OraclePropertyAnnotations : RelationalPropertyAnnotations, IOraclePropertyAnnotations
    {
        public OraclePropertyAnnotations([NotNull] IProperty property)
            : base(property)
        {
        }

        protected OraclePropertyAnnotations([NotNull] RelationalAnnotations annotations)
            : base(annotations)
        {
        }

        public virtual string HiLoSequenceName
        {
            get => (string)Annotations.Metadata[OracleAnnotationNames.HiLoSequenceName];
            [param: CanBeNull]
            set => SetHiLoSequenceName(value);
        }

        protected virtual bool SetHiLoSequenceName([CanBeNull] string value)
            => Annotations.SetAnnotation(
                OracleAnnotationNames.HiLoSequenceName,
                Check.NullButNotEmpty(value, nameof(value)));

        public virtual ISequence FindHiLoSequence()
        {
            var modelExtensions = Property.DeclaringEntityType.Model.Oracle();

            if (ValueGenerationStrategy != OracleValueGenerationStrategy.SequenceHiLo)
            {
                return null;
            }

            var sequenceName = HiLoSequenceName
                               ?? modelExtensions.HiLoSequenceName
                               ?? OracleModelAnnotations.DefaultHiLoSequenceName;

            return modelExtensions.FindSequence(sequenceName);
        }

        public virtual OracleValueGenerationStrategy? ValueGenerationStrategy
        {
            get => GetOracleValueGenerationStrategy(fallbackToModel: true);
            [param: CanBeNull]
            set => SetValueGenerationStrategy(value);
        }

        public virtual OracleValueGenerationStrategy? GetOracleValueGenerationStrategy(bool fallbackToModel)
        {
            var value = (OracleValueGenerationStrategy?)Annotations.Metadata[OracleAnnotationNames.ValueGenerationStrategy];

            if (value != null)
            {
                return value;
            }

            var relationalProperty = Property.Relational();
            if (!fallbackToModel
                || Property.ValueGenerated != ValueGenerated.OnAdd
                || relationalProperty.DefaultValue != null
                || relationalProperty.DefaultValueSql != null
                || relationalProperty.ComputedColumnSql != null)
            {
                return null;
            }

            var modelStrategy = Property.DeclaringEntityType.Model.Oracle().ValueGenerationStrategy;

            if (modelStrategy == OracleValueGenerationStrategy.SequenceHiLo
                && IsCompatibleSequenceHiLo(Property))
            {
                return OracleValueGenerationStrategy.SequenceHiLo;
            }

            if (modelStrategy == OracleValueGenerationStrategy.IdentityColumn
                && IsCompatibleIdentityColumn(Property))
            {
                return OracleValueGenerationStrategy.IdentityColumn;
            }

            return null;
        }

        protected virtual bool SetValueGenerationStrategy(OracleValueGenerationStrategy? value)
        {
            if (value != null)
            {
                var propertyType = Property.ClrType;

                if (value == OracleValueGenerationStrategy.IdentityColumn
                    && !IsCompatibleIdentityColumn(Property))
                {
                    if (ShouldThrowOnInvalidConfiguration)
                    {
                        throw new ArgumentException(
                            OracleStrings.IdentityBadType(
                                Property.Name, Property.DeclaringEntityType.DisplayName(), propertyType.ShortDisplayName()));
                    }

                    return false;
                }

                if (value == OracleValueGenerationStrategy.SequenceHiLo
                    && !IsCompatibleSequenceHiLo(Property))
                {
                    if (ShouldThrowOnInvalidConfiguration)
                    {
                        throw new ArgumentException(
                            OracleStrings.SequenceBadType(
                                Property.Name, Property.DeclaringEntityType.DisplayName(), propertyType.ShortDisplayName()));
                    }

                    return false;
                }
            }

            if (!CanSetValueGenerationStrategy(value))
            {
                return false;
            }

            if (!ShouldThrowOnConflict
                && ValueGenerationStrategy != value
                && value != null)
            {
                ClearAllServerGeneratedValues();
            }

            return Annotations.SetAnnotation(OracleAnnotationNames.ValueGenerationStrategy, value);
        }

        protected virtual bool CanSetValueGenerationStrategy(OracleValueGenerationStrategy? value)
        {
            if (GetOracleValueGenerationStrategy(fallbackToModel: false) == value)
            {
                return true;
            }

            if (!Annotations.CanSetAnnotation(OracleAnnotationNames.ValueGenerationStrategy, value))
            {
                return false;
            }

            if (ShouldThrowOnConflict)
            {
                if (GetDefaultValue(false) != null)
                {
                    throw new InvalidOperationException(
                        RelationalStrings.ConflictingColumnServerGeneration(nameof(ValueGenerationStrategy), Property.Name, nameof(DefaultValue)));
                }
                if (GetDefaultValueSql(false) != null)
                {
                    throw new InvalidOperationException(
                        RelationalStrings.ConflictingColumnServerGeneration(nameof(ValueGenerationStrategy), Property.Name, nameof(DefaultValueSql)));
                }
                if (GetComputedColumnSql(false) != null)
                {
                    throw new InvalidOperationException(
                        RelationalStrings.ConflictingColumnServerGeneration(nameof(ValueGenerationStrategy), Property.Name, nameof(ComputedColumnSql)));
                }
            }
            else if (value != null
                     && (!CanSetDefaultValue(null)
                         || !CanSetDefaultValueSql(null)
                         || !CanSetComputedColumnSql(null)))
            {
                return false;
            }

            return true;
        }

        protected override object GetDefaultValue(bool fallback)
        {
            if (fallback
                && ValueGenerationStrategy != null)
            {
                return null;
            }

            return base.GetDefaultValue(fallback);
        }

        protected override bool CanSetDefaultValue(object value)
        {
            if (ShouldThrowOnConflict)
            {
                if (ValueGenerationStrategy != null)
                {
                    throw new InvalidOperationException(
                        RelationalStrings.ConflictingColumnServerGeneration(nameof(DefaultValue), Property.Name, nameof(ValueGenerationStrategy)));
                }
            }
            else if (value != null
                     && !CanSetValueGenerationStrategy(null))
            {
                return false;
            }

            return base.CanSetDefaultValue(value);
        }

        protected override string GetDefaultValueSql(bool fallback)
        {
            if (fallback
                && ValueGenerationStrategy != null)
            {
                return null;
            }

            return base.GetDefaultValueSql(fallback);
        }

        protected override bool CanSetDefaultValueSql(string value)
        {
            if (ShouldThrowOnConflict)
            {
                if (ValueGenerationStrategy != null)
                {
                    throw new InvalidOperationException(
                        RelationalStrings.ConflictingColumnServerGeneration(nameof(DefaultValueSql), Property.Name, nameof(ValueGenerationStrategy)));
                }
            }
            else if (value != null
                     && !CanSetValueGenerationStrategy(null))
            {
                return false;
            }

            return base.CanSetDefaultValueSql(value);
        }

        protected override string GetComputedColumnSql(bool fallback)
        {
            if (fallback
                && ValueGenerationStrategy != null)
            {
                return null;
            }

            return base.GetComputedColumnSql(fallback);
        }

        protected override bool CanSetComputedColumnSql(string value)
        {
            if (ShouldThrowOnConflict)
            {
                if (ValueGenerationStrategy != null)
                {
                    throw new InvalidOperationException(
                        RelationalStrings.ConflictingColumnServerGeneration(nameof(ComputedColumnSql), Property.Name, nameof(ValueGenerationStrategy)));
                }
            }
            else if (value != null
                     && !CanSetValueGenerationStrategy(null))
            {
                return false;
            }

            return base.CanSetComputedColumnSql(value);
        }

        protected override void ClearAllServerGeneratedValues()
        {
            SetValueGenerationStrategy(null);

            base.ClearAllServerGeneratedValues();
        }

        private static bool IsCompatibleIdentityColumn(IProperty property)
        {
            var type = property.ClrType;

            return (type.IsInteger() || type == typeof(decimal)) && !HasConverter(property);
        }

        private static bool IsCompatibleSequenceHiLo(IProperty property)
            => property.ClrType.IsInteger() && !HasConverter(property);

        private static bool HasConverter(IProperty property)
            => (property.FindMapping()?.Converter
                ?? property.GetValueConverter()) != null;
    }
}
