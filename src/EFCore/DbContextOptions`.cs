// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore
{
    /// <summary>
    ///     The options to be used by a <see cref="DbContext" />. You normally override
    ///     <see cref="DbContext.OnConfiguring(DbContextOptionsBuilder)" /> or use a <see cref="DbContextOptionsBuilder{TContext}" />
    ///     to create instances of this class and it is not designed to be directly constructed in your application code.
    /// </summary>
    /// <typeparam name="TContext"> The type of the context these options apply to. </typeparam>
    public class DbContextOptions<TContext> : DbContextOptions
        where TContext : DbContext
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="DbContextOptions{TContext}" /> class. You normally override
        ///     <see cref="DbContext.OnConfiguring(DbContextOptionsBuilder)" /> or use a <see cref="DbContextOptionsBuilder{TContext}" />
        ///     to create instances of this class and it is not designed to be directly constructed in your application code.
        /// </summary>
        public DbContextOptions()
            : this(ImmutableSortedDictionary.Create<Type, IDbContextOptionsExtension>(TypeFullNameComparer.Instance))
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="DbContextOptions{TContext}" /> class. You normally override
        ///     <see cref="DbContext.OnConfiguring(DbContextOptionsBuilder)" /> or use a <see cref="DbContextOptionsBuilder{TContext}" />
        ///     to create instances of this class and it is not designed to be directly constructed in your application code.
        /// </summary>
        /// <param name="extensions"> The extensions that store the configured options. </param>
        public DbContextOptions(
            IReadOnlyDictionary<Type, IDbContextOptionsExtension> extensions)
            : base(extensions)
        {
        }

        /// <inheritdoc />
        public override DbContextOptions WithExtension<TExtension>(TExtension extension)
        {
            Check.NotNull(extension, nameof(extension));

            return new DbContextOptions<TContext>(ExtensionsMap.SetItem(extension.GetType(), extension));
        }

        /// <summary>
        ///     The type of context that these options are for (<typeparamref name="TContext" />).
        /// </summary>
        public override Type ContextType
            => typeof(TContext);
    }
}
