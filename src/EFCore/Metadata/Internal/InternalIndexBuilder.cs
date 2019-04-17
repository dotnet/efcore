// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics;
using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Metadata.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerDisplay("{Metadata,nq}")]
    // Issue#11266 This type is being used by provider code. Do not break.
    public class InternalIndexBuilder : InternalMetadataItemBuilder<Index>
    {
        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public InternalIndexBuilder([NotNull] Index index, [NotNull] InternalModelBuilder modelBuilder)
            : base(index, modelBuilder)
        {
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual bool IsUnique(bool isUnique, ConfigurationSource configurationSource)
        {
            if (configurationSource.Overrides(Metadata.GetIsUniqueConfigurationSource())
                || (Metadata.IsUnique == isUnique))
            {
                Metadata.SetIsUnique(isUnique, configurationSource);
                return true;
            }

            return false;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual InternalIndexBuilder Attach([NotNull] InternalEntityTypeBuilder entityTypeBuilder)
        {
            var properties = entityTypeBuilder.GetActualProperties(Metadata.Properties, null);
            if (properties == null)
            {
                return null;
            }

            var newIndexBuilder = entityTypeBuilder.HasIndex(properties, Metadata.GetConfigurationSource());
            newIndexBuilder?.MergeAnnotationsFrom(Metadata);

            var isUniqueConfigurationSource = Metadata.GetIsUniqueConfigurationSource();
            if (isUniqueConfigurationSource.HasValue)
            {
                newIndexBuilder?.IsUnique(Metadata.IsUnique, isUniqueConfigurationSource.Value);
            }

            return newIndexBuilder;
        }
    }
}
