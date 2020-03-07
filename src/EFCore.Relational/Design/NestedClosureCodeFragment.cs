// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Design
{
    /// <summary>
    ///     Represents a nested closure code fragment.
    /// </summary>
    public class NestedClosureCodeFragment
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="NestedClosureCodeFragment" /> class.
        /// </summary>
        /// <param name="parameter">The nested closure parameter's name.</param>
        /// <param name="methodCall">The method call used as the body of the nested closure.</param>
        public NestedClosureCodeFragment([NotNull] string parameter, [NotNull] MethodCallCodeFragment methodCall)
        {
            Check.NotEmpty(parameter, nameof(parameter));
            Check.NotNull(methodCall, nameof(methodCall));

            Parameter = parameter;
            MethodCall = methodCall;
        }

        /// <summary>
        ///     Gets the nested closure parameter's name.
        /// </summary>
        /// <value>The parameter name.</value>
        public virtual string Parameter { get; }

        /// <summary>
        ///     Gets the method call used as the body of the nested closure.
        /// </summary>
        /// <value>The method call.</value>
        public virtual MethodCallCodeFragment MethodCall { get; }
    }
}
