// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Relational.Model;
using Microsoft.Data.Entity.Relational.Utilities;

namespace Microsoft.Data.Entity.Relational.Update
{
    public class BatchExecutor
    {
        private readonly RelationalTypeMapper _typeMapper;

        /// <summary>
        ///     This constructor is intended only for use when creating test doubles that will override members
        ///     with mocked or faked behavior. Use of this constructor for other purposes may result in unexpected
        ///     behavior including but not limited to throwing <see cref="NullReferenceException" />.
        /// </summary>
        protected BatchExecutor()
        {
        }

        public BatchExecutor(
            [NotNull] RelationalTypeMapper typeMapper)
        {
            Check.NotNull(typeMapper, "typeMapper");

            _typeMapper = typeMapper;
        }

        public virtual async Task<int> ExecuteAsync(
            [NotNull] IEnumerable<ModificationCommandBatch> commandBatches,
            [NotNull] RelationalConnection connection,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            Check.NotNull(commandBatches, "commandBatches");
            Check.NotNull(connection, "connection");

            var rowsAffected = 0;
            await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                foreach (var commandbatch in commandBatches)
                {
                    rowsAffected += await commandbatch.ExecuteAsync(connection, _typeMapper, cancellationToken).ConfigureAwait(false);
                }
            }
            finally
            {
                connection.Close();
            }

            return rowsAffected;
        }
    }
}
