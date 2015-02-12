// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.Entity.ChangeTracking.Internal;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Relational.Update;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.Logging;
using Moq;
using Xunit;

namespace Microsoft.Data.Entity.Relational.Tests
{
    public class RelationalDataStoreTest
    {
        [Fact]
        public async Task SaveChangesAsync_delegates()
        {
            var relationalConnectionMock = new Mock<RelationalConnection>();
            var commandBatchPreparerMock = new Mock<CommandBatchPreparer>();
            var batchExecutorMock = new Mock<BatchExecutor>();

            var customServices = new ServiceCollection()
                .AddInstance(relationalConnectionMock.Object)
                .AddInstance(commandBatchPreparerMock.Object)
                .AddInstance(batchExecutorMock.Object)
                .AddSingleton<FakeRelationalDataStore>();

            var contextServices = RelationalTestHelpers.Instance.CreateContextServices(customServices);

            var relationalDataStore = contextServices.GetRequiredService<FakeRelationalDataStore>();

            var entries = new List<InternalEntityEntry>();
            var cancellationToken = new CancellationTokenSource().Token;

            await relationalDataStore.SaveChangesAsync(entries, cancellationToken);

            commandBatchPreparerMock.Verify(c => c.BatchCommands(entries, relationalDataStore.DbContextOptions));
            batchExecutorMock.Verify(be => be.ExecuteAsync(It.IsAny<IEnumerable<ModificationCommandBatch>>(), relationalConnectionMock.Object, cancellationToken));
        }

        [Fact]
        public void SaveChanges_delegates()
        {
            var relationalConnectionMock = new Mock<RelationalConnection>();
            var commandBatchPreparerMock = new Mock<CommandBatchPreparer>();
            var batchExecutorMock = new Mock<BatchExecutor>();

            var customServices = new ServiceCollection()
                .AddInstance(relationalConnectionMock.Object)
                .AddInstance(commandBatchPreparerMock.Object)
                .AddInstance(batchExecutorMock.Object)
                .AddSingleton<FakeRelationalDataStore>();

            var contextServices = RelationalTestHelpers.Instance.CreateContextServices(customServices);

            var relationalDataStore = contextServices.GetRequiredService<FakeRelationalDataStore>();

            var entries = new List<InternalEntityEntry>();

            relationalDataStore.SaveChanges(entries);

            commandBatchPreparerMock.Verify(c => c.BatchCommands(entries, relationalDataStore.DbContextOptions));
            batchExecutorMock.Verify(be => be.Execute(It.IsAny<IEnumerable<ModificationCommandBatch>>(), relationalConnectionMock.Object));
        }

        private class FakeRelationalDataStore : RelationalDataStore
        {
            public FakeRelationalDataStore(
                StateManager stateManager,
                DbContextService<IModel> model,
                EntityKeyFactorySource entityKeyFactorySource,
                EntityMaterializerSource entityMaterializerSource,
                ClrCollectionAccessorSource collectionAccessorSource,
                ClrPropertySetterSource propertySetterSource,
                RelationalConnection connection,
                CommandBatchPreparer batchPreparer,
                BatchExecutor batchExecutor,
                DbContextService<IDbContextOptions> options,
                ILoggerFactory loggerFactory)
                : base(stateManager, model, entityKeyFactorySource, entityMaterializerSource,
                    collectionAccessorSource, propertySetterSource, connection, batchPreparer, batchExecutor, options,
                    loggerFactory)
            {
            }
        }
    }
}
