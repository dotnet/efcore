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
    ///     Defines the binding of parameters to a CLR <see cref="ConstructorInfo" /> for an entity type.
    /// </summary>
    public class ConstructorBinding : InstantiationBinding
    {
        /// <summary>
        ///     Creates a new <see cref="ConstructorBinding" /> instance.
        /// </summary>
        /// <param name="constructor"> The constructor to use. </param>
        /// <param name="parameterBindings"> The parameters to bind. </param>
        public ConstructorBinding(
            [NotNull] ConstructorInfo constructor,
            [NotNull] IReadOnlyList<ParameterBinding> parameterBindings)
            : base(parameterBindings)
        {
            Check.NotNull(constructor, nameof(constructor));

            Constructor = constructor;
        }

        /// <summary>
        ///     The bound <see cref="ConstructorInfo" />.
        /// </summary>
        public virtual ConstructorInfo Constructor { get; }

        /// <summary>
        ///     Creates a <see cref="NewExpression" /> that represents creating an entity instance using the given
        ///     constructor.
        /// </summary>
        /// <param name="bindingInfo"> Information needed to create the expression. </param>
        /// <returns> The expression tree. </returns>
        public override Expression CreateConstructorExpression(ParameterBindingInfo bindingInfo)
            => Expression.New(
                Constructor,
                ParameterBindings.Select(b => b.BindToParameter(bindingInfo)));

        /// <summary>
        ///     The type that will be created from the expression tree created for this binding.
        /// </summary>
        public override Type RuntimeType
            => Constructor.DeclaringType;
    }
}
