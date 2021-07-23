// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Design
{
    /// <summary>
    ///     Represents usage of an attribute.
    /// </summary>
    public class AttributeCodeFragment
    {
        private readonly List<object> _arguments;

        /// <summary>
        ///     Initializes a new instance of the <see cref="AttributeCodeFragment" /> class.
        /// </summary>
        /// <param name="type"> The attribute's CLR type. </param>
        /// <param name="arguments"> The attribute's arguments. </param>
        public AttributeCodeFragment(Type type, params object[] arguments)
        {
            Check.NotNull(type, nameof(type));
            Check.NotNull(arguments, nameof(arguments));

            Type = type;
            _arguments = new List<object>(arguments);
        }

        /// <summary>
        ///     Gets or sets the attribute's type.
        /// </summary>
        /// <value> The attribute's type. </value>
        public virtual Type Type { get; }

        /// <summary>
        ///     Gets the method call's arguments.
        /// </summary>
        /// <value> The method call's arguments. </value>
        public virtual IReadOnlyList<object> Arguments
            => _arguments;
    }
}
