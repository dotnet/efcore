// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.Update
{
    /// <summary>
    ///     A <see cref="ReaderModificationCommandBatch" /> for providers which append an SQL query to find out
    ///     how many rows were affected (see <see cref="UpdateSqlGenerator.AppendSelectAffectedCountCommand" />).
    /// </summary>
    public abstract class AffectedCountModificationCommandBatch : ReaderModificationCommandBatch
    {
        protected AffectedCountModificationCommandBatch(
            [NotNull] IRelationalCommandBuilderFactory commandBuilderFactory,
            [NotNull] ISqlGenerationHelper sqlGenerationHelper,
            [NotNull] IUpdateSqlGenerator updateSqlGenerator,
            [NotNull] IRelationalValueBufferFactoryFactory valueBufferFactoryFactory)
            : base(commandBuilderFactory, sqlGenerationHelper, updateSqlGenerator, valueBufferFactoryFactory)
        {
        }

        protected override void Consume(DbDataReader reader)
        {
            Debug.Assert(CommandResultSet.Count == ModificationCommands.Count);
            var commandIndex = 0;

            try
            {
                var actualResultSetCount = 0;
                do
                {
                    while (commandIndex < CommandResultSet.Count
                           && CommandResultSet[commandIndex] == ResultSetMapping.NoResultSet)
                    {
                        commandIndex++;
                    }

                    if (commandIndex < CommandResultSet.Count)
                    {
                        commandIndex = ModificationCommands[commandIndex].RequiresResultPropagation
                            ? ConsumeResultSetWithPropagation(commandIndex, reader)
                            : ConsumeResultSetWithoutPropagation(commandIndex, reader);
                        actualResultSetCount++;
                    }
                }
                while (commandIndex < CommandResultSet.Count
                       && reader.NextResult());

#if DEBUG
                while (commandIndex < CommandResultSet.Count
                       && CommandResultSet[commandIndex] == ResultSetMapping.NoResultSet)
                {
                    commandIndex++;
                }

                Debug.Assert(commandIndex == ModificationCommands.Count,
                    "Expected " + ModificationCommands.Count + " results, got " + commandIndex);

                var expectedResultSetCount = CommandResultSet.Count(e => e == ResultSetMapping.LastInResultSet);

                Debug.Assert(actualResultSetCount == expectedResultSetCount,
                    "Expected " + expectedResultSetCount + " result sets, got " + actualResultSetCount);
#endif
            }
            catch (DbUpdateException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new DbUpdateException(
                    RelationalStrings.UpdateStoreException,
                    ex,
                    ModificationCommands[commandIndex].Entries);
            }
        }

        protected override async Task ConsumeAsync(
            DbDataReader reader,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            Debug.Assert(CommandResultSet.Count == ModificationCommands.Count);
            var commandIndex = 0;

            try
            {
                var actualResultSetCount = 0;
                do
                {
                    while (commandIndex < CommandResultSet.Count
                           && CommandResultSet[commandIndex] == ResultSetMapping.NoResultSet)
                    {
                        commandIndex++;
                    }

                    if (commandIndex < CommandResultSet.Count)
                    {
                        commandIndex = ModificationCommands[commandIndex].RequiresResultPropagation
                            ? await ConsumeResultSetWithPropagationAsync(commandIndex, reader, cancellationToken)
                            : await ConsumeResultSetWithoutPropagationAsync(commandIndex, reader, cancellationToken);
                        actualResultSetCount++;
                    }
                }
                while (commandIndex < CommandResultSet.Count
                       && await reader.NextResultAsync(cancellationToken));

#if DEBUG
                while (commandIndex < CommandResultSet.Count
                       && CommandResultSet[commandIndex] == ResultSetMapping.NoResultSet)
                {
                    commandIndex++;
                }

                Debug.Assert(commandIndex == ModificationCommands.Count,
                    "Expected " + ModificationCommands.Count + " results, got " + commandIndex);

                var expectedResultSetCount = CommandResultSet.Count(e => e == ResultSetMapping.LastInResultSet);

                Debug.Assert(actualResultSetCount == expectedResultSetCount,
                    "Expected " + expectedResultSetCount + " result sets, got " + actualResultSetCount);
#endif
            }
            catch (DbUpdateException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new DbUpdateException(
                    RelationalStrings.UpdateStoreException,
                    ex,
                    ModificationCommands[commandIndex].Entries);
            }
        }

        protected virtual int ConsumeResultSetWithPropagation(int commandIndex, [NotNull] DbDataReader reader)
        {
            var rowsAffected = 0;
            do
            {
                var tableModification = ModificationCommands[commandIndex];
                Debug.Assert(tableModification.RequiresResultPropagation);

                if (!reader.Read())
                {
                    var expectedRowsAffected = rowsAffected + 1;
                    while ((++commandIndex < CommandResultSet.Count)
                           && CommandResultSet[commandIndex - 1] == ResultSetMapping.NotLastInResultSet)
                    {
                        expectedRowsAffected++;
                    }

                    ThrowAggregateUpdateConcurrencyException(commandIndex, expectedRowsAffected, rowsAffected);
                }

                var valueBufferFactory = CreateValueBufferFactory(tableModification.ColumnModifications);

                tableModification.PropagateResults(valueBufferFactory.Create(reader));
                rowsAffected++;
            }
            while ((++commandIndex < CommandResultSet.Count)
                   && CommandResultSet[commandIndex - 1] == ResultSetMapping.NotLastInResultSet);

            return commandIndex;
        }

        protected virtual async Task<int> ConsumeResultSetWithPropagationAsync(
            int commandIndex, [NotNull] DbDataReader reader, CancellationToken cancellationToken)
        {
            var rowsAffected = 0;
            do
            {
                var tableModification = ModificationCommands[commandIndex];
                Debug.Assert(tableModification.RequiresResultPropagation);

                if (!await reader.ReadAsync(cancellationToken))
                {
                    var expectedRowsAffected = rowsAffected + 1;
                    while ((++commandIndex < CommandResultSet.Count)
                           && CommandResultSet[commandIndex - 1] == ResultSetMapping.NotLastInResultSet)
                    {
                        expectedRowsAffected++;
                    }

                    ThrowAggregateUpdateConcurrencyException(commandIndex, expectedRowsAffected, rowsAffected);
                }

                var valueBufferFactory = CreateValueBufferFactory(tableModification.ColumnModifications);

                tableModification.PropagateResults(valueBufferFactory.Create(reader));
                rowsAffected++;
            }
            while ((++commandIndex < CommandResultSet.Count)
                   && CommandResultSet[commandIndex - 1] == ResultSetMapping.NotLastInResultSet);

            return commandIndex;
        }

        protected virtual int ConsumeResultSetWithoutPropagation(int commandIndex, [NotNull] DbDataReader reader)
        {
            var expectedRowsAffected = 1;
            while ((++commandIndex < CommandResultSet.Count)
                   && CommandResultSet[commandIndex - 1] == ResultSetMapping.NotLastInResultSet)
            {
                Debug.Assert(!ModificationCommands[commandIndex].RequiresResultPropagation);

                expectedRowsAffected++;
            }

            if (reader.Read())
            {
                var rowsAffected = reader.GetInt32(0);
                if (rowsAffected != expectedRowsAffected)
                {
                    ThrowAggregateUpdateConcurrencyException(commandIndex, expectedRowsAffected, rowsAffected);
                }
            }
            else
            {
                ThrowAggregateUpdateConcurrencyException(commandIndex, 1, 0);
            }

            return commandIndex;
        }

        protected virtual async Task<int> ConsumeResultSetWithoutPropagationAsync(
            int commandIndex, [NotNull] DbDataReader reader, CancellationToken cancellationToken)
        {
            var expectedRowsAffected = 1;
            while ((++commandIndex < CommandResultSet.Count)
                   && CommandResultSet[commandIndex - 1] == ResultSetMapping.NotLastInResultSet)
            {
                Debug.Assert(!ModificationCommands[commandIndex].RequiresResultPropagation);

                expectedRowsAffected++;
            }

            if (await reader.ReadAsync(cancellationToken))
            {
                var rowsAffected = reader.GetInt32(0);
                if (rowsAffected != expectedRowsAffected)
                {
                    ThrowAggregateUpdateConcurrencyException(commandIndex, expectedRowsAffected, rowsAffected);
                }
            }
            else
            {
                ThrowAggregateUpdateConcurrencyException(commandIndex, 1, 0);
            }

            return commandIndex;
        }

        private IReadOnlyList<IUpdateEntry> AggregateEntries(int endIndex, int commandCount)
        {
            var entries = new List<IUpdateEntry>();
            for (var i = endIndex - commandCount; i < endIndex; i++)
            {
                entries.AddRange(ModificationCommands[i].Entries);
            }
            return entries;
        }

        protected virtual void ThrowAggregateUpdateConcurrencyException(
            int commandIndex,
            int expectedRowsAffected,
            int rowsAffected)
        {
            throw new DbUpdateConcurrencyException(
                RelationalStrings.UpdateConcurrencyException(expectedRowsAffected, rowsAffected),
                AggregateEntries(commandIndex, expectedRowsAffected));
        }
    }
}
