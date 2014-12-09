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
        private ConfigurationSource? _generateValueOnAddConfigurationSource;
        private ConfigurationSource? _isStoreComputedConfigurationSource;
        private ConfigurationSource? _useStoreDefaultConfigurationSource;

        public InternalPropertyBuilder([NotNull] Property property, [NotNull] InternalModelBuilder modelBuilder, ConfigurationSource configurationSource)
            : base(property, modelBuilder)
        {
            _isShadowPropertyConfigurationSource = configurationSource;
        }

        public virtual bool Required(bool isRequired, ConfigurationSource configurationSource)
        {
            if (CanSetRequired(isRequired, configurationSource))
            {
                if (_isRequiredConfigurationSource == null
                    && Metadata.IsNullable != null)
                {
                    _isRequiredConfigurationSource = ConfigurationSource.Explicit;
                }
                else
                {
                    _isRequiredConfigurationSource = configurationSource.Max(_isRequiredConfigurationSource);
                }

                Metadata.IsNullable = !isRequired;
                return true;
            }

            return false;
        }

        public virtual bool CanSetRequired(bool isRequired, ConfigurationSource configurationSource)
        {
            if (configurationSource.CanSet(_isRequiredConfigurationSource, Metadata.IsNullable.HasValue)
                || Metadata.IsNullable.Value == !isRequired)
            {
                return true;
            }

            return false;
        }

        public virtual bool MaxLength(int maxLength, ConfigurationSource configurationSource)
        {
            if (configurationSource.CanSet(_maxLengthConfigurationSource, Metadata.MaxLength.HasValue)
                || Metadata.MaxLength.Value == maxLength)
            {
                if (_maxLengthConfigurationSource == null
                    && Metadata.MaxLength != null)
                {
                    _maxLengthConfigurationSource = ConfigurationSource.Explicit;
                }
                else
                {
                    _maxLengthConfigurationSource = configurationSource.Max(_maxLengthConfigurationSource);
                }

                Metadata.MaxLength = maxLength;
                return true;
            }

            return false;
        }

        public virtual bool ConcurrencyToken(bool isConcurrencyToken, ConfigurationSource configurationSource)
        {
            if (configurationSource.CanSet(_isConcurrencyTokenConfigurationSource, Metadata.IsConcurrencyToken.HasValue)
                || Metadata.IsConcurrencyToken.Value == isConcurrencyToken)
            {
                if (_isConcurrencyTokenConfigurationSource == null
                    && Metadata.IsConcurrencyToken != null)
                {
                    _isConcurrencyTokenConfigurationSource = ConfigurationSource.Explicit;
                }
                else
                {
                    _isConcurrencyTokenConfigurationSource = configurationSource.Max(_isConcurrencyTokenConfigurationSource);
                }

                Metadata.IsConcurrencyToken = isConcurrencyToken;
                return true;
            }

            return false;
        }

        public virtual bool Shadow(bool isShadowProperty, ConfigurationSource configurationSource)
        {
            if (configurationSource.CanSet(_isShadowPropertyConfigurationSource, true)
                || Metadata.IsShadowProperty == isShadowProperty)
            {
                _isShadowPropertyConfigurationSource = configurationSource.Max(_isShadowPropertyConfigurationSource);

                Metadata.IsShadowProperty = isShadowProperty;
                return true;
            }

            return false;
        }

        public virtual bool GenerateValueOnAdd(bool generateValue, ConfigurationSource configurationSource)
        {
            if (configurationSource.CanSet(_generateValueOnAddConfigurationSource, Metadata.GenerateValueOnAdd.HasValue)
                || Metadata.GenerateValueOnAdd.Value == generateValue)
            {
                if (_generateValueOnAddConfigurationSource == null
                    && Metadata.GenerateValueOnAdd != null)
                {
                    _generateValueOnAddConfigurationSource = ConfigurationSource.Explicit;
                }
                else
                {
                    _generateValueOnAddConfigurationSource = configurationSource.Max(_generateValueOnAddConfigurationSource);
                }

                Metadata.GenerateValueOnAdd = generateValue;
                return true;
            }

            return false;
        }

        public virtual bool StoreComputed(bool storeComputed, ConfigurationSource configurationSource)
        {
            if (configurationSource.CanSet(_isStoreComputedConfigurationSource, Metadata.IsStoreComputed.HasValue)
                || Metadata.IsStoreComputed == storeComputed)
            {
                if (_isStoreComputedConfigurationSource == null
                    && Metadata.IsStoreComputed != null)
                {
                    _isStoreComputedConfigurationSource = ConfigurationSource.Explicit;
                }
                else
                {
                    _isStoreComputedConfigurationSource = configurationSource.Max(_isStoreComputedConfigurationSource);
                }

                Metadata.IsStoreComputed = storeComputed;
                return true;
            }

            return false;
        }

        public virtual bool UseStoreDefault(bool useDefault, ConfigurationSource configurationSource)
        {
            if (configurationSource.CanSet(_useStoreDefaultConfigurationSource, Metadata.UseStoreDefault.HasValue)
                || Metadata.UseStoreDefault.Value == useDefault)
            {
                if (_useStoreDefaultConfigurationSource == null
                    && Metadata.UseStoreDefault != null)
                {
                    _useStoreDefaultConfigurationSource = ConfigurationSource.Explicit;
                }
                else
                {
                    _useStoreDefaultConfigurationSource = configurationSource.Max(_useStoreDefaultConfigurationSource);
                }

                Metadata.UseStoreDefault = useDefault;
                return true;
            }

            return false;
        }
    }
}
