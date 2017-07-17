// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Migrations
{
    /// <summary>
    ///     Indicates that a class is a <see cref="Migration" /> and provides its identifier.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class MigrationAttribute : Attribute
    {
        /// <summary>
        ///     Creates a new instance of this attribute.
        /// </summary>
        /// <param name="id"> The migration identifier. </param>
        public MigrationAttribute([NotNull] string id)
        {
            Check.NotEmpty(id, nameof(id));

            Id = id;
        }

        /// <summary>
        ///     The migration identifier.
        /// </summary>
        public string Id { get; }
    }
}
