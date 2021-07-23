// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Microsoft.EntityFrameworkCore.ChangeTracking.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public abstract class SnapshotFactoryFactory<TInput> : SnapshotFactoryFactory
    {
        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual Func<TInput, ISnapshot> Create(IEntityType entityType)
        {
            if (GetPropertyCount(entityType) == 0)
            {
                return e => Snapshot.Empty;
            }

            var parameter = Expression.Parameter(typeof(TInput), "source");

            return Expression.Lambda<Func<TInput, ISnapshot>>(
                    CreateConstructorExpression(entityType, parameter),
                    parameter)
                .Compile();
        }
    }
}
