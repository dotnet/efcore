// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

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
        private readonly IDbQuerySource _querySource;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public DbSetInitializer(
            [NotNull] IDbSetFinder setFinder,
            [NotNull] IDbSetSource setSource,
            [NotNull] IDbQuerySource querySource)
        {
            _setFinder = setFinder;
            _setSource = setSource;
            _querySource = querySource;
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
                    setInfo.IsQueryType
                        ? ((IDbQueryCache)context).GetOrAddQuery(_querySource, setInfo.ClrType)
                        : ((IDbSetCache)context).GetOrAddSet(_setSource, setInfo.ClrType));
            }
        }
    }
}
