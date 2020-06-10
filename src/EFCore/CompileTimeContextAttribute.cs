// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore
{
    /// <summary>
    ///     Specifies the corresponding compile time context type.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class CompileTimeContextAttribute : Attribute
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="CompileTimeContextAttribute" /> class.
        /// </summary>
        /// <param name="contextType">The compile time context type.</param>
        public CompileTimeContextAttribute([NotNull] Type contextType)
        {
            Check.NotNull(contextType, nameof(contextType));

            ContextType = contextType;
        }

        /// <summary>
        ///     The compile time context type.
        /// </summary>
        public Type ContextType { get; }
    }
}
