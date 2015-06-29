// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Data.Entity.FunctionalTests;
using Microsoft.Data.Entity.FunctionalTests.TestModels.Inheritance;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.Logging;

namespace Microsoft.Data.Entity.SqlServer.FunctionalTests
{
    public class InheritanceSqlServerFixture : InheritanceFixtureBase
    {
        private readonly DbContextOptions _options;
        private readonly IServiceProvider _serviceProvider;

        public InheritanceSqlServerFixture()
        {
            _serviceProvider
                = new ServiceCollection()
                    .AddEntityFramework()
                    .AddSqlServer()
                    .ServiceCollection()
                    .AddSingleton(TestSqlServerModelSource.GetFactory(OnModelCreating))
                    .AddInstance<ILoggerFactory>(new TestSqlLoggerFactory())
                    .BuildServiceProvider();

            var testStore = SqlServerTestStore.CreateScratch();

            var optionsBuilder = new DbContextOptionsBuilder();
            optionsBuilder.UseSqlServer(testStore.Connection);
            _options = optionsBuilder.Options;

            // TODO: Do this via migrations

            testStore.ExecuteNonQuery(@"
                CREATE TABLE Country (
                    Id int NOT NULL PRIMARY KEY,
                    Name nvarchar(100) NOT NULL
                );

                CREATE TABLE Animal (
                    Species nvarchar(100) NOT NULL PRIMARY KEY,
                    Name nvarchar(100) NOT NULL,
                    CountryId int NOT NULL FOREIGN KEY REFERENCES Country (Id),
                    IsFlightless bit NOT NULL,
                    EagleId nvarchar(100) FOREIGN KEY REFERENCES Animal (Species),
                    [Group] int,
                    FoundOn tinyint,
                    Discriminator nvarchar(255) NOT NULL
                );");

            using (var context = CreateContext())
            {
                SeedData(context);
            }
        }

        public override AnimalContext CreateContext()
        {
            return new AnimalContext(_serviceProvider, _options);
        }

        public override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // TODO: Code First this

            var animal = modelBuilder.Entity<Animal>().Metadata;

            var discriminatorProperty
                = animal.AddProperty("Discriminator", typeof(string), shadowProperty: true);

            discriminatorProperty.IsNullable = false;
            //discriminatorProperty.IsReadOnlyBeforeSave = true; // #2132
            discriminatorProperty.IsReadOnlyAfterSave = true;
            discriminatorProperty.IsValueGeneratedOnAdd = true;

            animal.Relational().DiscriminatorProperty = discriminatorProperty;
        }
    }
}
