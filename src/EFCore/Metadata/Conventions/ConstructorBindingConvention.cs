// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

#nullable enable

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions
{
    /// <summary>
    ///     A convention that binds entity type constructor parameters to existing properties and service properties based on their names:
    ///     * [parameter name]
    ///     * [pascal-cased parameter name]
    ///     * _[parameter name]
    ///     * _[pascal-cased parameter name]
    ///     * m_[parameter name]
    ///     * m_[pascal-cased parameter name]
    /// </summary>
    public class ConstructorBindingConvention : IModelFinalizingConvention
    {
        /// <summary>
        ///     Creates a new instance of <see cref="ConstructorBindingConvention" />.
        /// </summary>
        /// <param name="dependencies"> Parameter object containing dependencies for this convention. </param>
        public ConstructorBindingConvention([NotNull] ProviderConventionSetBuilderDependencies dependencies)
        {
            Dependencies = dependencies;
        }

        /// <summary>
        ///     Parameter object containing service dependencies.
        /// </summary>
        protected virtual ProviderConventionSetBuilderDependencies Dependencies { get; }

        /// <inheritdoc />
        public virtual void ProcessModelFinalizing(
            IConventionModelBuilder modelBuilder,
            IConventionContext<IConventionModelBuilder> context)
        {
            foreach (EntityType entityType in modelBuilder.Metadata.GetEntityTypes())
            {
                if (!entityType.ClrType.IsAbstract
                    && ConfigurationSource.Convention.Overrides(entityType.GetConstructorBindingConfigurationSource()))
                {
                    Dependencies.ConstructorBindingFactory.GetBindings(
                        (IMutableEntityType)entityType, out var constructorBinding, out var serviceOnlyBinding);

                    entityType.Builder.HasConstructorBinding(constructorBinding, ConfigurationSource.Convention);
                    entityType.Builder.HasServiceOnlyConstructorBinding(serviceOnlyBinding, ConfigurationSource.Convention);
                }
            }
        }
    }
}
