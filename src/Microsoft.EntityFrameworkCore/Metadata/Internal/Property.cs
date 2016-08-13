// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
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
    [DebuggerDisplay("{DebuggerDisplay, nq}")]
    public class Property : PropertyBase, IMutableProperty
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
        public Property(
            [NotNull] string name,
            [NotNull] Type clrType,
            [NotNull] EntityType declaringEntityType,
            ConfigurationSource configurationSource)
            : base(name, null)
        {
            Check.NotNull(clrType, nameof(clrType));
            Check.NotNull(declaringEntityType, nameof(declaringEntityType));

            DeclaringEntityType = declaringEntityType;
            ClrType = clrType;
            Initialize(declaringEntityType, configurationSource);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public Property(
            [NotNull] PropertyInfo propertyInfo,
            [NotNull] EntityType declaringEntityType,
            ConfigurationSource configurationSource)
            : base(Check.NotNull(propertyInfo, nameof(propertyInfo)).Name, propertyInfo)
        {
            Check.NotNull(declaringEntityType, nameof(declaringEntityType));

            DeclaringEntityType = declaringEntityType;
            ClrType = propertyInfo.PropertyType;
            Initialize(declaringEntityType, configurationSource);
        }

        private void Initialize(EntityType declaringEntityType, ConfigurationSource configurationSource)
        {
            _configurationSource = configurationSource;

            Builder = new InternalPropertyBuilder(this, declaringEntityType.Model.Builder);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override EntityType DeclaringEntityType { get; }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual InternalPropertyBuilder Builder { get; [param: CanBeNull] set; }

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
            if (nullable)
            {
                if (!ClrType.IsNullableType())
                {
                    throw new InvalidOperationException(CoreStrings.CannotBeNullable(Name, DeclaringEntityType.DisplayName(), ClrType.ShortDisplayName()));
                }

                if (Keys != null)
                {
                    throw new InvalidOperationException(CoreStrings.CannotBeNullablePK(Name, DeclaringEntityType.DisplayName()));
                }
            }

            UpdateIsNullableConfigurationSource(configurationSource);

            var isChanging = IsNullable != nullable;
            SetFlag(nullable, PropertyFlags.IsNullable);
            if (isChanging)
            {
                DeclaringEntityType.Model.ConventionDispatcher.OnPropertyNullableChanged(Builder);
            }
        }

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
        protected override void OnFieldInfoSet(FieldInfo oldFieldInfo)
            => DeclaringEntityType.Model.ConventionDispatcher.OnPropertyFieldChanged(Builder, oldFieldInfo);

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
            if (!readOnlyAfterSave
                && Keys != null)
            {
                throw new InvalidOperationException(CoreStrings.KeyPropertyMustBeReadOnly(Name, DeclaringEntityType.DisplayName()));
            }
            SetFlag(readOnlyAfterSave, PropertyFlags.IsReadOnlyAfterSave);
            UpdateIsReadOnlyAfterSaveConfigurationSource(configurationSource);
        }

        private bool DefaultIsReadOnlyAfterSave
            => ((ValueGenerated == ValueGenerated.OnAddOrUpdate)
                && !IsStoreGeneratedAlways)
               || Keys != null;

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

        private bool DefaultRequiresValueGenerator
            => this.IsKey()
               && !this.IsForeignKey()
               && ValueGenerated == ValueGenerated.OnAdd;

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

                DeclaringEntityType.PropertyMetadataChanged();
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

                DeclaringEntityType.PropertyMetadataChanged();
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

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual IEnumerable<ForeignKey> GetContainingForeignKeys()
            => ((IProperty)this).GetContainingForeignKeys().Cast<ForeignKey>();

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual IEnumerable<Key> GetContainingKeys()
            => ((IProperty)this).GetContainingKeys().Cast<Key>();

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual IEnumerable<Index> GetContainingIndexes()
            => ((IProperty)this).GetContainingIndexes().Cast<Index>();

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
        public static string Format([NotNull] IEnumerable<IProperty> properties)
            => "{" + string.Join(", ", properties.Select(p => "'" + p.Name + "'")) + "}";

        [UsedImplicitly]
        private string DebuggerDisplay => $"{DeclaringEntityType.DisplayName()}.{Name} ({ClrType?.ShortDisplayName()})";

        IEntityType IPropertyBase.DeclaringEntityType => DeclaringEntityType;
        IMutableEntityType IMutableProperty.DeclaringEntityType => DeclaringEntityType;

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
        public static bool AreCompatible([NotNull] IReadOnlyList<Property> properties, [NotNull] EntityType entityType)
        {
            Check.NotNull(properties, nameof(properties));
            Check.NotNull(entityType, nameof(entityType));

            return properties.All(property =>
                property.IsShadowProperty
                || (entityType.HasClrType()
                    && (entityType.ClrType.GetRuntimeProperties().FirstOrDefault(p => p.Name == property.Name) != null)));
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual IKey PrimaryKey { get; [param: CanBeNull] set; }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual IReadOnlyList<IKey> Keys { get; [param: CanBeNull] set; }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual IReadOnlyList<IForeignKey> ForeignKeys { get; [param: CanBeNull] set; }
      
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual IReadOnlyList<IIndex> Indexes { get; [param: CanBeNull] set; }
    }
}
