// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.Entity.ChangeTracking.Internal;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Query.ExpressionTranslators;
using Microsoft.Data.Entity.Storage;
using Microsoft.Data.Entity.Update;
using Microsoft.Framework.DependencyInjection;
using Moq;
using Xunit;

namespace Microsoft.Data.Entity.Tests
{
    public class RelationalDatabaseTest
    {
        [Fact]
        public async Task SaveChangesAsync_delegates()
        {
            var commandBatchPreparerMock = new Mock<ICommandBatchPreparer>();
            var batchExecutorMock = new Mock<IBatchExecutor>();
            var relationalConnectionMock = new Mock<IRelationalConnection>();
            var fragmentTranslatorMock = new Mock<IExpressionFragmentTranslator>();

            var customServices = new ServiceCollection()
                .AddInstance(commandBatchPreparerMock.Object)
                .AddInstance(batchExecutorMock.Object)
                .AddInstance(relationalConnectionMock.Object)
                .AddInstance(fragmentTranslatorMock.Object)
                .AddScoped<RelationalDatabase>();


            var contextServices = RelationalTestHelpers.Instance.CreateContextServices(customServices);

            var relationalDatabase = contextServices.GetRequiredService<RelationalDatabase>();

            var entries = new List<InternalEntityEntry>();
            var cancellationToken = new CancellationTokenSource().Token;

            await relationalDatabase.SaveChangesAsync(entries, cancellationToken);

            commandBatchPreparerMock.Verify(c => c.BatchCommands(entries, contextServices.GetService<IDbContextOptions>()));
            batchExecutorMock.Verify(be => be.ExecuteAsync(It.IsAny<IEnumerable<ModificationCommandBatch>>(), relationalConnectionMock.Object, cancellationToken));
        }

        [Fact]
        public void SaveChanges_delegates()
        {
            var commandBatchPreparerMock = new Mock<ICommandBatchPreparer>();
            var batchExecutorMock = new Mock<IBatchExecutor>();
            var relationalConnectionMock = new Mock<IRelationalConnection>();

            var fragmentTranslatorMock = new Mock<IExpressionFragmentTranslator>();

            var customServices = new ServiceCollection()
                .AddInstance(commandBatchPreparerMock.Object)
                .AddInstance(batchExecutorMock.Object)
                .AddInstance(relationalConnectionMock.Object)
                .AddInstance(fragmentTranslatorMock.Object)
                .AddScoped<RelationalDatabase>();

            var contextServices = RelationalTestHelpers.Instance.CreateContextServices(customServices);

            var relationalDatabase = contextServices.GetRequiredService<RelationalDatabase>();

            var entries = new List<InternalEntityEntry>();

            relationalDatabase.SaveChanges(entries);

            commandBatchPreparerMock.Verify(c => c.BatchCommands(entries, contextServices.GetService<IDbContextOptions>()));
            batchExecutorMock.Verify(be => be.Execute(It.IsAny<IEnumerable<ModificationCommandBatch>>(), relationalConnectionMock.Object));
        }
    }
}
