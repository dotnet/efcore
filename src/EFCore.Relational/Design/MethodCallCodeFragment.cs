// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Design
{
    /// <summary>
    ///     Represents a call to a method.
    /// </summary>
    public class MethodCallCodeFragment
    {
        private readonly List<object> _arguments;

        /// <summary>
        ///     Initializes a new instance of the <see cref="MethodCallCodeFragment" /> class.
        /// </summary>
        /// <param name="method"> The method's name. </param>
        /// <param name="arguments"> The method call's arguments. </param>
        public MethodCallCodeFragment([NotNull] string method, [NotNull] params object[] arguments)
        {
            Check.NotEmpty(method, nameof(method));
            Check.NotNull(arguments, nameof(arguments));

            Method = method;
            _arguments = new List<object>(arguments);
        }


        /// <summary>
        ///     Gets or sets the method's name.
        /// </summary>
        /// <value> The method's name. </value>
        public virtual string Method { get; }

        /// <summary>
        ///     Gets the method call's arguments.
        /// </summary>
        /// <value> The method call's arguments. </value>
        public virtual IReadOnlyList<object> Arguments => _arguments;
    }
}
