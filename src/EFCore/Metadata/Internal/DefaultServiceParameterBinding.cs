// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.Metadata.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public class DefaultServiceParameterBinding : ServiceParameterBinding
    {
        private static readonly MethodInfo _getServiceMethod
            = typeof(InternalAccessorExtensions).GetMethod(nameof(InternalAccessorExtensions.GetService));

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public DefaultServiceParameterBinding(
            [NotNull] Type parameterType,
            [NotNull] Type serviceType,
            [CanBeNull] IPropertyBase consumedProperty = null)
            : base(parameterType, serviceType, consumedProperty)
        {
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public override Expression BindToParameter(
            Expression materializationExpression,
            Expression entityTypeExpression)
            => Expression.Call(
                _getServiceMethod.MakeGenericMethod(ServiceType),
                Expression.Convert(
                    Expression.Property(
                        materializationExpression,
                        MaterializationContext.ContextProperty),
                    typeof(IInfrastructure<IServiceProvider>)));
    }
}
