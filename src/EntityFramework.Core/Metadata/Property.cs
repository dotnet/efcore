// Copyright (c) .NET Foundation. All rights reserved.
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

        private int _index;

        public Property([NotNull] string name, [NotNull] Type clrType, [NotNull] EntityType entityType, bool shadowProperty = false)
            : base(name)
        {
            Check.NotNull(clrType, nameof(clrType));
            Check.NotNull(entityType, nameof(entityType));

            ClrType = clrType;
            EntityType = entityType;
            IsShadowProperty = shadowProperty;
        }

        public virtual Type ClrType { get; }

        public override EntityType EntityType { get; }

        public virtual bool? IsNullable
        {
            get { return GetFlag(PropertyFlags.IsNullable); }
            set
            {
                if (value.HasValue
                    && value.Value)
                {
                    if (!ClrType.IsNullableType())
                    {
                        throw new InvalidOperationException(Strings.CannotBeNullable(Name, EntityType.DisplayName(), ClrType.Name));
                    }
                    if (EntityType.FindPrimaryKey()?.Properties.Contains(this) == true)
                    {
                        throw new InvalidOperationException(Strings.CannotBeNullablePK(Name, EntityType.DisplayName()));
                    }
                }

                SetFlag(value, PropertyFlags.IsNullable);
            }
        }

        protected virtual bool DefaultIsNullable => (EntityType.FindPrimaryKey()?.Properties.Contains(this)) != true && ClrType.IsNullableType();

        public virtual StoreGeneratedPattern? StoreGeneratedPattern
        {
            get
            {
                var isIdentity = GetFlag(PropertyFlags.IsIdentity);
                var isComputed = GetFlag(PropertyFlags.IsComputed);

                return isIdentity == null && isComputed == null
                    ? (StoreGeneratedPattern?)null
                    : isIdentity.HasValue && isIdentity.Value
                        ? Metadata.StoreGeneratedPattern.Identity
                        : isComputed.HasValue && isComputed.Value
                            ? Metadata.StoreGeneratedPattern.Computed
                            : Metadata.StoreGeneratedPattern.None;
            }
            set
            {
                if (value == null)
                {
                    SetFlag(null, PropertyFlags.IsIdentity);
                    SetFlag(null, PropertyFlags.IsComputed);
                }
                else
                {
                    Check.IsDefined(value.Value, nameof(value));

                    SetFlag(value.Value == Metadata.StoreGeneratedPattern.Identity, PropertyFlags.IsIdentity);
                    SetFlag(value.Value == Metadata.StoreGeneratedPattern.Computed, PropertyFlags.IsComputed);
                }
            }
        }

        protected virtual StoreGeneratedPattern DefaultStoreGeneratedPattern => Metadata.StoreGeneratedPattern.None;

        public virtual bool? IsReadOnlyBeforeSave
        {
            get { return GetFlag(PropertyFlags.IsReadOnlyBeforeSave); }
            set { SetFlag(value, PropertyFlags.IsReadOnlyBeforeSave); }
        }

        protected virtual bool DefaultIsReadOnlyBeforeSave => StoreGeneratedPattern == Metadata.StoreGeneratedPattern.Computed;

        public virtual bool? IsReadOnlyAfterSave
        {
            get { return GetFlag(PropertyFlags.IsReadOnlyAfterSave); }
            set
            {
                if (value.HasValue
                    && !value.Value
                    && this.IsKey())
                {
                    throw new NotSupportedException(Strings.KeyPropertyMustBeReadOnly(Name, EntityType.Name));
                }
                SetFlag(value, PropertyFlags.IsReadOnlyAfterSave);
            }
        }

        protected virtual bool DefaultIsReadOnlyAfterSave
            => StoreGeneratedPattern == Metadata.StoreGeneratedPattern.Computed
               || this.IsKey();

        public virtual bool? IsValueGeneratedOnAdd
        {
            get { return GetFlag(PropertyFlags.IsValueGeneratedOnAdd); }
            set { SetFlag(value, PropertyFlags.IsValueGeneratedOnAdd); }
        }

        protected virtual bool DefaultIsValueGeneratedOnAdd => false;

        public virtual bool IsShadowProperty
        {
            get { return GetRequiredFlag(PropertyFlags.IsShadowProperty); }
            set
            {
                if (IsShadowProperty != value)
                {
                    SetRequiredFlag(value, PropertyFlags.IsShadowProperty);

                    EntityType.PropertyMetadataChanged(this);
                }

                SetRequiredFlag(value, PropertyFlags.IsShadowProperty);
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

                    EntityType.PropertyMetadataChanged(this);
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

        public virtual object SentinelValue { get; [param: CanBeNull] set; }

        private bool? GetFlag(PropertyFlags flag) => (_setFlags & flag) != 0 ? (_flags & flag) != 0 : (bool?)null;

        private bool GetRequiredFlag(PropertyFlags flag) => (_flags & flag) != 0;

        private void SetFlag(bool? value, PropertyFlags flag)
        {
            _setFlags = value.HasValue ? (_setFlags | flag) : (_setFlags & ~flag);
            _flags = value.HasValue && value.Value ? (_flags | flag) : (_flags & ~flag);
        }

        private void SetRequiredFlag(bool value, PropertyFlags flag)
        {
            _flags = value ? (_flags | flag) : (_flags & ~flag);
        }

        internal static string Format(IEnumerable<IProperty> properties)
            => "{" + string.Join(", ", properties.Select(p => "'" + p.Name + "'")) + "}";

        public static bool AreCompatible([NotNull] IReadOnlyList<Property> principalProperties,
            [NotNull] IReadOnlyList<Property> dependentProperties)
            => ArePropertyCountsEqual(principalProperties, dependentProperties)
               && ArePropertyTypesCompatible(principalProperties, dependentProperties);

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
            => principalProperties.Count == dependentProperties.Count;

        private static bool ArePropertyTypesCompatible(IReadOnlyList<Property> principalProperties, IReadOnlyList<Property> dependentProperties)
            => principalProperties.Select(p => p.ClrType.UnwrapNullableType()).SequenceEqual(dependentProperties.Select(p => p.ClrType.UnwrapNullableType()));

        bool IProperty.IsNullable => IsNullable ?? DefaultIsNullable;

        StoreGeneratedPattern IProperty.StoreGeneratedPattern => StoreGeneratedPattern ?? DefaultStoreGeneratedPattern;

        bool IProperty.IsReadOnlyBeforeSave => IsReadOnlyBeforeSave ?? DefaultIsReadOnlyBeforeSave;

        bool IProperty.IsReadOnlyAfterSave => IsReadOnlyAfterSave ?? DefaultIsReadOnlyAfterSave;

        bool IProperty.IsValueGeneratedOnAdd => IsValueGeneratedOnAdd ?? DefaultIsValueGeneratedOnAdd;

        bool IProperty.IsConcurrencyToken => IsConcurrencyToken ?? DefaultIsConcurrencyToken;

        object IProperty.SentinelValue => SentinelValue == null && !ClrType.IsNullableType() ? ClrType.GetDefaultValue() : SentinelValue;

        [Flags]
        private enum PropertyFlags : ushort
        {
            IsConcurrencyToken = 1,
            IsNullable = 2,
            IsReadOnlyBeforeSave = 4,
            IsReadOnlyAfterSave = 8,
            IsIdentity = 16,
            IsComputed = 32,
            IsValueGeneratedOnAdd = 64,
            IsShadowProperty = 128
        }
    }
}
