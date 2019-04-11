// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Metadata.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class ServiceMethodParameterBinding : DefaultServiceParameterBinding
    {
        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public ServiceMethodParameterBinding(
            [NotNull] Type parameterType,
            [NotNull] Type serviceType,
            [NotNull] MethodInfo method,
            [CanBeNull] IPropertyBase consumedProperty = null)
            : base(parameterType, serviceType, consumedProperty)
        {
            Method = method;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual MethodInfo Method { get; }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public override Expression BindToParameter(
            Expression materializationExpression,
            Expression entityTypeExpression)
        {
            var parameters = Method.GetParameters().Select(
                (p, i) => Expression.Parameter(p.ParameterType, "param" + i)).ToArray();

            var serviceVariable = Expression.Variable(ServiceType, "service");
            var delegateVariable = Expression.Variable(ParameterType, "delegate");

            return Expression.Block(
                new[] { serviceVariable, delegateVariable },
                new List<Expression>
                {
                    Expression.Assign(
                        serviceVariable,
                        base.BindToParameter(materializationExpression, entityTypeExpression)),
                    Expression.Assign(
                        delegateVariable,
                        Expression.Condition(
                            Expression.ReferenceEqual(serviceVariable, Expression.Constant(null)),
                            Expression.Constant(null, ParameterType),
                            Expression.Lambda(
                                Expression.Call(
                                    serviceVariable,
                                    Method,
                                    parameters),
                                parameters))),
                    delegateVariable
                });
        }
    }
}
