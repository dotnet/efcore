// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Microsoft.EntityFrameworkCore.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public class ManyToManyLoaderFactory
    {
        private static readonly MethodInfo _genericCreate
            = typeof(ManyToManyLoaderFactory).GetTypeInfo().GetDeclaredMethod(nameof(CreateManyToMany));

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual ICollectionLoader Create([NotNull] ISkipNavigation skipNavigation)
            => (ICollectionLoader)_genericCreate.MakeGenericMethod(
                    skipNavigation.TargetEntityType.ClrType,
                    skipNavigation.DeclaringEntityType.ClrType)
                .Invoke(null, new object[] { skipNavigation });

        [UsedImplicitly]
        private static ICollectionLoader CreateManyToMany<TEntity, TTargetEntity>(ISkipNavigation skipNavigation)
            where TEntity : class
            where TTargetEntity : class
            => new ManyToManyLoader<TEntity, TTargetEntity>(skipNavigation);
    }
}
