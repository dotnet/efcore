// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class ServicePropertyDiscoveryConvention : IEntityTypeAddedConvention, IBaseTypeChangedConvention
    {
        private readonly ICoreTypeMapper _typeMapper;
        private readonly IParameterBindingFactories _parameterBindingFactories;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public ServicePropertyDiscoveryConvention(
            [NotNull] ICoreTypeMapper typeMapper,
            [NotNull] IParameterBindingFactories parameterBindingFactories)
        {
            Check.NotNull(typeMapper, nameof(typeMapper));
            Check.NotNull(parameterBindingFactories, nameof(parameterBindingFactories));

            _typeMapper = typeMapper;
            _parameterBindingFactories = parameterBindingFactories;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual InternalEntityTypeBuilder Apply(InternalEntityTypeBuilder entityTypeBuilder)
        {
            Check.NotNull(entityTypeBuilder, nameof(entityTypeBuilder));
            var entityType = entityTypeBuilder.Metadata;

            if (entityType.HasClrType())
            {
                var candidates = entityType.ClrType.GetRuntimeProperties();

                foreach (var propertyInfo in candidates)
                {
                    if (!(propertyInfo.IsCandidateProperty()
                          && _typeMapper.FindMapping(propertyInfo) != null)
                        && propertyInfo.IsCandidateProperty(publicOnly: false))
                    {
                        var factory = _parameterBindingFactories.FindFactory(propertyInfo.PropertyType, propertyInfo.Name);

                        if (factory != null)
                        {
                            var serviceProperty = entityType.FindServiceProperty(propertyInfo.Name);
                            if (serviceProperty == null
                                || serviceProperty.PropertyInfo != propertyInfo)
                            {
                                serviceProperty = entityType.AddServiceProperty(propertyInfo, ConfigurationSource.Convention);
                            }

                            serviceProperty.SetParameterBinding(
                                (ServiceParameterBinding)factory.Bind(entityType, propertyInfo.PropertyType, propertyInfo.Name));
                        }
                    }
                }
            }

            return entityTypeBuilder;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual bool Apply(InternalEntityTypeBuilder entityTypeBuilder, EntityType oldBaseType)
            => Apply(entityTypeBuilder) != null;
    }
}
