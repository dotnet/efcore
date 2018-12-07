using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Cosmos.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Cosmos
{
    /// <summary>
    ///     Cosmos-specific extension methods for <see cref="PropertyBuilder" />.
    /// </summary>
    public static class CosmosPropertyBuilderExtensions
    {
        /// <summary>
        ///     Configures the property name that the property is mapped to when targeting Azure Cosmos.
        /// </summary>
        /// <remarks> If an empty string is supplied then the property will not be persisted. </remarks>
        /// <param name="propertyBuilder"> The builder for the property being configured. </param>
        /// <param name="name"> The name of the container. </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public static PropertyBuilder ToProperty(
            [NotNull] this PropertyBuilder propertyBuilder,
            [CanBeNull] string name)
            => propertyBuilder.GetInfrastructure<InternalPropertyBuilder>()
                .Cosmos(ConfigurationSource.Explicit)
                .ToProperty(name)
                ? propertyBuilder
                : propertyBuilder;

        /// <summary>
        ///     Configures the property name that the property is mapped to when targeting Azure Cosmos.
        /// </summary>
        /// <remarks> If an empty string is supplied then the property will not be persisted. </remarks>
        /// <typeparam name="TProperty"> The type of the property being configured. </typeparam>
        /// <param name="propertyBuilder"> The builder for the property being configured. </param>
        /// <param name="name"> The name of the container. </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public static PropertyBuilder<TProperty> ToProperty<TProperty>(
            [NotNull] this PropertyBuilder<TProperty> propertyBuilder,
            [CanBeNull] string name)
            => (PropertyBuilder<TProperty>)ToProperty((PropertyBuilder)propertyBuilder, name);
    }
}
