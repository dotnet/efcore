// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Internal;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Metadata
{
    [DebuggerDisplay("{EntityType.Name,nq}.{Name,nq} ({ClrType.Name,nq})")]
    public class Property : PropertyBase, IProperty
    {
        private PropertyFlags _flags;
        // TODO: Remove this once the model is readonly Issue #868
        private PropertyFlags _setFlags;
        private int _shadowIndex;
        private int _index;

        public Property([NotNull] string name, [NotNull] Type clrType, [NotNull] EntityType entityType, bool shadowProperty = false)
            : base(name)
        {
            Check.NotNull(clrType, nameof(clrType));
            Check.NotNull(entityType, nameof(entityType));

            ClrType = clrType;
            EntityType = entityType;
            _shadowIndex = shadowProperty ? 0 : -1;
        }

        public virtual Type ClrType { get; }

        public override EntityType EntityType { get; }

        public virtual bool? IsNullable
        {
            get { return GetFlag(PropertyFlags.IsNullable); }
            set
            {
                if (value.HasValue
                    && value.Value
                    && !ClrType.IsNullableType())
                {
                    throw new InvalidOperationException(Strings.CannotBeNullable(Name, EntityType.DisplayName(), ClrType.Name));
                }

                SetFlag(value, PropertyFlags.IsNullable);
            }
        }

        protected virtual bool DefaultIsNullable => ClrType.IsNullableType();

        public virtual bool? UseStoreDefault
        {
            get { return GetFlag(PropertyFlags.UseStoreDefault); }
            set { SetFlag(value, PropertyFlags.UseStoreDefault); }
        }

        protected virtual bool DefaultUseStoreDefault => false;

        public virtual bool? IsReadOnly
        {
            get { return GetFlag(PropertyFlags.IsReadOnly); }
            set
            {
                if (value.HasValue
                    && !value.Value
                    && this.IsKey())
                {
                    throw new NotSupportedException(Strings.KeyPropertyMustBeReadOnly(Name, EntityType.Name));
                }
                SetFlag(value, PropertyFlags.IsReadOnly);
            }
        }

        protected virtual bool DefaultIsReadOnly => this.IsKey();

        public virtual bool? IsStoreComputed
        {
            get { return GetFlag(PropertyFlags.IsStoreComputed); }
            set { SetFlag(value, PropertyFlags.IsStoreComputed); }
        }

        protected virtual bool DefaultIsStoreComputed => false;

        public virtual bool? GenerateValueOnAdd
        {
            get { return GetFlag(PropertyFlags.GenerateValueOnAdd); }
            set { SetFlag(value, PropertyFlags.GenerateValueOnAdd); }
        }

        protected virtual bool DefaultGenerateValueOnAdd => false;

        public virtual bool IsShadowProperty
        {
            get { return _shadowIndex >= 0; }
            set
            {
                if (IsShadowProperty != value)
                {
                    _shadowIndex = value ? 0 : -1;

                    EntityType.OnPropertyMetadataChanged(this, this);
                }
            }
        }

        public virtual bool? IsConcurrencyToken
        {
            get { return GetFlag(PropertyFlags.IsConcurrencyToken); }
            set
            {
                if (IsConcurrencyToken != value)
                {
                    SetFlag(value, PropertyFlags.IsConcurrencyToken);

                    EntityType.OnPropertyMetadataChanged(this, this);
                }
            }
        }

        protected virtual bool DefaultIsConcurrencyToken => false;

        public virtual int Index
        {
            get { return _index; }
            set
            {
                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(value));
                }
                _index = value;
            }
        }

        public virtual int ShadowIndex
        {
            get { return _shadowIndex; }
            set
            {
                if (value < 0
                    || !IsShadowProperty)
                {
                    throw new ArgumentOutOfRangeException(nameof(value));
                }

                _shadowIndex = value;
            }
        }

        public virtual object SentinelValue { get; [param: CanBeNull] set; }

        private bool? GetFlag(PropertyFlags flag) => (_setFlags & flag) != 0 ? (_flags & flag) != 0 : (bool?)null;

        private void SetFlag(bool? value, PropertyFlags flag)
        {
            _setFlags = value.HasValue ? (_setFlags | flag) : (_setFlags & ~flag);
            _flags = value.HasValue && value.Value ? (_flags | flag) : (_flags & ~flag);
        }

        internal static string Format(IEnumerable<IProperty> properties) => "{" + string.Join(", ", properties.Select(p => "'" + p.Name + "'")) + "}";

        public static bool AreCompatible([NotNull] IReadOnlyList<Property> principalProperties,
            [NotNull] IReadOnlyList<Property> dependentProperties)
        {
            return ArePropertyCountsEqual(principalProperties, dependentProperties)
                   && ArePropertyTypesCompatible(principalProperties, dependentProperties);
        }

        public static void EnsureCompatible([NotNull] IReadOnlyList<Property> principalProperties,
            [NotNull] IReadOnlyList<Property> dependentProperties)
        {
            if (!ArePropertyCountsEqual(principalProperties, dependentProperties))
            {
                throw new InvalidOperationException(
                    Strings.ForeignKeyCountMismatch(
                        Format(dependentProperties),
                        dependentProperties[0].EntityType.Name,
                        Format(principalProperties),
                        principalProperties[0].EntityType.Name));
            }

            if (!ArePropertyTypesCompatible(principalProperties, dependentProperties))
            {
                throw new InvalidOperationException(
                    Strings.ForeignKeyTypeMismatch(
                        Format(dependentProperties),
                        dependentProperties[0].EntityType.Name,
                        principalProperties[0].EntityType.Name));
            }
        }

        private static bool ArePropertyCountsEqual(IReadOnlyList<Property> principalProperties, IReadOnlyList<Property> dependentProperties)
        {
            return principalProperties.Count == dependentProperties.Count;
        }

        private static bool ArePropertyTypesCompatible(IReadOnlyList<Property> principalProperties, IReadOnlyList<Property> dependentProperties)
        {
            return principalProperties.Select(p => p.ClrType.UnwrapNullableType()).SequenceEqual(dependentProperties.Select(p => p.ClrType.UnwrapNullableType()));
        }

        bool IProperty.IsNullable => IsNullable ?? DefaultIsNullable;

        bool IProperty.UseStoreDefault => UseStoreDefault ?? DefaultUseStoreDefault;

        bool IProperty.IsReadOnly => IsReadOnly ?? DefaultIsReadOnly;

        bool IProperty.IsStoreComputed => IsStoreComputed ?? DefaultIsStoreComputed;

        bool IProperty.IsValueGeneratedOnAdd => GenerateValueOnAdd ?? DefaultGenerateValueOnAdd;

        bool IProperty.IsConcurrencyToken => IsConcurrencyToken ?? DefaultIsConcurrencyToken;

        object IProperty.SentinelValue => SentinelValue == null && !ClrType.IsNullableType() ? ClrType.GetDefaultValue() : SentinelValue;

        [Flags]
        private enum PropertyFlags : ushort
        {
            IsConcurrencyToken = 1,
            IsNullable = 2,
            IsReadOnly = 4,
            UseStoreDefault = 8,
            IsStoreComputed = 16,
            GenerateValueOnAdd = 32
        }
    }
}
