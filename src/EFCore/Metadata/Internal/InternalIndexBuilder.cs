// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Microsoft.EntityFrameworkCore.Metadata.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public class InternalIndexBuilder : AnnotatableBuilder<Index, InternalModelBuilder>, IConventionIndexBuilder
    {
        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public InternalIndexBuilder(Index index, InternalModelBuilder modelBuilder)
            : base(index, modelBuilder)
        {
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual InternalIndexBuilder? IsUnique(bool? unique, ConfigurationSource configurationSource)
        {
            if (!CanSetIsUnique(unique, configurationSource))
            {
                return null;
            }

            Metadata.SetIsUnique(unique, configurationSource);
            return this;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual bool CanSetIsUnique(bool? unique, ConfigurationSource? configurationSource)
            => Metadata.IsUnique == unique
                || configurationSource.Overrides(Metadata.GetIsUniqueConfigurationSource());

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual InternalIndexBuilder? Attach(InternalEntityTypeBuilder entityTypeBuilder)
        {
            var properties = entityTypeBuilder.GetActualProperties(Metadata.Properties, null);
            if (properties == null)
            {
                return null;
            }

            var newIndexBuilder = Metadata.Name == null
                ? entityTypeBuilder.HasIndex(properties, Metadata.GetConfigurationSource())
                : entityTypeBuilder.HasIndex(properties, Metadata.Name, Metadata.GetConfigurationSource());
            newIndexBuilder?.MergeAnnotationsFrom(Metadata);

            var isUniqueConfigurationSource = Metadata.GetIsUniqueConfigurationSource();
            if (isUniqueConfigurationSource.HasValue)
            {
                newIndexBuilder?.IsUnique(Metadata.IsUnique, isUniqueConfigurationSource.Value);
            }

            return newIndexBuilder;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        IConventionIndex IConventionIndexBuilder.Metadata
            => Metadata;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        IConventionIndexBuilder? IConventionIndexBuilder.IsUnique(bool? unique, bool fromDataAnnotation)
            => IsUnique(
                unique,
                fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        bool IConventionIndexBuilder.CanSetIsUnique(bool? unique, bool fromDataAnnotation)
            => CanSetIsUnique(
                unique,
                fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);
    }
}
