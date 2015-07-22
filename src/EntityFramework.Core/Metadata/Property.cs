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
    [DebuggerDisplay("{DeclaringEntityType.Name,nq}.{Name,nq} ({ClrType.Name,nq})")]
    public class Property : PropertyBase, IProperty
    {
        private PropertyFlags _flags;

        // TODO: Remove this once the model is readonly Issue #868
        private PropertyFlags _setFlags;

        private int _index;

        public Property([NotNull] string name, [NotNull] Type clrType, [NotNull] EntityType declaringEntityType, bool shadowProperty = false)
            : base(name)
        {
            Check.NotNull(clrType, nameof(clrType));
            Check.NotNull(declaringEntityType, nameof(declaringEntityType));

            ClrType = clrType;
            DeclaringEntityType = declaringEntityType;
            IsShadowProperty = shadowProperty;
        }

        public virtual Type ClrType { get; }

        public override EntityType DeclaringEntityType { get; }

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
                        throw new InvalidOperationException(Strings.CannotBeNullable(Name, DeclaringEntityType.DisplayName(), ClrType.Name));
                    }

                    if (DeclaringEntityType.FindPrimaryKey()?.Properties.Contains(this) ?? false)
                    {
                        throw new InvalidOperationException(Strings.CannotBeNullablePK(Name, DeclaringEntityType.DisplayName()));
                    }
                }

                SetFlag(value, PropertyFlags.IsNullable);
            }
        }

        protected virtual bool DefaultIsNullable => (DeclaringEntityType.FindPrimaryKey()?.Properties.Contains(this)) != true
                                                    && ClrType.IsNullableType();

        public virtual ValueGenerated? ValueGenerated
        {
            get
            {
                var isIdentity = GetFlag(PropertyFlags.ValueGeneratedOnAdd);
                var isComputed = GetFlag(PropertyFlags.ValueGeneratedOnAddOrUpdate);

                return isIdentity == null && isComputed == null
                    ? (ValueGenerated?)null
                    : isIdentity.HasValue && isIdentity.Value
                        ? Metadata.ValueGenerated.OnAdd
                        : isComputed.HasValue && isComputed.Value
                            ? Metadata.ValueGenerated.OnAddOrUpdate
                            : Metadata.ValueGenerated.Never;
            }
            set
            {
                if (value == null)
                {
                    SetFlag(null, PropertyFlags.ValueGeneratedOnAdd);
                    SetFlag(null, PropertyFlags.ValueGeneratedOnAddOrUpdate);
                }
                else
                {
                    Check.IsDefined(value.Value, nameof(value));

                    SetFlag(value.Value == Metadata.ValueGenerated.OnAdd, PropertyFlags.ValueGeneratedOnAdd);
                    SetFlag(value.Value == Metadata.ValueGenerated.OnAddOrUpdate, PropertyFlags.ValueGeneratedOnAddOrUpdate);
                }
            }
        }

        protected virtual ValueGenerated DefaultValueGenerated => Metadata.ValueGenerated.Never;

        public virtual bool? IsReadOnlyBeforeSave
        {
            get { return GetFlag(PropertyFlags.IsReadOnlyBeforeSave); }
            set { SetFlag(value, PropertyFlags.IsReadOnlyBeforeSave); }
        }

        protected virtual bool DefaultIsReadOnlyBeforeSave
            => ValueGenerated == Metadata.ValueGenerated.OnAddOrUpdate
               && !((IProperty)this).StoreGeneratedAlways;

        public virtual bool? IsReadOnlyAfterSave
        {
            get { return GetFlag(PropertyFlags.IsReadOnlyAfterSave); }
            set
            {
                if (value.HasValue
                    && !value.Value
                    && this.IsKey())
                {
                    throw new NotSupportedException(Strings.KeyPropertyMustBeReadOnly(Name, DeclaringEntityType.Name));
                }
                SetFlag(value, PropertyFlags.IsReadOnlyAfterSave);
            }
        }

        protected virtual bool DefaultIsReadOnlyAfterSave
            => (ValueGenerated == Metadata.ValueGenerated.OnAddOrUpdate
                && !((IProperty)this).StoreGeneratedAlways)
               || this.IsKey();

        public virtual bool? RequiresValueGenerator
        {
            get { return GetFlag(PropertyFlags.RequiresValueGenerator); }
            set { SetFlag(value, PropertyFlags.RequiresValueGenerator); }
        }

        protected virtual bool DefaultRequiresValueGenerator => false;

        public virtual bool IsShadowProperty
        {
            get { return GetRequiredFlag(PropertyFlags.IsShadowProperty); }
            set
            {
                if (IsShadowProperty != value)
                {
                    SetRequiredFlag(value, PropertyFlags.IsShadowProperty);

                    DeclaringEntityType.PropertyMetadataChanged(this);
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

                    DeclaringEntityType.PropertyMetadataChanged(this);
                }
            }
        }

        protected virtual bool DefaultIsConcurrencyToken => false;

        public virtual bool? StoreGeneratedAlways
        {
            get { return GetFlag(PropertyFlags.StoreGeneratedAlways); }
            set
            {
                if (StoreGeneratedAlways != value)
                {
                    SetFlag(value, PropertyFlags.StoreGeneratedAlways);

                    DeclaringEntityType.PropertyMetadataChanged(this);
                }
            }
        }

        protected virtual bool DefaultStoreGeneratedAlways
            => ValueGenerated == Metadata.ValueGenerated.OnAddOrUpdate && IsConcurrencyToken == true;

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

        public static void EnsureCompatible([NotNull] IReadOnlyList<Property> principalProperties, [NotNull] IReadOnlyList<Property> dependentProperties, [NotNull] EntityType principalEntityType, [NotNull] EntityType dependentEntityType)
        {
            if (!ArePropertyCountsEqual(principalProperties, dependentProperties))
            {
                throw new InvalidOperationException(
                    Strings.ForeignKeyCountMismatch(
                        Format(dependentProperties),
                        dependentProperties[0].DeclaringEntityType.Name,
                        Format(principalProperties),
                        principalProperties[0].DeclaringEntityType.Name));
            }

            if (!ArePropertyTypesCompatible(principalProperties, dependentProperties))
            {
                throw new InvalidOperationException(
                    Strings.ForeignKeyTypeMismatch(
                        Format(dependentProperties),
                        dependentProperties[0].DeclaringEntityType.Name,
                        principalProperties[0].DeclaringEntityType.Name));
            }
        }

        private static bool ArePropertyCountsEqual(IReadOnlyList<Property> principalProperties, IReadOnlyList<Property> dependentProperties)
            => principalProperties.Count == dependentProperties.Count;

        private static bool ArePropertyTypesCompatible(IReadOnlyList<Property> principalProperties, IReadOnlyList<Property> dependentProperties)
            => principalProperties.Select(p => p.ClrType.UnwrapNullableType()).SequenceEqual(dependentProperties.Select(p => p.ClrType.UnwrapNullableType()));

        bool IProperty.IsNullable => IsNullable ?? DefaultIsNullable;

        ValueGenerated IProperty.ValueGenerated => ValueGenerated ?? DefaultValueGenerated;

        bool IProperty.IsReadOnlyBeforeSave => IsReadOnlyBeforeSave ?? DefaultIsReadOnlyBeforeSave;

        bool IProperty.IsReadOnlyAfterSave => IsReadOnlyAfterSave ?? DefaultIsReadOnlyAfterSave;

        bool IProperty.RequiresValueGenerator => RequiresValueGenerator ?? DefaultRequiresValueGenerator;

        bool IProperty.IsConcurrencyToken => IsConcurrencyToken ?? DefaultIsConcurrencyToken;

        bool IProperty.StoreGeneratedAlways => StoreGeneratedAlways ?? DefaultStoreGeneratedAlways;

        object IProperty.SentinelValue => SentinelValue == null && !ClrType.IsNullableType() ? ClrType.GetDefaultValue() : SentinelValue;

        [Flags]
        private enum PropertyFlags : ushort
        {
            IsConcurrencyToken = 1,
            IsNullable = 2,
            IsReadOnlyBeforeSave = 4,
            IsReadOnlyAfterSave = 8,
            ValueGeneratedOnAdd = 16,
            ValueGeneratedOnAddOrUpdate = 32,
            RequiresValueGenerator = 64,
            IsShadowProperty = 128,
            StoreGeneratedAlways = 256
        }
    }
}
