// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.Metadata
{
    /// <summary>
    ///     Describes the binding from an <see cref="IProperty" /> to a parameter in a constructor, factory method,
    ///     or similar.
    /// </summary>
    public class PropertyParameterBinding : ParameterBinding
    {
        /// <summary>
        ///     Creates a new <see cref="PropertyParameterBinding" /> instance for the given <see cref="IProperty" />.
        /// </summary>
        /// <param name="property"> The property to bind. </param>
        public PropertyParameterBinding([NotNull] IProperty property)
            : base(property.ClrType, property)
        {
        }

        /// <summary>
        ///     Creates an expression tree representing the binding of the value of a property from a
        ///     materialization expression to a parameter of the constructor, factory method, etc.
        /// </summary>
        /// <param name="bindingInfo"> The binding information. </param>
        /// <returns> The expression tree. </returns>
        public override Expression BindToParameter(ParameterBindingInfo bindingInfo)
        {
            var property = ConsumedProperties[0];

            return Expression.Call(bindingInfo.MaterializationContextExpression, MaterializationContext.GetValueBufferMethod)
                .CreateValueBufferReadValueExpression(property.ClrType, bindingInfo.GetValueBufferIndex(property), property);
        }
    }
}
