// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Concurrent;
using System.Reflection;
using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class EntityFinderSource : IEntityFinderSource
    {
        private static readonly MethodInfo _genericCreate
            = typeof(EntityFinderSource).GetTypeInfo().GetDeclaredMethod(nameof(CreateConstructor));

        private readonly ConcurrentDictionary<Type, Func<DbContext, IEntityFinder>> _cache
            = new ConcurrentDictionary<Type, Func<DbContext, IEntityFinder>>();

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual IEntityFinder Create(DbContext context, Type type)
            => _cache.GetOrAdd(
                type,
                t =>
                    {
                        if (context.Model.FindEntityType(t) == null)
                        {
                            throw new InvalidOperationException(CoreStrings.InvalidSetType(t.ShortDisplayName()));
                        }

                        return (Func<DbContext, IEntityFinder>)_genericCreate.MakeGenericMethod(t).Invoke(null, null);
                    })(context);

        [UsedImplicitly]
        private static Func<DbContext, IEntityFinder> CreateConstructor<TEntity>() where TEntity : class
            => c => new EntityFinder<TEntity>(c);
    }
}
