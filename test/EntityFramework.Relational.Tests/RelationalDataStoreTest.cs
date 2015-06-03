// Copyright (c) .NET Foundation. All rights reserved.
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
using Microsoft.Data.Entity.Relational.Query.Methods;

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
            var valueBufferMock = new Mock<IRelationalValueBufferFactoryFactory>();
            var methodCallTranslatorMock = new Mock<IMethodCallTranslator>();
            var memberTranslatorMock = new Mock<IMemberTranslator>();

            var customServices = new ServiceCollection()
                .AddInstance(relationalConnectionMock.Object)
                .AddInstance(commandBatchPreparerMock.Object)
                .AddInstance(batchExecutorMock.Object)
                .AddInstance(valueBufferMock.Object)
                .AddInstance(methodCallTranslatorMock.Object)
                .AddInstance(memberTranslatorMock.Object)
                .AddScoped<FakeRelationalDataStore>();

            var contextServices = RelationalTestHelpers.Instance.CreateContextServices(customServices);

            var relationalDataStore = contextServices.GetRequiredService<FakeRelationalDataStore>();

            var entries = new List<InternalEntityEntry>();
            var cancellationToken = new CancellationTokenSource().Token;

            await relationalDataStore.SaveChangesAsync(entries, cancellationToken);

            commandBatchPreparerMock.Verify(c => c.BatchCommands(entries, relationalDataStore.EntityOptions));
            batchExecutorMock.Verify(be => be.ExecuteAsync(It.IsAny<IEnumerable<ModificationCommandBatch>>(), relationalConnectionMock.Object, cancellationToken));
        }

        [Fact]
        public void SaveChanges_delegates()
        {
            var relationalConnectionMock = new Mock<IRelationalConnection>();
            var commandBatchPreparerMock = new Mock<ICommandBatchPreparer>();
            var batchExecutorMock = new Mock<IBatchExecutor>();
            var valueBufferMock = new Mock<IRelationalValueBufferFactoryFactory>();
            var methodCallTranslatorMock = new Mock<IMethodCallTranslator>();
            var memberTranslatorMock = new Mock<IMemberTranslator>();

            var customServices = new ServiceCollection()
                .AddInstance(relationalConnectionMock.Object)
                .AddInstance(commandBatchPreparerMock.Object)
                .AddInstance(batchExecutorMock.Object)
                .AddInstance(valueBufferMock.Object)
                .AddInstance(methodCallTranslatorMock.Object)
                .AddInstance(memberTranslatorMock.Object)
                .AddScoped<FakeRelationalDataStore>();

            var contextServices = RelationalTestHelpers.Instance.CreateContextServices(customServices);

            var relationalDataStore = contextServices.GetRequiredService<FakeRelationalDataStore>();

            var entries = new List<InternalEntityEntry>();

            relationalDataStore.SaveChanges(entries);

            commandBatchPreparerMock.Verify(c => c.BatchCommands(entries, relationalDataStore.EntityOptions));
            batchExecutorMock.Verify(be => be.Execute(It.IsAny<IEnumerable<ModificationCommandBatch>>(), relationalConnectionMock.Object));
        }

        private class FakeRelationalDataStore : RelationalDataStore
        {
            public FakeRelationalDataStore(
                IModel model,
                IEntityKeyFactorySource entityKeyFactorySource,
                IEntityMaterializerSource entityMaterializerSource,
                IClrAccessorSource<IClrPropertyGetter> clrPropertyGetterSource,
                IRelationalConnection connection,
                ICommandBatchPreparer batchPreparer,
                IBatchExecutor batchExecutor,
                IEntityOptions options,
                ILoggerFactory loggerFactory,
                IRelationalValueBufferFactoryFactory valueBufferFactoryFactory,
                IMethodCallTranslator compositeMethodCallTranslator,
                IMemberTranslator compositeMemberTranslator)
                : base(
                      model, 
                      entityKeyFactorySource, 
                      entityMaterializerSource,
                      clrPropertyGetterSource,
                      connection, 
                      batchPreparer, 
                      batchExecutor, 
                      options, 
                      loggerFactory,
                      valueBufferFactoryFactory,
                      compositeMethodCallTranslator,
                      compositeMemberTranslator)
            {
            }
        }
    }
}
