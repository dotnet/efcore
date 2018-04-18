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
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class DefaultServiceParameterBinding : ServiceParameterBinding
    {
        private static readonly MethodInfo _getServiceMethod
            = typeof(InternalAccessorExtensions).GetMethod(nameof(InternalAccessorExtensions.GetService));

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public DefaultServiceParameterBinding(
            [NotNull] Type parameterType,
            [NotNull] Type serviceType,
            [CanBeNull] IPropertyBase consumedProperty = null)
            : base(parameterType, serviceType, consumedProperty)
        {
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override Expression BindToParameter(
            Expression materializationExpression,
            Expression entityTypeExpression,
            Expression entityExpression)
            => Expression.Call(
                _getServiceMethod.MakeGenericMethod(ServiceType),
                Expression.Convert(
                    Expression.Property(
                        materializationExpression,
                        MaterializationContext.ContextProperty),
                    typeof(IInfrastructure<IServiceProvider>)));
    }
}
