// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.TestUtilities.FakeProvider;
using Microsoft.EntityFrameworkCore.Update;
using Microsoft.EntityFrameworkCore.Update.Internal;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.TestUtilities
{
    public class RelationalTestHelpers : TestHelpers
    {
        protected RelationalTestHelpers()
        {
        }

        public static RelationalTestHelpers Instance { get; } = new RelationalTestHelpers();

        public ICommandBatchPreparer CreateCommandBatchPreparer(
            IModificationCommandBatchFactory modificationCommandBatchFactory = null,
            IStateManager stateManager = null,
            bool sensitiveLogging = false)
        {
            modificationCommandBatchFactory =
                modificationCommandBatchFactory
                ?? Instance.CreateContextServices().GetRequiredService<IModificationCommandBatchFactory>();

            stateManager = stateManager
                ?? Instance.CreateContextServices().GetRequiredService<IStateManager>();

            var loggingOptions = new LoggingOptions();
            if (sensitiveLogging)
            {
                loggingOptions.Initialize(new DbContextOptionsBuilder<DbContext>().EnableSensitiveDataLogging().Options);
            }

            return new CommandBatchPreparer(new CommandBatchPreparerDependencies(
                modificationCommandBatchFactory,
                new ParameterNameGeneratorFactory(new ParameterNameGeneratorDependencies()),
                new ModificationCommandComparer(),
                new KeyValueIndexFactorySource(),
                () => stateManager,
                loggingOptions));
        }

        public override IServiceCollection AddProviderServices(IServiceCollection services)
            => FakeRelationalOptionsExtension.AddEntityFrameworkRelationalDatabase(services);

        protected override void UseProviderOptions(DbContextOptionsBuilder optionsBuilder)
        {
            var extension = optionsBuilder.Options.FindExtension<FakeRelationalOptionsExtension>()
                            ?? new FakeRelationalOptionsExtension();

            ((IDbContextOptionsBuilderInfrastructure)optionsBuilder).AddOrUpdateExtension(
                extension.WithConnection(new FakeDbConnection("Database=Fake")));
        }
    }
}
