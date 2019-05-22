// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Metadata
{
    /// <summary>
    ///     Describes the binding from many EF model properties, dependency injection services, or metadata types to
    ///     a new array of objects suitable for passing to a general purpose factory method such as is often used for
    ///     creating proxies.
    /// </summary>
    public class ObjectArrayParameterBinding : ParameterBinding
    {
        private readonly IReadOnlyList<ParameterBinding> _bindings;

        /// <summary>
        ///     Creates a new <see cref="ObjectArrayParameterBinding" /> taking all the given <see cref="ParameterBinding" />
        ///     instances and combining them into one binding that will initialize an array of <see cref="object" />.
        /// </summary>
        /// <param name="bindings"> The binding to combine. </param>
        public ObjectArrayParameterBinding([NotNull] IReadOnlyList<ParameterBinding> bindings)
            : base(
                typeof(object[]),
                Check.NotNull(bindings, nameof(bindings)).SelectMany(b => b.ConsumedProperties).ToArray())
        {
            _bindings = bindings;
        }

        /// <summary>
        ///     Creates an expression tree representing the binding of the value of a property from a
        ///     materialization expression to a parameter of the constructor, factory method, etc.
        /// </summary>
        /// <param name="bindingInfo"> The binding information. </param>
        /// <returns> The expression tree. </returns>
        public override Expression BindToParameter(ParameterBindingInfo bindingInfo)
            => Expression.NewArrayInit(
                typeof(object),
                _bindings.Select(
                    b =>
                    {
                        var expression = b.BindToParameter(bindingInfo);

                        if (expression.Type.GetTypeInfo().IsValueType)
                        {
                            expression = Expression.Convert(expression, typeof(object));
                        }

                        return expression;
                    }));
    }
}
