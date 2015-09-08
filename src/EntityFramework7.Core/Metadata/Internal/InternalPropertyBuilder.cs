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
        private ConfigurationSource? _requiresValueGeneratorConfigurationSource;
        private ConfigurationSource? _valueGeneratedConfigurationSource;

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
               || ((IProperty)Metadata).IsNullable == !isRequired;

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

        public virtual bool UseValueGenerator(bool? generateValue, ConfigurationSource configurationSource)
        {
            if (configurationSource.CanSet(_requiresValueGeneratorConfigurationSource, Metadata.RequiresValueGenerator.HasValue)
                || Metadata.RequiresValueGenerator.Value == generateValue)
            {
                if (_requiresValueGeneratorConfigurationSource == null
                    && Metadata.RequiresValueGenerator != null)
                {
                    _requiresValueGeneratorConfigurationSource = ConfigurationSource.Explicit;
                }
                else
                {
                    _requiresValueGeneratorConfigurationSource = configurationSource.Max(_requiresValueGeneratorConfigurationSource);
                }

                Metadata.RequiresValueGenerator = generateValue;
                return true;
            }

            return false;
        }

        public virtual bool ValueGenerated(ValueGenerated? valueGenerated, ConfigurationSource configurationSource)
        {
            if (configurationSource.CanSet(_valueGeneratedConfigurationSource, Metadata.ValueGenerated.HasValue)
                || Metadata.ValueGenerated == valueGenerated)
            {
                if (_valueGeneratedConfigurationSource == null
                    && Metadata.ValueGenerated != null)
                {
                    _valueGeneratedConfigurationSource = ConfigurationSource.Explicit;
                }
                else
                {
                    _valueGeneratedConfigurationSource = configurationSource.Max(_valueGeneratedConfigurationSource);
                }

                Metadata.ValueGenerated = valueGenerated;
                return true;
            }

            return false;
        }
    }
}
