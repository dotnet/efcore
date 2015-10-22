// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Internal;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Metadata
{
    [DebuggerDisplay("{DeclaringEntityType.Name,nq}.{Name,nq} ({ClrType?.Name,nq})")]
    public class Property : Annotatable, IMutableProperty
    {
        private PropertyFlags _flags;
        private PropertyFlags _setFlags;
        private int _index;
        private Type _clrType;

        public Property([NotNull] string name, [NotNull] EntityType declaringEntityType)
        {
            Check.NotEmpty(name, nameof(name));
            Check.NotNull(declaringEntityType, nameof(declaringEntityType));

            Name = name;
            DeclaringEntityType = declaringEntityType;
        }

        public virtual string Name { get; }

        public virtual Type ClrType
        {
            get { return _clrType; }
            [param: NotNull]
            set
            {
                Check.NotNull(value, nameof(value));
                if (value != ((IProperty)this).ClrType)
                {
                    var foreignKey = this.FindReferencingForeignKeys().FirstOrDefault();
                    if (foreignKey != null)
                    {
                        throw new InvalidOperationException(
                            CoreStrings.PropertyClrTypeCannotBeChangedWhenReferenced(Name, Format(foreignKey.Properties), foreignKey.DeclaringEntityType.Name));
                    }
                }
                _clrType = value;
            }
        }

        protected virtual Type DefaultClrType => typeof(string);

        public virtual EntityType DeclaringEntityType { get; }

        public virtual bool? IsNullable
        {
            get { return GetFlag(PropertyFlags.IsNullable); }
            set
            {
                if (value.HasValue
                    && value.Value)
                {
                    if (!((IProperty)this).ClrType.IsNullableType())
                    {
                        throw new InvalidOperationException(CoreStrings.CannotBeNullable(Name, DeclaringEntityType.DisplayName(), ((IProperty)this).ClrType.Name));
                    }

                    if (DeclaringEntityType.FindPrimaryKey()?.Properties.Contains(this) ?? false)
                    {
                        throw new InvalidOperationException(CoreStrings.CannotBeNullablePK(Name, DeclaringEntityType.DisplayName()));
                    }
                }

                SetFlag(value, PropertyFlags.IsNullable);
            }
        }

        protected virtual bool DefaultIsNullable => (DeclaringEntityType.FindPrimaryKey()?.Properties.Contains(this)) != true
                                                    && ((IProperty)this).ClrType.IsNullableType();

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
                    throw new NotSupportedException(CoreStrings.KeyPropertyMustBeReadOnly(Name, DeclaringEntityType.Name));
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

        public virtual bool? IsShadowProperty
        {
            get { return GetFlag(PropertyFlags.IsShadowProperty); }
            set
            {
                if (IsShadowProperty != value)
                {
                    if (value == false)
                    {
                        if (DeclaringEntityType.ClrType == null)
                        {
                            throw new InvalidOperationException(CoreStrings.ClrPropertyOnShadowEntity(Name, DeclaringEntityType.DisplayName()));
                        }

                        var clrProperty = DeclaringEntityType.ClrType.GetPropertiesInHierarchy(Name).FirstOrDefault();
                        if (clrProperty == null)
                        {
                            throw new InvalidOperationException(CoreStrings.NoClrProperty(Name, DeclaringEntityType.DisplayName()));
                        }

                        if (ClrType == null)
                        {
                            ClrType = clrProperty.PropertyType;
                        }
                        else if (ClrType != clrProperty.PropertyType)
                        {
                            throw new InvalidOperationException(CoreStrings.PropertyWrongClrType(Name, DeclaringEntityType.DisplayName()));
                        }
                    }

                    SetFlag(value, PropertyFlags.IsShadowProperty);

                    DeclaringEntityType.PropertyMetadataChanged(this);
                }

                SetFlag(value, PropertyFlags.IsShadowProperty);
            }
        }

        protected virtual bool DefaultIsShadowProperty => true;

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

        private bool? GetFlag(PropertyFlags flag) => (_setFlags & flag) != 0 ? (_flags & flag) != 0 : (bool?)null;

        private void SetFlag(bool? value, PropertyFlags flag)
        {
            _setFlags = value.HasValue ? (_setFlags | flag) : (_setFlags & ~flag);
            _flags = value.HasValue && value.Value ? (_flags | flag) : (_flags & ~flag);
        }

        internal static string Format(IEnumerable<IProperty> properties)
            => "{" + String.Join(", ", properties.Select(p => "'" + p.Name + "'")) + "}";

        Type IProperty.ClrType => ClrType ?? DefaultClrType;
        bool IProperty.IsConcurrencyToken => IsConcurrencyToken ?? DefaultIsConcurrencyToken;
        bool IProperty.IsReadOnlyBeforeSave => IsReadOnlyBeforeSave ?? DefaultIsReadOnlyBeforeSave;
        bool IProperty.IsReadOnlyAfterSave => IsReadOnlyAfterSave ?? DefaultIsReadOnlyAfterSave;
        bool IProperty.IsShadowProperty => IsShadowProperty ?? DefaultIsShadowProperty;
        bool IProperty.IsNullable => IsNullable ?? DefaultIsNullable;
        ValueGenerated IProperty.ValueGenerated => ValueGenerated ?? DefaultValueGenerated;
        bool IProperty.RequiresValueGenerator => RequiresValueGenerator ?? DefaultRequiresValueGenerator;

        bool IProperty.StoreGeneratedAlways => StoreGeneratedAlways ?? DefaultStoreGeneratedAlways;
        IEntityType IPropertyBase.DeclaringEntityType => DeclaringEntityType;
        IMutableEntityType IMutableProperty.DeclaringEntityType => DeclaringEntityType;

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

        public static bool AreCompatible([NotNull] IReadOnlyList<Property> properties, [NotNull] EntityType entityType)
        {
            Check.NotNull(properties, nameof(properties));
            Check.NotNull(entityType, nameof(entityType));

            return properties.All(property =>
                ((IProperty)property).IsShadowProperty
                || (entityType.HasClrType()
                    && entityType.ClrType.GetRuntimeProperties().FirstOrDefault(p => p.Name == property.Name) != null));
        }
    }
}
