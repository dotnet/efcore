// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Cosmos.Metadata.Conventions.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public class CosmosDiscriminatorConvention : DiscriminatorConvention, IEntityTypeAddedConvention
    {
        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public CosmosDiscriminatorConvention([NotNull] IDiagnosticsLogger<DbLoggerCategory.Model> logger)
            : base(logger)
        {
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public InternalEntityTypeBuilder Apply(InternalEntityTypeBuilder entityTypeBuilder)
        {
            var entityType = entityTypeBuilder.Metadata;
            if (entityTypeBuilder.Metadata.BaseType == null
                && !entityTypeBuilder.Metadata.GetDerivedTypes().Any())
            {
                ((IConventionEntityTypeBuilder)entityTypeBuilder).HasDiscriminator(typeof(string))
                    .HasValue(entityType, entityType.ShortName());
            }

            return entityTypeBuilder;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public override bool Apply(InternalEntityTypeBuilder entityTypeBuilder, EntityType oldBaseType)
        {
            IConventionEntityTypeBuilder conventionEntityTypeBuilder = entityTypeBuilder;
            IConventionDiscriminatorBuilder discriminator;
            var entityType = entityTypeBuilder.Metadata;
            if (entityType.BaseType == null)
            {
                discriminator = conventionEntityTypeBuilder.HasDiscriminator(typeof(string));
            }
            else
            {
                discriminator = ((IConventionEntityTypeBuilder)entityType.BaseType.Builder)?.HasDiscriminator(typeof(string));

                if (entityType.BaseType.BaseType == null)
                {
                    discriminator?.HasValue(entityType.BaseType, entityType.BaseType.ShortName());
                }
            }

            if (discriminator != null)
            {
                discriminator.HasValue(entityTypeBuilder.Metadata, entityTypeBuilder.Metadata.ShortName());
                SetDefaultDiscriminatorValues(entityType.GetDerivedTypes(), discriminator);
            }

            return true;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public override bool Apply(InternalModelBuilder modelBuilder, EntityType type)
        {
            return true;
        }
    }
}
