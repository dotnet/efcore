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
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
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
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual MethodInfo Method { get; }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override Expression BindToParameter(
            Expression materializationExpression,
            Expression entityTypeExpression,
            Expression entityExpression)
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
                        base.BindToParameter(materializationExpression, entityTypeExpression, entityExpression)),
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
