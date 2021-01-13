// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Microsoft.EntityFrameworkCore.Infrastructure
{
    /// <summary>
    ///     A struct representing facets of <see cref="DbSet{TEntity}" /> property defined on DbContext derived type.
    /// </summary>
    public readonly struct DbSetProperty
    {
        /// <summary>
        ///     Initializes new <see cref="DbSetProperty" /> with given values.
        /// </summary>
        /// <param name="name"> The name of DbSet. </param>
        /// <param name="type"> The entity clr type of DbSet. </param>
        /// <param name="setter"> The setter for DbSet property. </param>
        public DbSetProperty(
            [NotNull] string name,
            [NotNull] Type type,
            [CanBeNull] IClrPropertySetter setter)
        {
            Name = name;
            Type = type;
            Setter = setter;
        }

        /// <summary>
        ///     Gets the name of this DbSet property.
        /// </summary>
        public string Name { get; }

        /// <summary>
        ///     Gets the clr type of entity type this DbSet property represent.
        /// </summary>
        public Type Type { get; }

        /// <summary>
        ///     Gets the clr type of entity type this DbSet property represent.
        /// </summary>
        [Obsolete("Use Type")]
        public Type ClrType
            => Type;

        /// <summary>
        ///     The property setter for this DbSet property.
        /// </summary>
        public IClrPropertySetter Setter { get; }
    }
}
