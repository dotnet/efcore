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
    ///     Defines how to create an entity instance through the binding of EF model properties to, for
    ///     example, constructor parameters or parameters of a factory method.
    /// </summary>
    public abstract class InstantiationBinding
    {
        /// <summary>
        ///     Creates a new <see cref="InstantiationBinding" /> instance.
        /// </summary>
        /// <param name="parameterBindings"> The parameter bindings to use. </param>
        protected InstantiationBinding(
            [NotNull] IReadOnlyList<ParameterBinding> parameterBindings)
        {
            Check.NotNull(parameterBindings, nameof(parameterBindings));

            ParameterBindings = parameterBindings;
        }

        /// <summary>
        ///     Creates an expression tree that represents creating an entity instance from the given binding
        ///     information. For example, this might be a <see cref="NewExpression" /> to call a constructor,
        ///     or a <see cref="MethodCallExpression" /> to call a factory method.
        /// </summary>
        /// <param name="bindingInfo"> Information needed to create the expression. </param>
        /// <returns> The expression tree. </returns>
        public abstract Expression CreateConstructorExpression(ParameterBindingInfo bindingInfo);

        /// <summary>
        ///     The collection of <see cref="ParameterBinding" /> instances used.
        /// </summary>
        public virtual IReadOnlyList<ParameterBinding> ParameterBindings { get; }

        /// <summary>
        ///     The type that will be created from the expression tree created for this binding.
        /// </summary>
        public abstract Type RuntimeType { get; }
    }
}
