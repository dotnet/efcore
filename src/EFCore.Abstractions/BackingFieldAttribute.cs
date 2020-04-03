// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore
{
    /// <summary>
    ///     Names the backing field associated with this property or navigation property.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class BackingFieldAttribute : Attribute
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="BackingFieldAttribute" /> class.
        /// </summary>
        /// <param name="name"> The name of the backing field. </param>
        public BackingFieldAttribute([NotNull] string name)
        {
            Check.NotEmpty(name, nameof(name));

            Name = name;
        }

        /// <summary>
        ///     The name of the backing field.
        /// </summary>
        public string Name { get; }
    }
}
