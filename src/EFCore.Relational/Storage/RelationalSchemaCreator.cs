// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Storage
{
    /// <summary>
    ///     <para>
    ///         Performs schema creation.
    ///     </para>
    ///     <para>
    ///         This type is typically used by schema providers (and other extensions). It is generally
    ///         not used in application code.
    ///     </para>
    /// </summary>
    public abstract class RelationalSchemaCreator : IRelationalSchemaCreator
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="RelationalSchemaCreator" /> class.
        /// </summary>
        /// <param name="dependencies"> Parameter object containing dependencies for this service. </param>
        protected RelationalSchemaCreator([NotNull] RelationalSchemaCreatorDependencies dependencies)
        {
            Check.NotNull(dependencies, nameof(dependencies));

            Dependencies = dependencies;
        }

        /// <summary>
        ///     Parameter object containing service dependencies.
        /// </summary>
        protected virtual RelationalSchemaCreatorDependencies Dependencies { get; }

        /// <summary>
        ///     Determines whether the schema exists.
        /// </summary>
        /// <returns>
        ///     True if the schema exists; otherwise false.
        /// </returns>
        public abstract bool Exists();

        /// <summary>
        ///     Asynchronously determines whether the physical schema exists.
        /// </summary>
        /// <param name="cancellationToken">
        ///     A <see cref="CancellationToken" /> to observe while waiting for the task to complete.
        /// </param>
        /// <returns>
        ///     A task that represents the asynchronous operation. The task result contains
        ///     true if the schema exists; otherwise false.
        /// </returns>
        public virtual Task<bool> ExistsAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();

            return Task.FromResult(Exists());
        }

        /// <summary>
        ///     Creates the physical schema. Does not attempt to populate it with any schema.
        /// </summary>
        public abstract void Create();

        /// <summary>
        ///     Asynchronously creates the schema.
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
        /// <returns>
        ///     A task that represents the asynchronous operation.
        /// </returns>
        public virtual Task CreateAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();

            Create();

            return Task.FromResult(0);
        }

        /// <summary>
        ///     Deletes the schema.
        /// </summary>
        public abstract void Delete();

        /// <summary>
        ///     Asynchronously deletes the schema.
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
        /// <returns>
        ///     A task that represents the asynchronous operation.
        /// </returns>
        public virtual Task DeleteAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();

            Delete();

            return Task.FromResult(0);
        }
        
        /// <summary>
        ///     <para>
        ///         Ensures that the schema for the context does not exist. If it does not exist, no action is taken. If it does
        ///         exist then the schema is deleted.
        ///     </para>
        /// </summary>
        /// <returns>
        ///     True if the schema is deleted, false if it did not exist.
        /// </returns>
        public virtual bool EnsureDeleted()
        {
            if (Exists())
            {
                Delete();
                return true;
            }
            return false;
        }

        /// <summary>
        ///     <para>
        ///         Asynchronously ensures that the schema for the context does not exist. If it does not exist, no action is taken. If it does
        ///         exist then the schema is deleted.
        ///     </para>
        /// </summary>
        /// <param name="cancellationToken">A <see cref="T:System.Threading.CancellationToken" /> to observe while waiting for the task to complete.</param>
        /// <returns>
        ///     A task that represents the asynchronous save operation. The task result contains true if the schema is deleted,
        ///     false if it did not exist.
        /// </returns>
        public virtual async Task<bool> EnsureDeletedAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            if (await ExistsAsync(cancellationToken))
            {
                await DeleteAsync(cancellationToken);

                return true;
            }
            return false;
        }

        /// <summary>
        ///     Ensures that the schema for the context exists. If it exists, no action is taken. If it does not
        ///     exist then the schema is created. If the schema exists, then no effort is made
        ///     to ensure it is compatible with the model for this context.
        /// </summary>
        /// <returns>
        ///     True if the schema is created, false if it already existed.
        /// </returns>
        public virtual bool EnsureCreated()
        {
            if (!Exists())
            {
                Create();
                return true;
            }
            
            return false;
        }

        /// <summary>
        ///     Asynchronously ensures that the schema for the context exists. If it exists, no action is taken. If it does not
        ///     exist then the schema is created. If the schema exists, then no effort is made
        ///     to ensure it is compatible with the model for this context.
        /// </summary>
        /// <param name="cancellationToken">A <see cref="T:System.Threading.CancellationToken" /> to observe while waiting for the task to complete.</param>
        /// <returns>
        ///     A task that represents the asynchronous save operation. The task result contains true if the schema is created,
        ///     false if it already existed.
        /// </returns>
        public virtual async Task<bool> EnsureCreatedAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            if (!await ExistsAsync(cancellationToken))
            {
                await CreateAsync(cancellationToken);
                return true;
            }

            return false;
        }
    }
}
