// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.Internal
{
    /// <summary>
    ///     <para>
    ///         This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///         directly from your code. This API may change or be removed in future releases.
    ///     </para>
    ///     <para>
    ///         The service lifetime is <see cref="ServiceLifetime.Singleton"/>. This means a single instance
    ///         is used by many <see cref="DbContext"/> instances. The implementation must be thread-safe.
    ///         This service cannot depend on services registered as <see cref="ServiceLifetime.Scoped"/>.
    ///     </para>
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
                setInfo.Setter.SetClrValue(
                    context,
                    ((IDbSetCache)context).GetOrAddSet(_setSource, setInfo.ClrType));
            }
        }
    }
}
