// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public class OwnedTypesConvention : IEntityTypeRemovedConvention
    {
        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public OwnedTypesConvention([NotNull] IDiagnosticsLogger<DbLoggerCategory.Model> logger)
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
        public virtual bool Apply(InternalModelBuilder modelBuilder, EntityType type)
        {
            if (type.HasDefiningNavigation())
            {
                var entityTypes = modelBuilder.Metadata.GetEntityTypes(type.Name);
                var otherEntityType = entityTypes.FirstOrDefault();
                if (otherEntityType?.HasDefiningNavigation() == true)
                {
                    var ownership = otherEntityType.FindOwnership();
                    if (ownership != null
                        && entityTypes.Count == 1)
                    {
                        using (modelBuilder.Metadata.ConventionDispatcher.StartBatch())
                        {
                            var detachedRelationship = InternalEntityTypeBuilder.DetachRelationship(ownership);

                            var weakSnapshot = InternalEntityTypeBuilder.DetachAllMembers(otherEntityType);
                            modelBuilder.RemoveEntityType(otherEntityType, ConfigurationSource.Explicit);

                            detachedRelationship.WeakEntityTypeSnapshot = weakSnapshot;
                            detachedRelationship.Attach(ownership.PrincipalEntityType.Builder);
                        }
                    }
                }
            }

            return true;
        }
    }
}
