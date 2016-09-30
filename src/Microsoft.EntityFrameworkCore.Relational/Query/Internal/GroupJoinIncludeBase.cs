// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Microsoft.EntityFrameworkCore.Query.Internal
{
    public abstract class GroupJoinIncludeBase : IDisposable
    {
        protected GroupJoinIncludeBase(
            [NotNull] IReadOnlyList<INavigation> navigationPath,
            bool querySourceRequiresTracking)
        {
            NavigationPath = navigationPath;
            QuerySourceRequiresTracking = querySourceRequiresTracking;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected virtual IReadOnlyList<INavigation> NavigationPath { get; }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected virtual bool QuerySourceRequiresTracking { get; }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual Delegate EntityAccessor { get; [param: NotNull] protected set; }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual void Dispose()
        {
            // no-op
        }
    }
}
