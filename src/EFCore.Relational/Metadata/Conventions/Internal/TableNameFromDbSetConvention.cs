// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class TableNameFromDbSetConvention : IBaseTypeChangedConvention
    {
        private readonly IDictionary<Type, DbSetProperty> _sets;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public TableNameFromDbSetConvention([CanBeNull] DbContext context, [CanBeNull] IDbSetFinder setFinder)
            => _sets = setFinder?.CreateClrTypeDbSetMapping(context);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual bool Apply(InternalEntityTypeBuilder entityTypeBuilder, EntityType oldBaseType)
        {
            if (_sets != null)
            {
                var entityType = entityTypeBuilder.Metadata;

                if (oldBaseType == null
                    && entityType.BaseType != null)
                {
                    entityTypeBuilder.Relational(ConfigurationSource.Convention).ToTable(null);
                }
                else if (oldBaseType != null
                         && entityType.BaseType == null
                         && _sets.ContainsKey(entityType.ClrType))
                {
                    entityTypeBuilder.Relational(ConfigurationSource.Convention).ToTable(_sets[entityType.ClrType].Name);
                }
            }

            return true;
        }
    }
}
