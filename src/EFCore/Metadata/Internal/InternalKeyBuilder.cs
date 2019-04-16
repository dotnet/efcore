// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics;
using System.Linq;
using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Metadata.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    [DebuggerDisplay("{Metadata,nq}")]
    // Issue#11266 This type is being used by provider code. Do not break.
    public class InternalKeyBuilder : InternalMetadataItemBuilder<Key>
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public InternalKeyBuilder([NotNull] Key key, [NotNull] InternalModelBuilder modelBuilder)
            : base(key, modelBuilder)
        {
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
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
    }
}
