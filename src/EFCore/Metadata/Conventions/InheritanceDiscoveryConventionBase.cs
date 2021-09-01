// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Infrastructure;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions
{
    /// <summary>
    ///     Base type for inheritance discovery conventions
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-conventions">EF Core model building conventions</see> for more information.
    /// </remarks>
    [Obsolete]
    public abstract class InheritanceDiscoveryConventionBase
    {
        /// <summary>
        ///     Creates a new instance of <see cref="InheritanceDiscoveryConventionBase" />.
        /// </summary>
        /// <param name="dependencies"> Parameter object containing dependencies for this convention. </param>
        protected InheritanceDiscoveryConventionBase(ProviderConventionSetBuilderDependencies dependencies)
        {
            Dependencies = dependencies;
        }

        /// <summary>
        ///     Dependencies for this service.
        /// </summary>
        protected virtual ProviderConventionSetBuilderDependencies Dependencies { get; }

        /// <summary>
        ///     Finds an entity type in the model that's associated with a CLR type that the given entity type's
        ///     associated CLR type is derived from and is the closest one in the CLR hierarchy.
        /// </summary>
        /// <param name="entityType"> The entity type. </param>
        protected virtual IConventionEntityType? FindClosestBaseType(IConventionEntityType entityType)
        {
            var baseType = entityType.ClrType.BaseType;
            var model = entityType.Model;
            IConventionEntityType? baseEntityType = null;
            while (baseType != null
                && baseEntityType == null
                && baseType != typeof(object))
            {
                baseEntityType = model.FindEntityType(baseType);
                baseType = baseType.BaseType;
            }

            return baseEntityType;
        }
    }
}
