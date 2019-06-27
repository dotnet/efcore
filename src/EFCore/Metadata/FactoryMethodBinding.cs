// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Metadata
{
    /// <summary>
    ///     Defines the binding of parameters to a factory method.
    /// </summary>
    public class FactoryMethodBinding : InstantiationBinding
    {
        private readonly object _factoryInstance;
        private readonly MethodInfo _factoryMethod;

        /// <summary>
        ///     Creates a new <see cref="FactoryMethodBinding" /> instance for a static factory method.
        /// </summary>
        /// <param name="factoryMethod"> The factory method to bind to. </param>
        /// <param name="parameterBindings"> The parameters to use. </param>
        /// <param name="runtimeType"> The CLR type of the instance created by the factory method. </param>
        public FactoryMethodBinding(
            [NotNull] MethodInfo factoryMethod,
            [NotNull] IReadOnlyList<ParameterBinding> parameterBindings,
            [NotNull] Type runtimeType)
            : base(parameterBindings)
        {
            Check.NotNull(factoryMethod, nameof(factoryMethod));
            Check.NotNull(runtimeType, nameof(runtimeType));

            _factoryMethod = factoryMethod;
            RuntimeType = runtimeType;
        }

        /// <summary>
        ///     Creates a new <see cref="FactoryMethodBinding" /> instance for a static factory method.
        /// </summary>
        /// <param name="factoryInstance"> The object on which the factory method should be called. </param>
        /// <param name="factoryMethod"> The factory method to bind to. </param>
        /// <param name="parameterBindings"> The parameters to use. </param>
        /// <param name="runtimeType"> The CLR type of the instance created by the factory method. </param>
        public FactoryMethodBinding(
            [NotNull] object factoryInstance,
            [NotNull] MethodInfo factoryMethod,
            [NotNull] IReadOnlyList<ParameterBinding> parameterBindings,
            [NotNull] Type runtimeType)
            : this(factoryMethod, parameterBindings, runtimeType)
        {
            Check.NotNull(factoryInstance, nameof(factoryInstance));

            _factoryInstance = factoryInstance;
        }

        /// <summary>
        ///     Creates a <see cref="MethodCallExpression" /> using the given method.
        /// </summary>
        /// <param name="bindingInfo"> Information needed to create the expression. </param>
        /// <returns> The expression tree. </returns>
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
        ///     The type that will be created from the expression tree created for this binding.
        /// </summary>
        public override Type RuntimeType { get; }
    }
}
