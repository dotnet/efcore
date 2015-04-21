// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.Entity.ChangeTracking.Internal;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Metadata.Internal;
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
            var relationalConnectionMock = new Mock<IRelationalConnection>();
            var commandBatchPreparerMock = new Mock<ICommandBatchPreparer>();
            var batchExecutorMock = new Mock<IBatchExecutor>();
            var valueReaderMock = new Mock<IRelationalValueReaderFactoryFactory>();

            var customServices = new ServiceCollection()
                .AddInstance(relationalConnectionMock.Object)
                .AddInstance(commandBatchPreparerMock.Object)
                .AddInstance(batchExecutorMock.Object)
                .AddInstance(valueReaderMock.Object)
                .AddScoped<FakeRelationalDataStore>();

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
            var relationalConnectionMock = new Mock<IRelationalConnection>();
            var commandBatchPreparerMock = new Mock<ICommandBatchPreparer>();
            var batchExecutorMock = new Mock<IBatchExecutor>();
            var valueReaderMock = new Mock<IRelationalValueReaderFactoryFactory>();

            var customServices = new ServiceCollection()
                .AddInstance(relationalConnectionMock.Object)
                .AddInstance(commandBatchPreparerMock.Object)
                .AddInstance(batchExecutorMock.Object)
                .AddInstance(valueReaderMock.Object)
                .AddScoped<FakeRelationalDataStore>();

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
                IModel model,
                IEntityKeyFactorySource entityKeyFactorySource,
                IEntityMaterializerSource entityMaterializerSource,
                IRelationalConnection connection,
                ICommandBatchPreparer batchPreparer,
                IBatchExecutor batchExecutor,
                IDbContextOptions options,
                ILoggerFactory loggerFactory,
                IRelationalValueReaderFactoryFactory valueReaderFactoryFactory)
                : base(
                      model, 
                      entityKeyFactorySource, 
                      entityMaterializerSource, 
                      connection, 
                      batchPreparer, 
                      batchExecutor, 
                      options, 
                      loggerFactory,
                      valueReaderFactoryFactory)
            {
            }
        }
    }
}
