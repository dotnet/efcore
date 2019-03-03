// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Metadata.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class DbFunctionParameter : IMutableDbFunctionParameter, IConventionDbFunctionParameter
    {
        private readonly IMutableDbFunction _parent;
        private readonly string _name;
        private readonly Type _clrType;
        private bool _supportsNullPropagation;
        private string _storeType;
        private RelationalTypeMapping _typeMapping;

        private ConfigurationSource? _supportsNullPropagationConfigurationSource;
        private ConfigurationSource? _storeTypeConfigurationSource;
        private ConfigurationSource? _typeMappingConfigurationSource;


        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public DbFunctionParameter([NotNull] IMutableDbFunction parent, [NotNull] string name, [NotNull] Type clrType)
        {
            Check.NotEmpty(name, nameof(name));
            Check.NotNull(clrType, nameof(clrType));
            Check.NotNull(parent, nameof(parent));

            _name = name;
            _parent = parent;
            _clrType = clrType;
        }

        public virtual IConventionDbFunctionParameterBuilder Builder => new DbFunctionParameterBuilder(this);

        public virtual bool SupportsNullPropagation
        {
            get => _supportsNullPropagation;
            set => SetSupportsNullPropagation(value, ConfigurationSource.Explicit);
        }

        public virtual void SetSupportsNullPropagation(bool supportsNullPropagation, ConfigurationSource configurationSource)
        {
            _supportsNullPropagation = supportsNullPropagation;

            UpdateSupportsNullPropagationConfigurationSource(configurationSource);
        }

        private void UpdateSupportsNullPropagationConfigurationSource(ConfigurationSource configurationSource)
            => _supportsNullPropagationConfigurationSource = configurationSource.Max(_supportsNullPropagationConfigurationSource);

        public virtual ConfigurationSource? GetSupportsNullPropagationConfigurationSource() => _supportsNullPropagationConfigurationSource;

        public virtual string StoreType
        {
            get => _storeType;
            set => SetStoreType(value, ConfigurationSource.Explicit);
        }

        public virtual void SetStoreType([CanBeNull] string storeType, ConfigurationSource configurationSource)
        {
            _storeType = storeType;

            UpdateStoreTypeConfigurationSource(configurationSource);
        }

        private void UpdateStoreTypeConfigurationSource(ConfigurationSource configurationSource)
            => _storeTypeConfigurationSource = configurationSource.Max(_storeTypeConfigurationSource);

        public virtual ConfigurationSource? GetStoreTypeConfigurationSource() => _storeTypeConfigurationSource;

        public virtual RelationalTypeMapping TypeMapping
        {
            get => _typeMapping;
            set => SetTypeMapping(value, ConfigurationSource.Explicit);
        }

        public virtual void SetTypeMapping(RelationalTypeMapping typeMapping, ConfigurationSource configurationSource)
        {
            _typeMapping = typeMapping;

            UpdateTypeMappingConfigurationSource(configurationSource);
        }

        private void UpdateTypeMappingConfigurationSource(ConfigurationSource configurationSource)
            => _typeMappingConfigurationSource = configurationSource.Max(_typeMappingConfigurationSource);

        public virtual ConfigurationSource? GetTypeMappingConfigurationSource() => _typeMappingConfigurationSource;

        IConventionDbFunction IConventionDbFunctionParameter.Parent => (IConventionDbFunction)_parent;

        IDbFunction IDbFunctionParameter.Parent => _parent;

        IMutableDbFunction IMutableDbFunctionParameter.Parent => _parent;

        string IDbFunctionParameter.Name => _name;

        Type IDbFunctionParameter.ClrType => _clrType;

        void IConventionDbFunctionParameter.SetStoreType(string storeType, bool fromDataAnnotation)
            => SetStoreType(storeType, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        void IConventionDbFunctionParameter.SetSupportsNullPropagation(bool supportsNullPropagation, bool fromDataAnnotation)
            => SetSupportsNullPropagation(supportsNullPropagation,
                fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        void IConventionDbFunctionParameter.SetTypeMapping(RelationalTypeMapping typeMapping, bool fromDataAnnotation)
            => SetTypeMapping(typeMapping, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);
    }
}
