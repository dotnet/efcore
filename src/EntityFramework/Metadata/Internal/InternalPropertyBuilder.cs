// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;

namespace Microsoft.Data.Entity.Metadata.Internal
{
    public class InternalPropertyBuilder : InternalMetadataItemBuilder<Property>
    {
        private ConfigurationSource? _isRequiredConfigurationSource;
        private ConfigurationSource? _maxLengthConfigurationSource;
        private ConfigurationSource? _isConcurrencyTokenConfigurationSource;
        private ConfigurationSource _isShadowPropertyConfigurationSource;
        private ConfigurationSource? _valueGenerationConfigurationSource;
        private ConfigurationSource? _useStoreDefaultConfigurationSource;

        public InternalPropertyBuilder([NotNull] Property property, [NotNull] InternalModelBuilder modelBuilder, ConfigurationSource configurationSource)
            : base(property, modelBuilder)
        {
            _isShadowPropertyConfigurationSource = configurationSource;
        }

        public virtual bool Required(bool isRequired, ConfigurationSource configurationSource)
        {
            if (configurationSource.CanSet(_isRequiredConfigurationSource, Metadata.IsNullable.HasValue))
            {
                _isRequiredConfigurationSource = configurationSource;
                Metadata.IsNullable = !isRequired;
                return true;
            }

            return false;
        }

        public virtual bool MaxLength(int maxLength, ConfigurationSource configurationSource)
        {
            if (configurationSource.CanSet(_maxLengthConfigurationSource, Metadata.MaxLength.HasValue))
            {
                _maxLengthConfigurationSource = configurationSource;
                Metadata.MaxLength = maxLength;
                return true;
            }

            return false;
        }

        public virtual bool ConcurrencyToken(bool isConcurrencyToken, ConfigurationSource configurationSource)
        {
            if (configurationSource.CanSet(_isConcurrencyTokenConfigurationSource, Metadata.IsConcurrencyToken.HasValue))
            {
                _isConcurrencyTokenConfigurationSource = configurationSource;
                Metadata.IsConcurrencyToken = isConcurrencyToken;
                return true;
            }

            return false;
        }

        public virtual bool Shadow(bool isShadowProperty, ConfigurationSource configurationSource)
        {
            if (configurationSource.CanSet(_isShadowPropertyConfigurationSource, true))
            {
                _isShadowPropertyConfigurationSource = configurationSource;
                Metadata.IsShadowProperty = isShadowProperty;
                return true;
            }

            return false;
        }

        public virtual bool GenerateValuesOnAdd(bool generateValues, ConfigurationSource configurationSource)
        {
            if (configurationSource.CanSet(_valueGenerationConfigurationSource, Metadata.ValueGeneration.HasValue))
            {
                _valueGenerationConfigurationSource = configurationSource;
                Metadata.ValueGeneration = generateValues ? ValueGeneration.OnAdd : ValueGeneration.None;
                return true;
            }

            return false;
        }

        public virtual bool StoreComputed(bool storeComputed, ConfigurationSource configurationSource)
        {
            if (configurationSource.CanSet(_valueGenerationConfigurationSource, Metadata.ValueGeneration.HasValue))
            {
                _valueGenerationConfigurationSource = configurationSource;
                Metadata.ValueGeneration = storeComputed ? ValueGeneration.OnAddAndUpdate : ValueGeneration.None;
                return true;
            }

            return false;
        }

        public virtual bool UseStoreDefault(bool useDefault, ConfigurationSource configurationSource)
        {
            if (configurationSource.CanSet(_useStoreDefaultConfigurationSource, Metadata.UseStoreDefault.HasValue))
            {
                _useStoreDefaultConfigurationSource = configurationSource;
                Metadata.UseStoreDefault = useDefault;
                return true;
            }

            return false;
        }
    }
}
