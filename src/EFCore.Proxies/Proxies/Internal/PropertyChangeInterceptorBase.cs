// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Castle.DynamicProxy;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Proxies.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public abstract class PropertyChangeInterceptorBase
    {
        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected PropertyChangeInterceptorBase([NotNull] IEntityType entityType)
        {
            EntityType = entityType;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected virtual IEntityType EntityType { get; }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected virtual ValueComparer GetValueComparer([NotNull] IProperty property)
            => property.IsKey()
                || property.IsForeignKey()
                    ? property.GetKeyValueComparer()
                    : property.GetValueComparer();

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected virtual string FindPropertyName([NotNull] IInvocation invocation)
        {
            var methodName = invocation.Method.Name;
            if (methodName.Equals("set_Item", StringComparison.Ordinal))
            {
#pragma warning disable EF1001 // Internal EF Core API usage.
                var indexerPropertyInfo = EntityType.FindIndexerPropertyInfo();
#pragma warning restore EF1001 // Internal EF Core API usage.

                if (indexerPropertyInfo != null
                    && indexerPropertyInfo.GetSetMethod(nonPublic: true) == invocation.Method)
                {
                    return (string)invocation.Arguments[0];
                }
            }

            return methodName.Substring(4);
        }
    }
}
