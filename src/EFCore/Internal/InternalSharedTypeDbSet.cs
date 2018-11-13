// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class InternalSharedTypeDbSet<TEntity> : InternalDbSet<TEntity>
        where TEntity : class
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public InternalSharedTypeDbSet([NotNull] DbContext context, [NotNull] string entityTypeName)
            :base(context)
        {
            Check.NotNull(context, nameof(context));

            EntityTypeName = entityTypeName;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual string EntityTypeName { get; }

        protected override IEntityType EntityType
        {
            get
            {
                if (_entityType == null)
                {
                    _entityType = _context.Model.FindEntityType(EntityTypeName);
                    if (_entityType == null)
                    {
                        throw new InvalidOperationException(CoreStrings.InvalidSharedTypeSet(EntityTypeName));
                    }
                }

                return _entityType;
            }
        }
    }
}
