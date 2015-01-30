// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.Entity.ChangeTracking;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Query;
using Microsoft.Data.Entity.Relational.Update;
using Microsoft.Data.Entity.Storage;
using Microsoft.Data.Entity.Tests;
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

            var stateEntries = new List<StateEntry>();
            var cancellationToken = new CancellationTokenSource().Token;

            await relationalDataStore.SaveChangesAsync(stateEntries, cancellationToken);

            commandBatchPreparerMock.Verify(c => c.BatchCommands(stateEntries, relationalDataStore.DbContextOptions));
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

            var stateEntries = new List<StateEntry>();

            relationalDataStore.SaveChanges(stateEntries);

            commandBatchPreparerMock.Verify(c => c.BatchCommands(stateEntries, relationalDataStore.DbContextOptions));
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
                ILoggerFactory loggerFactory,
                ICompiledQueryCache compiledQueryCache)
                : base(stateManager, model, entityKeyFactorySource, entityMaterializerSource,
                    collectionAccessorSource, propertySetterSource, connection, batchPreparer, batchExecutor, options, 
                    loggerFactory, compiledQueryCache)
            {
            }
        }
    }
}
