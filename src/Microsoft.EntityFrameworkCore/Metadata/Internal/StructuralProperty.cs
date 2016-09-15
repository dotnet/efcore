// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Metadata.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public abstract class StructuralProperty : AccessibleProperty, IMutableStructuralProperty
    {
        private int _flags;

        private ConfigurationSource _configurationSource;
        private ConfigurationSource? _isReadOnlyAfterSaveConfigurationSource;
        private ConfigurationSource? _isReadOnlyBeforeSaveConfigurationSource;
        private ConfigurationSource? _isNullableConfigurationSource;
        private ConfigurationSource? _isConcurrencyTokenConfigurationSource;
        private ConfigurationSource? _isStoreGeneratedAlwaysConfigurationSource;
        private ConfigurationSource? _requiresValueGeneratorConfigurationSource;
        private ConfigurationSource? _valueGeneratedConfigurationSource;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected StructuralProperty(
            [NotNull] string name,
            [NotNull] Type clrType,
            ConfigurationSource configurationSource)
            : base(name, null)
        {
            Check.NotNull(clrType, nameof(clrType));

            ClrType = clrType;
            _configurationSource = configurationSource;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected StructuralProperty(
            [NotNull] PropertyInfo propertyInfo,
            ConfigurationSource configurationSource)
            : base(Check.NotNull(propertyInfo, nameof(propertyInfo)).Name, propertyInfo)
        {
            ClrType = propertyInfo.PropertyType;
            _configurationSource = configurationSource;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual ConfigurationSource GetConfigurationSource() => _configurationSource;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual void UpdateConfigurationSource(ConfigurationSource configurationSource)
            => _configurationSource = _configurationSource.Max(configurationSource);

        // Needed for a workaround before reference counting is implemented
        // Issue #214
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual void SetConfigurationSource(ConfigurationSource configurationSource)
            => _configurationSource = configurationSource;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override Type ClrType { get; }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual bool IsNullable
        {
            get
            {
                bool value;
                return TryGetFlag(PropertyFlags.IsNullable, out value) ? value : DefaultIsNullable;
            }
            set { SetIsNullable(value, ConfigurationSource.Explicit); }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual void SetIsNullable(bool nullable, ConfigurationSource configurationSource)
        {
            if (nullable 
                && !ClrType.IsNullableType())
            {
                throw new InvalidOperationException(CoreStrings.CannotBeNullable(Name, DeclaringType.DisplayName(), ClrType.ShortDisplayName()));
            }

            UpdateIsNullableConfigurationSource(configurationSource);

            var isChanging = IsNullable != nullable;
            SetFlag(nullable, PropertyFlags.IsNullable);
            if (isChanging)
            {
                OnPropertyNullableChanged();
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected abstract void OnPropertyNullableChanged();

        private bool DefaultIsNullable => ClrType.IsNullableType();

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual ConfigurationSource? GetIsNullableConfigurationSource() => _isNullableConfigurationSource;

        private void UpdateIsNullableConfigurationSource(ConfigurationSource configurationSource)
            => _isNullableConfigurationSource = configurationSource.Max(_isNullableConfigurationSource);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual ValueGenerated ValueGenerated
        {
            get
            {
                var value = _flags & (int)PropertyFlags.ValueGenerated;

                return value == 0 ? DefaultValueGenerated : (ValueGenerated)((value >> 8) - 1);
            }
            set { SetValueGenerated(value, ConfigurationSource.Explicit); }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual void SetValueGenerated(ValueGenerated? valueGenerated, ConfigurationSource configurationSource)
        {
            _flags &= ~(int)PropertyFlags.ValueGenerated;

            if (valueGenerated == null)
            {
                _valueGeneratedConfigurationSource = null;
            }
            else
            {
                _flags |= ((int)valueGenerated + 1) << 8;
                UpdateValueGeneratedConfigurationSource(configurationSource);
            }
        }

        private static ValueGenerated DefaultValueGenerated => ValueGenerated.Never;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual ConfigurationSource? GetValueGeneratedConfigurationSource() => _valueGeneratedConfigurationSource;

        private void UpdateValueGeneratedConfigurationSource(ConfigurationSource configurationSource)
            => _valueGeneratedConfigurationSource = configurationSource.Max(_valueGeneratedConfigurationSource);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual bool IsReadOnlyBeforeSave
        {
            get
            {
                bool value;
                return TryGetFlag(PropertyFlags.IsReadOnlyBeforeSave, out value) ? value : DefaultIsReadOnlyBeforeSave;
            }
            set { SetIsReadOnlyBeforeSave(value, ConfigurationSource.Explicit); }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual void SetIsReadOnlyBeforeSave(bool readOnlyBeforeSave, ConfigurationSource configurationSource)
        {
            SetFlag(readOnlyBeforeSave, PropertyFlags.IsReadOnlyBeforeSave);
            UpdateIsReadOnlyBeforeSaveConfigurationSource(configurationSource);
        }

        private bool DefaultIsReadOnlyBeforeSave
            => (ValueGenerated == ValueGenerated.OnAddOrUpdate)
               && !IsStoreGeneratedAlways;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual ConfigurationSource? GetIsReadOnlyBeforeSaveConfigurationSource() => _isReadOnlyBeforeSaveConfigurationSource;

        private void UpdateIsReadOnlyBeforeSaveConfigurationSource(ConfigurationSource configurationSource)
            => _isReadOnlyBeforeSaveConfigurationSource = configurationSource.Max(_isReadOnlyBeforeSaveConfigurationSource);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual bool IsReadOnlyAfterSave
        {
            get
            {
                bool value;
                return TryGetFlag(PropertyFlags.IsReadOnlyAfterSave, out value) ? value : DefaultIsReadOnlyAfterSave;
            }
            set { SetIsReadOnlyAfterSave(value, ConfigurationSource.Explicit); }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual void SetIsReadOnlyAfterSave(bool readOnlyAfterSave, ConfigurationSource configurationSource)
        {
            SetFlag(readOnlyAfterSave, PropertyFlags.IsReadOnlyAfterSave);
            UpdateIsReadOnlyAfterSaveConfigurationSource(configurationSource);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected abstract bool DefaultIsReadOnlyAfterSave { get; }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual ConfigurationSource? GetIsReadOnlyAfterSaveConfigurationSource() => _isReadOnlyAfterSaveConfigurationSource;

        private void UpdateIsReadOnlyAfterSaveConfigurationSource(ConfigurationSource configurationSource)
            => _isReadOnlyAfterSaveConfigurationSource = configurationSource.Max(_isReadOnlyAfterSaveConfigurationSource);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual bool RequiresValueGenerator
        {
            get
            {
                bool value;
                return TryGetFlag(PropertyFlags.RequiresValueGenerator, out value) ? value : DefaultRequiresValueGenerator;
            }
            set { SetRequiresValueGenerator(value, ConfigurationSource.Explicit); }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual void SetRequiresValueGenerator(bool requiresValueGenerator, ConfigurationSource configurationSource)
        {
            SetFlag(requiresValueGenerator, PropertyFlags.RequiresValueGenerator);
            UpdateRequiresValueGeneratorConfigurationSource(configurationSource);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected abstract bool DefaultRequiresValueGenerator { get; }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual ConfigurationSource? GetRequiresValueGeneratorConfigurationSource() => _requiresValueGeneratorConfigurationSource;

        private void UpdateRequiresValueGeneratorConfigurationSource(ConfigurationSource configurationSource)
            => _requiresValueGeneratorConfigurationSource = configurationSource.Max(_requiresValueGeneratorConfigurationSource);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual bool IsShadowProperty => MemberInfo == null;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual bool IsConcurrencyToken
        {
            get
            {
                bool value;
                return TryGetFlag(PropertyFlags.IsConcurrencyToken, out value) ? value : DefaultIsConcurrencyToken;
            }
            set { SetIsConcurrencyToken(value, ConfigurationSource.Explicit); }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual void SetIsConcurrencyToken(bool concurrencyToken, ConfigurationSource configurationSource)
        {
            if (IsConcurrencyToken != concurrencyToken)
            {
                SetFlag(concurrencyToken, PropertyFlags.IsConcurrencyToken);

                DeclaringType.PropertyMetadataChanged();
            }
            UpdateIsConcurrencyTokenConfigurationSource(configurationSource);
        }

        private static bool DefaultIsConcurrencyToken => false;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual ConfigurationSource? GetIsConcurrencyTokenConfigurationSource() => _isConcurrencyTokenConfigurationSource;

        private void UpdateIsConcurrencyTokenConfigurationSource(ConfigurationSource configurationSource)
            => _isConcurrencyTokenConfigurationSource = configurationSource.Max(_isConcurrencyTokenConfigurationSource);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual bool IsStoreGeneratedAlways
        {
            get
            {
                bool value;
                return TryGetFlag(PropertyFlags.StoreGeneratedAlways, out value) ? value : DefaultStoreGeneratedAlways;
            }
            set { SetIsStoreGeneratedAlways(value, ConfigurationSource.Explicit); }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual void SetIsStoreGeneratedAlways(bool storeGeneratedAlways, ConfigurationSource configurationSource)
        {
            if (IsStoreGeneratedAlways != storeGeneratedAlways)
            {
                SetFlag(storeGeneratedAlways, PropertyFlags.StoreGeneratedAlways);

                DeclaringType.PropertyMetadataChanged();
            }
            UpdateIsStoreGeneratedAlwaysConfigurationSource(configurationSource);
        }

        private bool DefaultStoreGeneratedAlways => (ValueGenerated == ValueGenerated.OnAddOrUpdate) && IsConcurrencyToken;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual ConfigurationSource? GetIsStoreGeneratedAlwaysConfigurationSource() => _isStoreGeneratedAlwaysConfigurationSource;

        private void UpdateIsStoreGeneratedAlwaysConfigurationSource(ConfigurationSource configurationSource)
            => _isStoreGeneratedAlwaysConfigurationSource = configurationSource.Max(_isStoreGeneratedAlwaysConfigurationSource);

        private bool TryGetFlag(PropertyFlags flag, out bool value)
        {
            var coded = _flags & (int)flag;
            value = coded == (int)flag;
            return coded != 0;
        }

        private void SetFlag(bool value, PropertyFlags flag)
        {
            if (value)
            {
                _flags |= (int)flag;
            }
            else
            {
                var falseValue = ((int)flag << 1) & (int)flag;
                _flags = (_flags & ~(int)flag) | falseValue;
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static string Format([NotNull] IEnumerable<IAccessibleProperty> properties, bool includeTypes = false)
            => "{"
               + string.Join(", ",
                   properties.Select(p => "'" + p.Name + "'" + (includeTypes ? " : " + p.ClrType.DisplayName(fullName: false) : "")))
               + "}";

        private enum PropertyFlags
        {
            IsConcurrencyToken = 3 << 0,
            IsNullable = 3 << 2,
            IsReadOnlyBeforeSave = 3 << 4,
            IsReadOnlyAfterSave = 3 << 6,
            ValueGenerated = 7 << 8,
            RequiresValueGenerator = 3 << 11,
            StoreGeneratedAlways = 3 << 13
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static bool AreCompatible([NotNull] IReadOnlyList<StructuralProperty> properties, [NotNull] IMutableStructuralType entityType)
        {
            Check.NotNull(properties, nameof(properties));
            Check.NotNull(entityType, nameof(entityType));

            return properties.All(property =>
                property.IsShadowProperty
                || (entityType.HasClrType()
                    && ((property.PropertyInfo != null
                         && entityType.ClrType.GetRuntimeProperties().FirstOrDefault(p => p.Name == property.Name) != null)
                        || (property.FieldInfo != null
                            && entityType.ClrType.GetRuntimeFields().FirstOrDefault(p => p.Name == property.Name) != null))));
        }
    }
}
