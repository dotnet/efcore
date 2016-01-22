// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Infrastructure
{
    /// <summary>
    ///     Identifies the <see cref="DbContext" /> that a class belongs to. For example, this attribute is used
    ///     to identify which context a migration applies to.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class DbContextAttribute : Attribute
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="DbContextAttribute" /> class.
        /// </summary>
        /// <param name="contextType"> The associated context. </param>
        public DbContextAttribute([NotNull] Type contextType)
        {
            Check.NotNull(contextType, nameof(contextType));

            ContextType = contextType;
        }

        /// <summary>
        ///     Gets the associated context.
        /// </summary>
        public Type ContextType { get; }
    }
}
