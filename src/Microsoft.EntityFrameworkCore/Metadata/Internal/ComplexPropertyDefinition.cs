// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Metadata.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class ComplexPropertyDefinition : PropertyBase, IMutableComplexPropertyDefinition
    {
        private ConfigurationSource _configurationSource;
        private bool? _nullable;
        private bool? _readOnlyBeforeSave;
        private bool? _readOnlyAfterSave;
        private bool? _storeGeneratedAlways;
        private bool? _requiresValueGenerator;
        private bool? _concurrencyToken;
        private ValueGenerated? _valueGenerated;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public ComplexPropertyDefinition(
            [NotNull] string name,
            [NotNull] Type clrType,
            [CanBeNull] PropertyInfo propertyInfo,
            [CanBeNull] FieldInfo fieldInfo,
            [NotNull] ComplexTypeDefinition declaringType,
            ConfigurationSource configurationSource)
            : base(name, propertyInfo, fieldInfo)
        {
            Check.NotNull(clrType, nameof(clrType));
            Check.NotNull(declaringType, nameof(declaringType));

            DeclaringType = declaringType;
            ClrType = clrType;
            _configurationSource = configurationSource;

            // TODO: ComplexType builders
            //Builder = new InternalPropertyBuilder(this, declaringEntityType.Model.Builder);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public new virtual ComplexTypeDefinition DeclaringType { get; }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        // TODO: ComplexType notification that definition has changed
        protected override void PropertyMetadataChanged() => DeclaringType.PropertyMetadataChanged();

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual bool? IsNullableDefault
        {
            get { return _nullable; }
            set { SetIsNullableDefault(value, ConfigurationSource.Explicit); }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual void SetIsNullableDefault(bool? nullable, ConfigurationSource configurationSource)
        {
            _nullable = nullable;
            IsNullableConfigurationSource = configurationSource.Max(IsNullableConfigurationSource);
            PropertyMetadataChanged();
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual ConfigurationSource? IsNullableConfigurationSource { get; private set; }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual bool? IsReadOnlyBeforeSaveDefault
        {
            get { return _readOnlyBeforeSave; }
            set { SetIsReadOnlyBeforeSaveDefault(value, ConfigurationSource.Explicit); }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual void SetIsReadOnlyBeforeSaveDefault(bool? readOnlyBeforeSave, ConfigurationSource configurationSource)
        {
            _readOnlyBeforeSave = readOnlyBeforeSave;
            IsReadOnlyBeforeSaveConfigurationSource = configurationSource.Max(IsReadOnlyBeforeSaveConfigurationSource);
            PropertyMetadataChanged();
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual ConfigurationSource? IsReadOnlyBeforeSaveConfigurationSource { get; private set; }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual bool? IsReadOnlyAfterSaveDefault
        {
            get { return _readOnlyAfterSave; }
            set { SetIsReadOnlyAfterSaveDefault(value, ConfigurationSource.Explicit); }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual void SetIsReadOnlyAfterSaveDefault(bool? readOnlyAfterSave, ConfigurationSource configurationSource)
        {
            _readOnlyAfterSave = readOnlyAfterSave;
            IsReadOnlyAfterSaveConfigurationSource = configurationSource.Max(IsReadOnlyAfterSaveConfigurationSource);
            PropertyMetadataChanged();
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual ConfigurationSource? IsReadOnlyAfterSaveConfigurationSource { get; private set; }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual bool? IsStoreGeneratedAlwaysDefault
        {
            get { return _storeGeneratedAlways; }
            set { SetIsStoreGeneratedAlwaysDefault(value, ConfigurationSource.Explicit); }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual void SetIsStoreGeneratedAlwaysDefault(bool? storeGeneratedAlways, ConfigurationSource configurationSource)
        {
            _storeGeneratedAlways = storeGeneratedAlways;
            IsStoreGeneratedAlwaysConfigurationSource = configurationSource.Max(IsStoreGeneratedAlwaysConfigurationSource);
            PropertyMetadataChanged();
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual ConfigurationSource? IsStoreGeneratedAlwaysConfigurationSource { get; private set; }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual bool? RequiresValueGeneratorDefault
        {
            get { return _requiresValueGenerator; }
            set { SetRequiresValueGeneratorDefault(value, ConfigurationSource.Explicit); }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual void SetRequiresValueGeneratorDefault(bool? requiresValueGenerator, ConfigurationSource configurationSource)
        {
            _requiresValueGenerator = requiresValueGenerator;
            RequiresValueGeneratorConfigurationSource = configurationSource.Max(RequiresValueGeneratorConfigurationSource);
            PropertyMetadataChanged();
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual ConfigurationSource? RequiresValueGeneratorConfigurationSource { get; private set; }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual bool? IsConcurrencyTokenDefault
        {
            get { return _concurrencyToken; }
            set { SetIsConcurrencyTokenDefault(value, ConfigurationSource.Explicit); }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual void SetIsConcurrencyTokenDefault(bool? concurrencyToken, ConfigurationSource configurationSource)
        {
            _concurrencyToken = concurrencyToken;
            IsConcurrencyTokenConfigurationSource = configurationSource.Max(IsConcurrencyTokenConfigurationSource);
            PropertyMetadataChanged();
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual ConfigurationSource? IsConcurrencyTokenConfigurationSource { get; private set; }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual ValueGenerated? ValueGeneratedDefault
        {
            get { return _valueGenerated; }
            set { SetValueGeneratedDefault(value, ConfigurationSource.Explicit); }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual void SetValueGeneratedDefault(ValueGenerated? valueGenerated, ConfigurationSource configurationSource)
        {
            _valueGenerated = valueGenerated;
            ValueGeneratedConfigurationSource = configurationSource.Max(ValueGeneratedConfigurationSource);
            PropertyMetadataChanged();
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual ConfigurationSource? ValueGeneratedConfigurationSource { get; private set; }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual bool IsShadowProperty => MemberInfo == null;

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

        // TODO: ComplexType builders
        //protected override void OnFieldInfoSet(FieldInfo oldFieldInfo)
        //    => DeclaringType.Model.ConventionDispatcher.OnPropertyFieldChanged(Builder, oldFieldInfo);

        ITypeBase IPropertyBase.DeclaringType => DeclaringType;
        IComplexTypeDefinition IComplexPropertyDefinition.DeclaringType => DeclaringType;
        IMutableComplexTypeDefinition IMutableComplexPropertyDefinition.DeclaringType => DeclaringType;
        IMutableTypeBase IMutablePropertyBase.DeclaringType => DeclaringType;

        // TODO: ComplexType debug strings public override string ToString() => this.ToDebugString();

        // TODO: ComplexType debug strings public virtual DebugView<Property> DebugView => new DebugView<Property>(this, m => m.ToDebugString(false));
    }
}
