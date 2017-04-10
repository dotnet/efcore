// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class DbSetInitializer : IDbSetInitializer
    {
        private readonly IDbSetFinder _setFinder;
        private readonly IDbSetSource _setSource;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public DbSetInitializer(
            [NotNull] IDbSetFinder setFinder,
            [NotNull] IDbSetSource setSource)
        {
            _setFinder = setFinder;
            _setSource = setSource;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual void InitializeSets(DbContext context)
        {
            foreach (var setInfo in _setFinder.FindSets(context).Where(p => p.Setter != null))
            {
                setInfo.Setter.SetClrValue(context, _setSource.Create(context, setInfo.ClrType));
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual DbSet<TEntity> CreateSet<TEntity>(DbContext context) where TEntity : class
            => (DbSet<TEntity>)CreateSet(context, typeof(TEntity));

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual object CreateSet(DbContext context, Type type)
            => _setSource.Create(context, type);
    }
}
