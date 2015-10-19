// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Storage;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Infrastructure
{
    /// <summary>
    ///     Provides access to database related information and operations for a context.
    ///     Instances of this class are typically obtained from <see cref="DbContext.Database" /> and it is not designed
    ///     to be directly constructed in your application code.
    /// </summary>
    public class DatabaseFacade : IInfrastructure<IServiceProvider>
    {
        private readonly DbContext _context;

        /// <summary>
        ///     Initializes a new instance of the <see cref="DatabaseFacade" /> class. Instances of this class are typically
        ///     obtained from <see cref="DbContext.Database" /> and it is not designed to be directly constructed
        ///     in your application code.
        /// </summary>
        /// <param name="context"> The context this database API belongs to .</param>
        public DatabaseFacade([NotNull] DbContext context)
        {
            Check.NotNull(context, nameof(context));

            _context = context;
        }

        /// <summary>
        ///     Ensures that the database for the context exists. If it exists, no action is taken. If it does not
        ///     exist then the database and all its schema are created. If the database exists, then no effort is made
        ///     to ensure it is compatible with the model for this context.
        /// </summary>
        /// <returns> True if the database is created, false if it already existed. </returns>
        public virtual bool EnsureCreated() => this.GetService<IDatabaseCreator>().EnsureCreated();

        /// <summary>
        ///     Asynchronously ensures that the database for the context exists. If it exists, no action is taken. If it does not
        ///     exist then the database and all its schema are created. If the database exists, then no effort is made
        ///     to ensure it is compatible with the model for this context.
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
        /// <returns> 
        ///     A task that represents the asynchronous save operation. The task result contains true if the database is created, 
        ///     false if it already existed. 
        /// </returns>
        public virtual Task<bool> EnsureCreatedAsync(CancellationToken cancellationToken = default(CancellationToken))
            => this.GetService<IDatabaseCreator>().EnsureCreatedAsync(cancellationToken);

        /// <summary>
        ///     <para>
        ///         Ensures that the database for the context does not exist. If it does not exist, no action is taken. If it does
        ///         exist then the database is deleted.
        ///     </para>
        ///     <para>
        ///         Warning: The entire database is deleted an no effort is made to remove just the database objects that are used by 
        ///         the model for this context.
        ///     </para>
        /// </summary>
        /// <returns> True if the database is deleted, false if it did not exist. </returns>
        public virtual bool EnsureDeleted() => this.GetService<IDatabaseCreator>().EnsureDeleted();

        /// <summary>
        ///     <para>
        ///         Asynchronously ensures that the database for the context does not exist. If it does not exist, no action is taken. If it does
        ///         exist then the database is deleted.
        ///     </para>
        ///     <para>
        ///         Warning: The entire database is deleted an no effort is made to remove just the database objects that are used by 
        ///         the model for this context.
        ///     </para>
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
        /// <returns> 
        ///     A task that represents the asynchronous save operation. The task result contains true if the database is deleted, 
        ///     false if it did not exist. 
        /// </returns>
        public virtual Task<bool> EnsureDeletedAsync(CancellationToken cancellationToken = default(CancellationToken))
            => this.GetService<IDatabaseCreator>().EnsureDeletedAsync(cancellationToken);

        /// <summary>
        ///     Starts a new transaction.
        /// </summary>
        /// <returns>
        ///     A <see cref="IDbContextTransaction"/> that represents the started transaction.
        /// </returns>
        public virtual IDbContextTransaction BeginTransaction()
            => this.GetService<IDbContextTransactionManager>().BeginTransaction();

        /// <summary>
        ///     Asynchronously starts a new transaction.
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
        /// <returns>
        ///     A task that represents the asynchronus transaction initialization. The task result contains a <see cref="IDbContextTransaction"/>
        ///     that represents the started transaction.
        /// </returns>
        public virtual Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default(CancellationToken))
            => this.GetService<IDbContextTransactionManager>().BeginTransactionAsync(cancellationToken);

        /// <summary>
        ///     Applies the outstanding operations in the current transaction to the database.
        /// </summary>
        public virtual void CommitTransaction()
            => this.GetService<IDbContextTransactionManager>().CommitTransaction();

        /// <summary>
        ///     Discards the outstanding operations in the current transaction.
        /// </summary>
        public virtual void RollbackTransaction()
            => this.GetService<IDbContextTransactionManager>().RollbackTransaction();

        /// <summary>
        ///     <para>
        ///         Gets the scoped <see cref="IServiceProvider" /> being used to resolve services.
        ///     </para>
        ///     <para>
        ///         This property is intended for use by extension methods that need to make use of services
        ///         not directly exposed in the public API surface.
        ///     </para>
        /// </summary>
        IServiceProvider IInfrastructure<IServiceProvider>.Instance => ((IInfrastructure<IServiceProvider>)_context).Instance;
    }
}
