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
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public class FactoryMethodConstructorBinding : ConstructorBinding
    {
        private readonly object _factoryInstance;
        private readonly MethodInfo _factoryMethod;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
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
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
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
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
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
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public override Type RuntimeType { get; }
    }
}
