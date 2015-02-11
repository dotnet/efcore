// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;

namespace Microsoft.Data.Entity
{
    /// <summary>
    ///     <para>
    ///         Represents the options for a <see cref="DbContext" /> instance (such as the data store to be targeted). The
    ///         <see cref="DbContextOptions" /> for a context can be configured by overriding
    ///         <see cref="DbContext.OnConfiguring(DbContextOptions)" />
    ///         or externally creating a <see cref="DbContextOptions" /> and passing it to the <see cref="DbContext" />
    ///         constructor.
    ///     </para>
    ///     <para>
    ///         Data stores (and other extensions) typically define extension methods on this object that allow you to
    ///         configure the context.
    ///     </para>
    /// </summary>
    /// <typeparam name="T"> The type of context that will be constructed with the options. </typeparam>
    public class DbContextOptions<T> : DbContextOptions
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="DbContextOptions{T}" /> class.
        /// </summary>
        public DbContextOptions()
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="DbContextOptions{T}" /> class with options cloned from
        ///     another <see cref="DbContextOptions" /> instance.
        /// </summary>
        /// <param name="copyFrom"> The options to be cloned. </param>
        protected DbContextOptions([NotNull] DbContextOptions copyFrom)
            : base(copyFrom)
        {
        }

        /// <inheritdoc />
        public override DbContextOptions Clone() => new DbContextOptions<T>(this);

        /// <summary>
        ///     Sets the model to be used. If the model is set on <see cref="DbContextOptions" /> then
        ///     <see cref="DbContext.OnModelCreating(ModelBuilder)" /> will not be called on any context constructed
        ///     from the options.
        /// </summary>
        /// <param name="model"> The model to be used. </param>
        /// <returns>
        ///     The same <see cref="DbContextOptions{T}" /> instance so that multiple configuration calls can be chained together.
        /// </returns>
        public new virtual DbContextOptions<T> UseModel([NotNull] IModel model) => (DbContextOptions<T>)base.UseModel(model);
    }
}
