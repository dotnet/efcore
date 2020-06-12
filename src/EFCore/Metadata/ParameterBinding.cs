// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Metadata
{
    /// <summary>
    ///     Describes the binding from one or many EF model properties, dependency injection services, or metadata types to
    ///     a parameter in a constructor, factory method, or similar.
    /// </summary>
    public abstract class ParameterBinding
    {
        /// <summary>
        ///     Creates a new <see cref="ParameterBinding" /> instance.
        /// </summary>
        /// <param name="parameterType"> The parameter CLR type. </param>
        /// <param name="consumedProperties"> The properties that are handled by this binding and so do not need to be set in some other way. </param>
        protected ParameterBinding(
            [NotNull] Type parameterType,
            [NotNull] params IPropertyBase[] consumedProperties)
        {
            Check.NotNull(parameterType, nameof(parameterType));
            Check.NotNull(consumedProperties, nameof(consumedProperties));

            ParameterType = parameterType;
            ConsumedProperties = consumedProperties;
        }

        /// <summary>
        ///     The parameter CLR type.
        /// </summary>
        public virtual Type ParameterType { get; }

        /// <summary>
        ///     The properties that are handled by this binding and so do not need to be set in some other way.
        /// </summary>
        public virtual IReadOnlyList<IPropertyBase> ConsumedProperties { get; }

        /// <summary>
        ///     Creates an expression tree representing the binding of the value of a property from a
        ///     materialization expression to a parameter of the constructor, factory method, etc.
        /// </summary>
        /// <param name="bindingInfo"> The binding information. </param>
        /// <returns> The expression tree. </returns>
        public abstract Expression BindToParameter(ParameterBindingInfo bindingInfo);
    }
}
