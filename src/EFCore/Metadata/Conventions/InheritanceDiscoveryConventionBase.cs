// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Infrastructure;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions
{
    /// <summary>
    ///     Base type for inheritance discovery conventions
    /// </summary>
    [Obsolete]
    public abstract class InheritanceDiscoveryConventionBase
    {
        /// <summary>
        ///     Creates a new instance of <see cref="InheritanceDiscoveryConventionBase" />.
        /// </summary>
        /// <param name="dependencies"> Parameter object containing dependencies for this convention. </param>
        protected InheritanceDiscoveryConventionBase([NotNull] ProviderConventionSetBuilderDependencies dependencies)
        {
            Dependencies = dependencies;
        }

        /// <summary>
        ///     Parameter object containing service dependencies.
        /// </summary>
        protected virtual ProviderConventionSetBuilderDependencies Dependencies { get; }

        /// <summary>
        ///     Finds an entity type in the model that's associated with a CLR type that the given entity type's
        ///     associated CLR type is derived from and is the closest one in the CLR hierarchy.
        /// </summary>
        /// <param name="entityType"> The entity type. </param>
        protected virtual IConventionEntityType FindClosestBaseType([NotNull] IConventionEntityType entityType)
        {
            var baseType = entityType.ClrType.BaseType;
            var model = entityType.Model;
            IConventionEntityType baseEntityType = null;
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
