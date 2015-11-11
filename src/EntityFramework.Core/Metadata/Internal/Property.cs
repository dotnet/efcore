// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Internal;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Metadata.Internal
{
    [DebuggerDisplay("{DeclaringEntityType.Name,nq}.{Name,nq} ({ClrType?.Name,nq})")]
    public class Property : ConventionalAnnotatable, IMutableProperty, IPropertyBaseAccessors, IPropertyIndexesAccessor
    {
        // Warning: Never access this field directly as access needs to be thread-safe
        private IClrPropertyGetter _getter;

        // Warning: Never access this field directly as access needs to be thread-safe
        private IClrPropertySetter _setter;

        // Warning: Never access this field directly as access needs to be thread-safe
        private PropertyIndexes _indexes;

        private PropertyFlags _flags;
        private PropertyFlags _setFlags;
        private Type _clrType;

        private ConfigurationSource _configurationSource;
        private ConfigurationSource? _clrTypeConfigurationSource;
        private ConfigurationSource? _isReadOnlyAfterSaveConfigurationSource;
        private ConfigurationSource? _isReadOnlyBeforeSaveConfigurationSource;
        private ConfigurationSource? _isNullableConfigurationSource;
        private ConfigurationSource? _isConcurrencyTokenConfigurationSource;
        private ConfigurationSource? _isShadowPropertyConfigurationSource;
        private ConfigurationSource? _isStoreGeneratedAlwaysConfigurationSource;
        private ConfigurationSource? _requiresValueGeneratorConfigurationSource;
        private ConfigurationSource? _valueGeneratedConfigurationSource;

        public Property([NotNull] string name, [NotNull] EntityType declaringEntityType, ConfigurationSource configurationSource)
        {
            Check.NotEmpty(name, nameof(name));
            Check.NotNull(declaringEntityType, nameof(declaringEntityType));

            Name = name;
            DeclaringEntityType = declaringEntityType;
            _configurationSource = configurationSource;

            Builder = new InternalPropertyBuilder(this, declaringEntityType.Model.Builder);
        }

        public virtual string Name { get; }
        public virtual EntityType DeclaringEntityType { get; }
        public virtual InternalPropertyBuilder Builder { get; [param: CanBeNull] set; }

        public virtual ConfigurationSource GetConfigurationSource() => _configurationSource;

        public virtual void UpdateConfigurationSource(ConfigurationSource configurationSource)
            => _configurationSource = _configurationSource.Max(configurationSource);

        public virtual Type ClrType
        {
            get { return _clrType ?? DefaultClrType; }
            [param: NotNull]
            set { HasClrType(value, ConfigurationSource.Explicit); }
        }

        public virtual void HasClrType([NotNull] Type type, ConfigurationSource configurationSource)
        {
            Check.NotNull(type, nameof(type));
            if (type != ((IProperty)this).ClrType)
            {
                var foreignKey = this.FindReferencingForeignKeys().FirstOrDefault();
                if (foreignKey != null)
                {
                    throw new InvalidOperationException(
                        CoreStrings.PropertyClrTypeCannotBeChangedWhenReferenced(Name, Format(foreignKey.Properties), foreignKey.DeclaringEntityType.Name));
                }
            }
            _clrType = type;
            UpdateClrTypeConfigurationSource(configurationSource);
        }

        private static Type DefaultClrType => typeof(string);

        public virtual ConfigurationSource? GetClrTypeConfigurationSource() => _clrTypeConfigurationSource;

        private void UpdateClrTypeConfigurationSource(ConfigurationSource configurationSource)
            => _clrTypeConfigurationSource = configurationSource.Max(_clrTypeConfigurationSource);

        public virtual bool IsNullable
        {
            get { return GetFlag(PropertyFlags.IsNullable) ?? DefaultIsNullable; }
            set { SetIsNullable(value, ConfigurationSource.Explicit); }
        }

        public virtual void SetIsNullable(bool nullable, ConfigurationSource configurationSource)
        {
            if (nullable)
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

            SetFlag(nullable, PropertyFlags.IsNullable);
            UpdateIsNullableConfigurationSource(configurationSource);
        }

        private bool DefaultIsNullable => (DeclaringEntityType.FindPrimaryKey()?.Properties.Contains(this) != true)
                                          && ((IProperty)this).ClrType.IsNullableType();

        public virtual ConfigurationSource? GetIsNullableConfigurationSource() => _isNullableConfigurationSource;

        private void UpdateIsNullableConfigurationSource(ConfigurationSource configurationSource)
            => _isNullableConfigurationSource = configurationSource.Max(_isNullableConfigurationSource);

        public virtual ValueGenerated ValueGenerated
        {
            get
            {
                var isIdentity = GetFlag(PropertyFlags.ValueGeneratedOnAdd);
                var isComputed = GetFlag(PropertyFlags.ValueGeneratedOnAddOrUpdate);

                return (isIdentity == null) && (isComputed == null)
                    ? DefaultValueGenerated
                    : isIdentity.HasValue && isIdentity.Value
                        ? ValueGenerated.OnAdd
                        : isComputed.HasValue && isComputed.Value
                            ? ValueGenerated.OnAddOrUpdate
                            : ValueGenerated.Never;
            }
            set { SetValueGenerated(value, ConfigurationSource.Explicit); }
        }

        public virtual void SetValueGenerated(ValueGenerated? valueGenerated, ConfigurationSource configurationSource)
        {
            if (valueGenerated == null)
            {
                SetFlag(null, PropertyFlags.ValueGeneratedOnAdd);
                SetFlag(null, PropertyFlags.ValueGeneratedOnAddOrUpdate);
                _valueGeneratedConfigurationSource = null;
            }
            else
            {
                Check.IsDefined(valueGenerated.Value, nameof(valueGenerated));

                SetFlag(valueGenerated.Value == ValueGenerated.OnAdd, PropertyFlags.ValueGeneratedOnAdd);
                SetFlag(valueGenerated.Value == ValueGenerated.OnAddOrUpdate, PropertyFlags.ValueGeneratedOnAddOrUpdate);
                UpdateValueGeneratedConfigurationSource(configurationSource);
            }
        }

        private static ValueGenerated DefaultValueGenerated => ValueGenerated.Never;
        public virtual ConfigurationSource? GetValueGeneratedConfigurationSource() => _valueGeneratedConfigurationSource;

        private void UpdateValueGeneratedConfigurationSource(ConfigurationSource configurationSource)
            => _valueGeneratedConfigurationSource = configurationSource.Max(_valueGeneratedConfigurationSource);

        public virtual bool IsReadOnlyBeforeSave
        {
            get { return GetFlag(PropertyFlags.IsReadOnlyBeforeSave) ?? DefaultIsReadOnlyBeforeSave; }
            set { SetIsReadOnlyBeforeSave(value, ConfigurationSource.Explicit); }
        }

        public virtual void SetIsReadOnlyBeforeSave(bool readOnlyBeforeSave, ConfigurationSource configurationSource)
        {
            SetFlag(readOnlyBeforeSave, PropertyFlags.IsReadOnlyBeforeSave);
            UpdateIsReadOnlyBeforeSaveConfigurationSource(configurationSource);
        }

        private bool DefaultIsReadOnlyBeforeSave
            => (ValueGenerated == ValueGenerated.OnAddOrUpdate)
               && !((IProperty)this).IsStoreGeneratedAlways;

        public virtual ConfigurationSource? GetIsReadOnlyBeforeSaveConfigurationSource() => _isReadOnlyBeforeSaveConfigurationSource;

        private void UpdateIsReadOnlyBeforeSaveConfigurationSource(ConfigurationSource configurationSource)
            => _isReadOnlyBeforeSaveConfigurationSource = configurationSource.Max(_isReadOnlyBeforeSaveConfigurationSource);

        public virtual bool IsReadOnlyAfterSave
        {
            get { return GetFlag(PropertyFlags.IsReadOnlyAfterSave) ?? DefaultIsReadOnlyAfterSave; }
            set { SetIsReadOnlyAfterSave(value, ConfigurationSource.Explicit); }
        }

        public virtual void SetIsReadOnlyAfterSave(bool readOnlyAfterSave, ConfigurationSource configurationSource)
        {
            if (!readOnlyAfterSave
                && this.IsKey())
            {
                throw new NotSupportedException(CoreStrings.KeyPropertyMustBeReadOnly(Name, DeclaringEntityType.Name));
            }
            SetFlag(readOnlyAfterSave, PropertyFlags.IsReadOnlyAfterSave);
            UpdateIsReadOnlyAfterSaveConfigurationSource(configurationSource);
        }

        private bool DefaultIsReadOnlyAfterSave
            => ((ValueGenerated == ValueGenerated.OnAddOrUpdate)
                && !((IProperty)this).IsStoreGeneratedAlways)
               || this.IsKey();

        public virtual ConfigurationSource? GetIsReadOnlyAfterSaveConfigurationSource() => _isReadOnlyAfterSaveConfigurationSource;

        private void UpdateIsReadOnlyAfterSaveConfigurationSource(ConfigurationSource configurationSource)
            => _isReadOnlyAfterSaveConfigurationSource = configurationSource.Max(_isReadOnlyAfterSaveConfigurationSource);

        public virtual bool RequiresValueGenerator
        {
            get { return GetFlag(PropertyFlags.RequiresValueGenerator) ?? DefaultRequiresValueGenerator; }
            set { SetRequiresValueGenerator(value, ConfigurationSource.Explicit); }
        }

        public virtual void SetRequiresValueGenerator(bool requiresValueGenerator, ConfigurationSource configurationSource)
        {
            SetFlag(requiresValueGenerator, PropertyFlags.RequiresValueGenerator);
            UpdateRequiresValueGeneratorConfigurationSource(configurationSource);
        }

        private static bool DefaultRequiresValueGenerator => false;
        public virtual ConfigurationSource? GetRequiresValueGeneratorConfigurationSource() => _requiresValueGeneratorConfigurationSource;

        private void UpdateRequiresValueGeneratorConfigurationSource(ConfigurationSource configurationSource)
            => _requiresValueGeneratorConfigurationSource = configurationSource.Max(_requiresValueGeneratorConfigurationSource);

        public virtual bool IsShadowProperty
        {
            get { return GetFlag(PropertyFlags.IsShadowProperty) ?? DefaultIsShadowProperty; }
            set { SetIsShadowProperty(value, ConfigurationSource.Explicit); }
        }

        public virtual void SetIsShadowProperty(bool shadowProperty, ConfigurationSource configurationSource)
        {
            if (IsShadowProperty != shadowProperty)
            {
                if (shadowProperty == false)
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

                SetFlag(shadowProperty, PropertyFlags.IsShadowProperty);

                DeclaringEntityType.PropertyMetadataChanged();
            }
            else
            {
                SetFlag(shadowProperty, PropertyFlags.IsShadowProperty);
            }

            UpdateIsShadowPropertyConfigurationSource(configurationSource);
        }

        private static bool DefaultIsShadowProperty => true;
        public virtual ConfigurationSource? GetIsShadowPropertyConfigurationSource() => _isShadowPropertyConfigurationSource;

        private void UpdateIsShadowPropertyConfigurationSource(ConfigurationSource configurationSource)
            => _isShadowPropertyConfigurationSource = configurationSource.Max(_isShadowPropertyConfigurationSource);

        public virtual bool IsConcurrencyToken
        {
            get { return GetFlag(PropertyFlags.IsConcurrencyToken) ?? DefaultIsConcurrencyToken; }
            set { SetIsConcurrencyToken(value, ConfigurationSource.Explicit); }
        }

        public virtual void SetIsConcurrencyToken(bool concurrencyToken, ConfigurationSource configurationSource)
        {
            if (IsConcurrencyToken != concurrencyToken)
            {
                SetFlag(concurrencyToken, PropertyFlags.IsConcurrencyToken);

                DeclaringEntityType.PropertyMetadataChanged();
            }
            UpdateIsConcurrencyTokenConfigurationSource(configurationSource);
        }

        private static bool DefaultIsConcurrencyToken => false;
        public virtual ConfigurationSource? GetIsConcurrencyTokenConfigurationSource() => _isConcurrencyTokenConfigurationSource;

        private void UpdateIsConcurrencyTokenConfigurationSource(ConfigurationSource configurationSource)
            => _isConcurrencyTokenConfigurationSource = configurationSource.Max(_isConcurrencyTokenConfigurationSource);

        public virtual bool IsStoreGeneratedAlways
        {
            get { return GetFlag(PropertyFlags.StoreGeneratedAlways) ?? DefaultStoreGeneratedAlways; }
            set { SetIsStoreGeneratedAlways(value, ConfigurationSource.Explicit); }
        }

        public virtual void SetIsStoreGeneratedAlways(bool storeGeneratedAlways, ConfigurationSource configurationSource)
        {
            if (IsStoreGeneratedAlways != storeGeneratedAlways)
            {
                SetFlag(storeGeneratedAlways, PropertyFlags.StoreGeneratedAlways);

                DeclaringEntityType.PropertyMetadataChanged();
            }
            UpdateIsStoreGeneratedAlwaysConfigurationSource(configurationSource);
        }

        private bool DefaultStoreGeneratedAlways => (ValueGenerated == ValueGenerated.OnAddOrUpdate) && IsConcurrencyToken;
        public virtual ConfigurationSource? GetIsStoreGeneratedAlwaysConfigurationSource() => _isStoreGeneratedAlwaysConfigurationSource;

        private void UpdateIsStoreGeneratedAlwaysConfigurationSource(ConfigurationSource configurationSource)
            => _isStoreGeneratedAlwaysConfigurationSource = configurationSource.Max(_isStoreGeneratedAlwaysConfigurationSource);

        public virtual IEnumerable<ForeignKey> FindContainingForeignKeys()
            => ((IProperty)this).FindContainingForeignKeys().Cast<ForeignKey>();

        private bool? GetFlag(PropertyFlags flag) => (_setFlags & flag) != 0 ? (_flags & flag) != 0 : (bool?)null;

        private void SetFlag(bool? value, PropertyFlags flag)
        {
            _setFlags = value.HasValue ? _setFlags | flag : _setFlags & ~flag;
            _flags = value.HasValue && value.Value ? _flags | flag : _flags & ~flag;
        }

        internal static string Format(IEnumerable<IProperty> properties)
            => "{" + string.Join(", ", properties.Select(p => "'" + p.Name + "'")) + "}";

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
                    && (entityType.ClrType.GetRuntimeProperties().FirstOrDefault(p => p.Name == property.Name) != null)));
        }

        public virtual IClrPropertyGetter Getter
            => LazyInitializer.EnsureInitialized(ref _getter, () => new ClrPropertyGetterFactory().Create(this));

        public virtual IClrPropertySetter Setter
            => LazyInitializer.EnsureInitialized(ref _setter, () => new ClrPropertySetterFactory().Create(this));

        public virtual PropertyIndexes Indexes
        {
            get { return LazyInitializer.EnsureInitialized(ref _indexes, CalculateIndexes); }

            set
            {
                if (value == null)
                {
                    // This path should only kick in when the model is still mutable and therefore access does not need
                    // to be thread-safe.
                    _indexes = null;
                }
                else
                {
                    LazyInitializer.EnsureInitialized(ref _indexes, () => value);
                }
            }
        }

        private PropertyIndexes CalculateIndexes() => DeclaringEntityType.CalculateIndexes(this);
    }
}
