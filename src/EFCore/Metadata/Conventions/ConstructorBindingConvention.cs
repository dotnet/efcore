// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

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
        public ConstructorBindingConvention(ProviderConventionSetBuilderDependencies dependencies)
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
