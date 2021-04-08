// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
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
        public ObjectArrayParameterBinding(IReadOnlyList<ParameterBinding> bindings)
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

                        if (expression.Type.IsValueType)
                        {
                            expression = Expression.Convert(expression, typeof(object));
                        }

                        return expression;
                    }));

        /// <summary>
        ///     Creates a copy that contains the given consumed properties.
        /// </summary>
        /// <param name="consumedProperties"> The new consumed properties. </param>
        /// <returns> A copy with replaced consumed properties. </returns>
        public override ParameterBinding With(IPropertyBase[] consumedProperties)
        {
            var newBindings = new List<ParameterBinding>(_bindings.Count);
            var propertyCount = 0;
            foreach (var binding in _bindings)
            {
                var newBinding = binding.With(consumedProperties.Skip(propertyCount).Take(binding.ConsumedProperties.Count).ToArray());
                newBindings.Add(newBinding);
                propertyCount += binding.ConsumedProperties.Count;
            }

            return new ObjectArrayParameterBinding(newBindings);
        }
    }
}
