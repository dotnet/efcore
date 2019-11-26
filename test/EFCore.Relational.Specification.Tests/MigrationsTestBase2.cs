// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Scaffolding;
using Microsoft.EntityFrameworkCore.Scaffolding.Metadata;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Microsoft.EntityFrameworkCore
{
    public class MigrationsTestBase2<TFixture> : IClassFixture<TFixture>
        where TFixture : MigrationsTestBase2<TFixture>.MigrationsFixtureBase2, new()
    {
        protected TFixture Fixture { get; }

        protected MigrationsTestBase2(TFixture fixture)
        {
            Fixture = fixture;
        }

        [ConditionalFact]
        public virtual void CreateIndexOperation_unique()
            => ExecuteIncremental(
                modelBuilder =>
                {
                    modelBuilder.Entity(
                        "People", entityBuilder =>
                        {
                            entityBuilder.Property<int>("Id");
                            entityBuilder.Property<string>("FirstName");
                            entityBuilder.Property<string>("LastName");
                        });
                },
                modelBuilder => modelBuilder.Entity("People").HasIndex("FirstName", "LastName").IsUnique(),
                model => Assert.True(model.Tables.Single().Indexes.Single().IsUnique));

        protected virtual void ExecuteIncremental(
            Action<ModelBuilder> buildSourceAction,
            Action<ModelBuilder> buildTargetIncrementalAction,
            Action<DatabaseModel> modelAsserter)
            => Execute(
                buildSourceAction,
                modelBuilder =>
                {
                    buildSourceAction(modelBuilder);
                    buildTargetIncrementalAction(modelBuilder);
                },
                modelAsserter);

        protected virtual void Execute(
            Action<ModelBuilder> buildSourceAction,
            Action<ModelBuilder> buildTargetAction,
            Action<DatabaseModel> modelAsserter)
        {
            var context = Fixture.CreateContext();
            var serviceProvider = ((IInfrastructure<IServiceProvider>)context).Instance;
            var migrationsSqlGenerator = serviceProvider.GetRequiredService<IMigrationsSqlGenerator>();
            var modelDiffer = serviceProvider.GetRequiredService<IMigrationsModelDiffer>();
            var migrationsCommandExecutor = serviceProvider.GetRequiredService<IMigrationCommandExecutor>();
            var connection = serviceProvider.GetRequiredService<IRelationalConnection>();
            var databaseModelFactory = serviceProvider.GetRequiredService<IDatabaseModelFactory>();

            // Build the source and target models
            var sourceModelBuilder = Fixture.TestHelpers.CreateConventionBuilder(skipValidation: true);
            buildSourceAction(sourceModelBuilder);
            var sourceModel = sourceModelBuilder.FinalizeModel();

            var targetModelBuilder = Fixture.TestHelpers.CreateConventionBuilder(skipValidation: true);
            buildTargetAction(targetModelBuilder);
            var targetModel = targetModelBuilder.FinalizeModel();

            // Apply migrations to get to the source state, and do a scaffolding snapshot for later comparison
            migrationsCommandExecutor.ExecuteNonQuery(
                migrationsSqlGenerator.Generate(modelDiffer.GetDifferences(null, sourceModel), sourceModel),
                connection);
            var sourceScaffoldedModel = databaseModelFactory.Create(
                context.Database.GetDbConnection(),
                new DatabaseModelFactoryOptions());

            try
            {
                // Apply migrations to get from source to target
                migrationsCommandExecutor.ExecuteNonQuery(
                    migrationsSqlGenerator.Generate(modelDiffer.GetDifferences(sourceModel, targetModel), targetModel),
                    connection);

                // Reverse-engineer and execute the test-provided assertions on the resulting database model
                var targetScaffoldedModel = databaseModelFactory.Create(
                    context.Database.GetDbConnection(),
                    new DatabaseModelFactoryOptions());

                modelAsserter(targetScaffoldedModel);

                // Apply reverse migrations to go back to the source state
                migrationsCommandExecutor.ExecuteNonQuery(
                    migrationsSqlGenerator.Generate(modelDiffer.GetDifferences(targetModel, sourceModel), sourceModel),
                    connection);

                var sourceScaffoldedModel2 = databaseModelFactory.Create(
                    context.Database.GetDbConnection(),
                    new DatabaseModelFactoryOptions());

                // TODO: Complete all equality implementations in DatabaseModel and related types
                //Assert.Equal(sourceScaffoldedModel, sourceScaffoldedModel2);

                // Apply reverse migrations to go back to the initial empty state
                migrationsCommandExecutor.ExecuteNonQuery(
                    migrationsSqlGenerator.Generate(modelDiffer.GetDifferences(sourceModel, null)),
                    connection);

                var emptyModel = databaseModelFactory.Create(
                    context.Database.GetDbConnection(),
                    new DatabaseModelFactoryOptions());

                Assert.Empty(emptyModel.Tables);
                Assert.Empty(emptyModel.Sequences);
            }
            catch
            {
                try
                {
                    Fixture.TestStore.Clean(context);
                }
                catch
                {
                    // ignored, throw the original exception
                }

                throw;
            }
        }

        public abstract class MigrationsFixtureBase2 : SharedStoreFixtureBase<PoolableDbContext>
        {
            public abstract TestHelpers TestHelpers { get; }
            public TestSqlLoggerFactory TestSqlLoggerFactory => (TestSqlLoggerFactory)ListLoggerFactory;
        }
    }
}
