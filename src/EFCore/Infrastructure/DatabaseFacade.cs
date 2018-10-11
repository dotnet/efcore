// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Infrastructure
{
    /// <summary>
    ///     Provides access to database related information and operations for a context.
    ///     Instances of this class are typically obtained from <see cref="DbContext.Database" /> and it is not designed
    ///     to be directly constructed in your application code.
    /// </summary>
    public class DatabaseFacade : IInfrastructure<IServiceProvider>
    {
        private readonly DbContext _context;
        private IDatabaseCreator _databaseCreator;
        private IDbContextTransactionManager _transactionManager;
        private IExecutionStrategyFactory _executionStrategyFactory;

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
        ///     <para>
        ///         Ensures that the database for the context exists. If it exists, no action is taken. If it does not
        ///         exist then the database and all its schema are created. If the database exists, then no effort is made
        ///         to ensure it is compatible with the model for this context.
        ///     </para>
        ///     <para>
        ///         Note that this API does not use migrations to create the database. In addition, the database that is
        ///         created cannot be later updated using migrations. If you are targeting a relational database and using migrations,
        ///         you can use the DbContext.Database.Migrate() method to ensure the database is created and all migrations
        ///         are applied.
        ///     </para>
        /// </summary>
        /// <returns> True if the database is created, false if it already existed. </returns>
        public virtual bool EnsureCreated() => DatabaseCreator.EnsureCreated();

        /// <summary>
        ///     <para>
        ///         Asynchronously ensures that the database for the context exists. If it exists, no action is taken. If it does not
        ///         exist then the database and all its schema are created. If the database exists, then no effort is made
        ///         to ensure it is compatible with the model for this context.
        ///     </para>
        ///     <para>
        ///         Note that this API does not use migrations to create the database. In addition, the database that is
        ///         created cannot be later updated using migrations. If you are targeting a relational database and using migrations,
        ///         you can use the DbContext.Database.Migrate() method to ensure the database is created and all migrations
        ///         are applied.
        ///     </para>
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
        /// <returns>
        ///     A task that represents the asynchronous save operation. The task result contains true if the database is created,
        ///     false if it already existed.
        /// </returns>
        public virtual Task<bool> EnsureCreatedAsync(CancellationToken cancellationToken = default)
            => DatabaseCreator.EnsureCreatedAsync(cancellationToken);

        /// <summary>
        ///     <para>
        ///         Ensures that the database for the context does not exist. If it does not exist, no action is taken. If it does
        ///         exist then the database is deleted.
        ///     </para>
        ///     <para>
        ///         Warning: The entire database is deleted, and no effort is made to remove just the database objects that are used by
        ///         the model for this context.
        ///     </para>
        /// </summary>
        /// <returns> True if the database is deleted, false if it did not exist. </returns>
        public virtual bool EnsureDeleted() => DatabaseCreator.EnsureDeleted();

        /// <summary>
        ///     <para>
        ///         Asynchronously ensures that the database for the context does not exist. If it does not exist, no action is taken. If it does
        ///         exist then the database is deleted.
        ///     </para>
        ///     <para>
        ///         Warning: The entire database is deleted, and no effort is made to remove just the database objects that are used by
        ///         the model for this context.
        ///     </para>
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
        /// <returns>
        ///     A task that represents the asynchronous save operation. The task result contains true if the database is deleted,
        ///     false if it did not exist.
        /// </returns>
        public virtual Task<bool> EnsureDeletedAsync(CancellationToken cancellationToken = default)
            => DatabaseCreator.EnsureDeletedAsync(cancellationToken);

        /// <summary>
        ///     <para>
        ///         Determines whether or not the database is available and can be connected to.
        ///     </para>
        ///     <para>
        ///         Note that being able to connect to the database does not mean that it is
        ///         up-to-date with regard to schema creation, etc.
        ///     </para>
        /// </summary>
        /// <returns> <c>True</c> if the database is available; <c>false</c> otherwise. </returns>
        public virtual bool CanConnect()
            => DatabaseCreator is IDatabaseCreatorWithCanConnect withCanConnect
                ? withCanConnect.CanConnect()
                : throw new NotImplementedException(CoreStrings.CanConnectNotImplemented);

        /// <summary>
        ///     <para>
        ///         Determines whether or not the database is available and can be connected to.
        ///     </para>
        ///     <para>
        ///         Note that being able to connect to the database does not mean that it is
        ///         up-to-date with regard to schema creation, etc.
        ///     </para>
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
        /// <returns> <c>True</c> if the database is available; <c>false</c> otherwise. </returns>
        public virtual Task<bool> CanConnectAsync(CancellationToken cancellationToken = default)
            => DatabaseCreator is IDatabaseCreatorWithCanConnect withCanConnect
                ? withCanConnect.CanConnectAsync(cancellationToken)
                : throw new NotImplementedException(CoreStrings.CanConnectNotImplemented);

        /// <summary>
        ///     Starts a new transaction.
        /// </summary>
        /// <returns>
        ///     A <see cref="IDbContextTransaction" /> that represents the started transaction.
        /// </returns>
        public virtual IDbContextTransaction BeginTransaction()
            => TransactionManager.BeginTransaction();

        /// <summary>
        ///     Asynchronously starts a new transaction.
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
        /// <returns>
        ///     A task that represents the asynchronous transaction initialization. The task result contains a <see cref="IDbContextTransaction" />
        ///     that represents the started transaction.
        /// </returns>
        public virtual Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
            => TransactionManager.BeginTransactionAsync(cancellationToken);

        /// <summary>
        ///     Applies the outstanding operations in the current transaction to the database.
        /// </summary>
        public virtual void CommitTransaction()
            => TransactionManager.CommitTransaction();

        /// <summary>
        ///     Discards the outstanding operations in the current transaction.
        /// </summary>
        public virtual void RollbackTransaction()
            => TransactionManager.RollbackTransaction();

        /// <summary>
        ///     Creates an instance of the configured <see cref="IExecutionStrategy" />.
        /// </summary>
        /// <returns>An <see cref="IExecutionStrategy" /> instance.</returns>
        public virtual IExecutionStrategy CreateExecutionStrategy()
            => ExecutionStrategyFactory.Create();

        /// <summary>
        ///     <para>
        ///         Gets the current <see cref="IDbContextTransaction" /> being used by the context, or null
        ///         if no transaction is in use.
        ///     </para>
        ///     <para>
        ///         This property will be null unless one of the 'BeginTransaction' or 'UseTransaction' methods has
        ///         been called, some of which are available as extension methods installed by EF providers.
        ///         No attempt is made to obtain a transaction from the current DbConnection or similar.
        ///     </para>
        ///     <para>
        ///         For relational databases, the underlying DbTransaction can be obtained using the
        ///         'Microsoft.EntityFrameworkCore.Storage.GetDbTransaction'extension method
        ///         on the returned <see cref="IDbContextTransaction" />.
        ///     </para>
        /// </summary>
        public virtual IDbContextTransaction CurrentTransaction
            => TransactionManager.CurrentTransaction;

        /// <summary>
        ///     <para>
        ///         Gets or sets a value indicating whether or not a transaction will be created
        ///         automatically by <see cref="DbContext.SaveChanges()" /> if none of the
        ///         'BeginTransaction' or 'UseTransaction' methods have been called.
        ///     </para>
        ///     <para>
        ///         Setting this value to false will also disable the <see cref="IExecutionStrategy" />
        ///         for <see cref="DbContext.SaveChanges()" />
        ///     </para>
        ///     <para>
        ///         The default value is true, meaning that SaveChanges will always use a transaction
        ///         when saving changes.
        ///     </para>
        ///     <para>
        ///         Setting this value to false should only be done with caution since the database
        ///         could be left in a corrupted state if SaveChanges fails.
        ///     </para>
        /// </summary>
        public virtual bool AutoTransactionsEnabled { get; set; } = true;

        /// <summary>
        ///     <para>
        ///         Returns the name of the database provider currently in use.
        ///         The name is typically the name of the provider assembly.
        ///         It is usually easier to use a sugar method such as 'IsSqlServer()' instead of
        ///         calling this method directly.
        ///     </para>
        ///     <para>
        ///         This method can only be used after the <see cref="DbContext" /> has been configured because
        ///         it is only then that the provider is known. This means that this method cannot be used
        ///         in <see cref="DbContext.OnConfiguring" /> because this is where application code sets the
        ///         provider to use as part of configuring the context.
        ///     </para>
        /// </summary>
        public virtual string ProviderName
            => _context.GetService<IEnumerable<IDatabaseProvider>>()
                ?.Select(p => p.Name)
                .FirstOrDefault();

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

        private IDbContextTransactionManager TransactionManager
            => _transactionManager ?? (_transactionManager = this.GetService<IDbContextTransactionManager>());

        private IDatabaseCreator DatabaseCreator
            => _databaseCreator ?? (_databaseCreator = this.GetService<IDatabaseCreator>());

        private IExecutionStrategyFactory ExecutionStrategyFactory
            => _executionStrategyFactory ?? (_executionStrategyFactory = this.GetService<IExecutionStrategyFactory>());

        #region Hidden System.Object members

        /// <summary>
        ///     Returns a string that represents the current object.
        /// </summary>
        /// <returns> A string that represents the current object. </returns>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override string ToString() => base.ToString();

        /// <summary>
        ///     Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="obj"> The object to compare with the current object. </param>
        /// <returns> true if the specified object is equal to the current object; otherwise, false. </returns>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override bool Equals(object obj) => base.Equals(obj);

        /// <summary>
        ///     Serves as the default hash function.
        /// </summary>
        /// <returns> A hash code for the current object. </returns>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override int GetHashCode() => base.GetHashCode();

        #endregion
    }
}
