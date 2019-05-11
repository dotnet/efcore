// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public class DiscriminatorConvention : IBaseTypeChangedConvention, IEntityTypeRemovedConvention
    {
        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public DiscriminatorConvention([NotNull] IDiagnosticsLogger<DbLoggerCategory.Model> logger)
        {
            Logger = logger;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected virtual IDiagnosticsLogger<DbLoggerCategory.Model> Logger { get; }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual bool Apply(InternalEntityTypeBuilder entityTypeBuilder, EntityType oldBaseType)
        {
            if (oldBaseType != null
                && oldBaseType.BaseType == null
                && oldBaseType.GetDirectlyDerivedTypes().Count == 0)
            {
                ((IConventionEntityTypeBuilder)oldBaseType.Builder)?.HasNoDeclaredDiscriminator();
            }

            IConventionEntityTypeBuilder conventionEntityTypeBuilder = entityTypeBuilder;
            var entityType = entityTypeBuilder.Metadata;
            var derivedEntityTypes = entityType.GetDerivedTypes().ToList();

            IConventionDiscriminatorBuilder discriminator;
            if (entityType.BaseType == null)
            {
                if (derivedEntityTypes.Count == 0)
                {
                    conventionEntityTypeBuilder.HasNoDeclaredDiscriminator();
                    return true;
                }

                discriminator = conventionEntityTypeBuilder.HasDiscriminator(typeof(string));
            }
            else
            {
                if (conventionEntityTypeBuilder.HasNoDeclaredDiscriminator() == null)
                {
                    return true;
                }

                IConventionEntityTypeBuilder rootTypeBuilder = entityType.RootType().Builder;
                discriminator = rootTypeBuilder?.HasDiscriminator(typeof(string));

                if (entityType.BaseType.BaseType == null)
                {
                    discriminator?.HasValue(entityType.BaseType, entityType.BaseType.ShortName());
                }
            }

            if (discriminator != null)
            {
                discriminator.HasValue(entityTypeBuilder.Metadata, entityTypeBuilder.Metadata.ShortName());
                SetDefaultDiscriminatorValues(derivedEntityTypes, discriminator);
            }

            return true;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual bool Apply(InternalModelBuilder modelBuilder, EntityType type)
        {
            var oldBaseType = type.BaseType;
            if (oldBaseType != null
                && oldBaseType.BaseType == null
                && oldBaseType.GetDirectlyDerivedTypes().Count == 0)
            {
                ((IConventionEntityTypeBuilder)oldBaseType.Builder)?.HasNoDeclaredDiscriminator();
            }

            return true;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected virtual void SetDefaultDiscriminatorValues(
            IEnumerable<EntityType> entityTypes, IConventionDiscriminatorBuilder discriminator)
        {
            foreach (var entityType in entityTypes)
            {
                discriminator.HasValue(entityType, entityType.ShortName());
            }
        }
    }
}
