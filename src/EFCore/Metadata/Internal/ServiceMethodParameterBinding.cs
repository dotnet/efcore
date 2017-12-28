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
    public class ServiceMethodParameterBinding : ServiceParameterBinding
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public ServiceMethodParameterBinding(
            [NotNull] Type parameterType,
            [NotNull] Type serviceType,
            [NotNull] MethodInfo method)
            : base(parameterType, serviceType)
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
        public override Expression BindToParameter(ParameterBindingInfo bindingInfo)
        {
            var parameters = Method.GetParameters().Select(
                (p, i) => Expression.Parameter(p.ParameterType, "param" + i)).ToArray();

            var serviceVariable = Expression.Variable(ServiceType, "service");

            return Expression.Block(
                new[] { serviceVariable },
                new List<Expression>
                {
                    Expression.Assign(
                        serviceVariable,
                        base.BindToParameter(bindingInfo)),
                    Expression.Lambda(
                        Expression.Call(
                            serviceVariable,
                            Method,
                            parameters),
                        parameters)
                });
        }
    }
}
