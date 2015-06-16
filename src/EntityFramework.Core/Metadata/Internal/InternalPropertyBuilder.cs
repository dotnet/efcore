// Copyright (c) .NET Foundation. All rights reserved.
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
        private ConfigurationSource? _isValueGeneratedOnAddConfigurationSource;
        private ConfigurationSource? _storeGeneratedPatternConfigurationSource;

        public InternalPropertyBuilder(
            [NotNull] Property property,
            [NotNull] InternalModelBuilder modelBuilder,
            ConfigurationSource configurationSource)
            : base(property, modelBuilder)
        {
            _isShadowPropertyConfigurationSource = configurationSource;
        }

        public virtual bool Required(bool? isRequired, ConfigurationSource configurationSource)
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

        public virtual bool CanSetRequired(bool? isRequired, ConfigurationSource configurationSource)
            => configurationSource.CanSet(_isRequiredConfigurationSource, Metadata.IsNullable.HasValue)
               || Metadata.IsNullable.Value == !isRequired;

        public virtual bool MaxLength(int? maxLength, ConfigurationSource configurationSource)
        {
            if (configurationSource.CanSet(_maxLengthConfigurationSource, Metadata.GetMaxLength().HasValue)
                || Metadata.GetMaxLength().Value == maxLength)
            {
                if (_maxLengthConfigurationSource == null
                    && Metadata.GetMaxLength() != null)
                {
                    _maxLengthConfigurationSource = ConfigurationSource.Explicit;
                }
                else
                {
                    _maxLengthConfigurationSource = configurationSource.Max(_maxLengthConfigurationSource);
                }

                Metadata.SetMaxLength(maxLength);
                return true;
            }

            return false;
        }

        public virtual bool ConcurrencyToken(bool? isConcurrencyToken, ConfigurationSource configurationSource)
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

        public virtual bool GenerateValueOnAdd(bool? generateValue, ConfigurationSource configurationSource)
        {
            if (configurationSource.CanSet(_isValueGeneratedOnAddConfigurationSource, Metadata.IsValueGeneratedOnAdd.HasValue)
                || Metadata.IsValueGeneratedOnAdd.Value == generateValue)
            {
                if (_isValueGeneratedOnAddConfigurationSource == null
                    && Metadata.IsValueGeneratedOnAdd != null)
                {
                    _isValueGeneratedOnAddConfigurationSource = ConfigurationSource.Explicit;
                }
                else
                {
                    _isValueGeneratedOnAddConfigurationSource = configurationSource.Max(_isValueGeneratedOnAddConfigurationSource);
                }

                Metadata.IsValueGeneratedOnAdd = generateValue;
                return true;
            }

            return false;
        }

        public virtual bool StoreGeneratedPattern(StoreGeneratedPattern? storeGeneratedPattern, ConfigurationSource configurationSource)
        {
            if (configurationSource.CanSet(_storeGeneratedPatternConfigurationSource, Metadata.StoreGeneratedPattern.HasValue)
                || Metadata.StoreGeneratedPattern == storeGeneratedPattern)
            {
                if (_storeGeneratedPatternConfigurationSource == null
                    && Metadata.StoreGeneratedPattern != null)
                {
                    _storeGeneratedPatternConfigurationSource = ConfigurationSource.Explicit;
                }
                else
                {
                    _storeGeneratedPatternConfigurationSource = configurationSource.Max(_storeGeneratedPatternConfigurationSource);
                }

                Metadata.StoreGeneratedPattern = storeGeneratedPattern;
                return true;
            }

            return false;
        }
    }
}
