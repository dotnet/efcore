// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using JetBrains.Annotations;
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
    public class InternalKeyBuilder : AnnotatableBuilder<Key, InternalModelBuilder>, IConventionKeyBuilder
    {
        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public InternalKeyBuilder([NotNull] Key key, [NotNull] InternalModelBuilder modelBuilder)
            : base(key, modelBuilder)
        {
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual InternalKeyBuilder Attach(
            [NotNull] InternalEntityTypeBuilder entityTypeBuilder,
            ConfigurationSource? primaryKeyConfigurationSource)
        {
            var propertyNames = Metadata.Properties.Select(p => p.Name).ToList();
            foreach (var propertyName in propertyNames)
            {
                if (entityTypeBuilder.Metadata.FindProperty(propertyName) == null)
                {
                    return null;
                }
            }

            var newKeyBuilder = entityTypeBuilder.HasKey(propertyNames, Metadata.GetConfigurationSource());

            newKeyBuilder?.MergeAnnotationsFrom(Metadata);

            if (primaryKeyConfigurationSource.HasValue
                && newKeyBuilder != null)
            {
                var currentPrimaryKeyConfigurationSource = entityTypeBuilder.Metadata.GetPrimaryKeyConfigurationSource();
                if (currentPrimaryKeyConfigurationSource?.Overrides(primaryKeyConfigurationSource.Value) != true)
                {
                    entityTypeBuilder.PrimaryKey(newKeyBuilder.Metadata.Properties, primaryKeyConfigurationSource.Value);
                }
            }

            return newKeyBuilder;
        }

        IConventionKey IConventionKeyBuilder.Metadata
            => Metadata;
    }
}
