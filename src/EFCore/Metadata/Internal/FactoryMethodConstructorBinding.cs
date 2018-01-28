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
    public class FactoryMethodConstructorBinding : ConstructorBinding
    {
        private readonly object _factoryInstance;
        private readonly MethodInfo _factoryMethod;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public FactoryMethodConstructorBinding(
            [NotNull] MethodInfo factoryMethod,
            [NotNull] IReadOnlyList<ParameterBinding> parameterBindings,
            [NotNull] Type runtimeType)
            : base(parameterBindings)
        {
            _factoryMethod = factoryMethod;
            RuntimeType = runtimeType;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public FactoryMethodConstructorBinding(
            [NotNull] object factoryInstance,
            [NotNull] MethodInfo factoryMethod,
            [NotNull] IReadOnlyList<ParameterBinding> parameterBindings,
            [NotNull] Type runtimeType)
            : this(factoryMethod, parameterBindings, runtimeType)
        {
            _factoryInstance = factoryInstance;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override Expression CreateConstructorExpression(ParameterBindingInfo bindingInfo)
        {
            var arguments = ParameterBindings.Select(b => b.BindToParameter(bindingInfo));

            Expression expression
                = _factoryInstance == null
                    ? Expression.Call(
                        _factoryMethod,
                        arguments)
                    : Expression.Call(
                        Expression.Constant(_factoryInstance),
                        _factoryMethod,
                        arguments);

            if (_factoryMethod.ReturnType != RuntimeType)
            {
                expression = Expression.Convert(expression, RuntimeType);
            }

            return expression;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override Type RuntimeType { get; }
    }
}
