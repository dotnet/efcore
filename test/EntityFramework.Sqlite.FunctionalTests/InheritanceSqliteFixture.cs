// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Data.Entity.FunctionalTests;
using Microsoft.Data.Entity.FunctionalTests.TestModels.Inheritance;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.Logging;

namespace Microsoft.Data.Entity.Sqlite.FunctionalTests
{
    public class InheritanceSqliteFixture : InheritanceRelationalFixture
    {
        private readonly DbContextOptions _options;
        private readonly IServiceProvider _serviceProvider;

        public InheritanceSqliteFixture()
        {
            _serviceProvider
                = new ServiceCollection()
                    .AddEntityFramework()
                    .AddSqlite()
                    .ServiceCollection()
                    .AddSingleton(TestSqliteModelSource.GetFactory(OnModelCreating))
                    .AddInstance<ILoggerFactory>(new TestSqlLoggerFactory())
                    .BuildServiceProvider();

            var testStore = SqliteTestStore.CreateScratch();

            var optionsBuilder = new DbContextOptionsBuilder();
            optionsBuilder.UseSqlite(testStore.Connection);
            _options = optionsBuilder.Options;

            // TODO: Do this via migrations & update pipeline

            testStore.ExecuteNonQuery(@"
                DROP TABLE IF EXISTS Country;
                DROP TABLE IF EXISTS Animal;

                CREATE TABLE Country (
                    Id int NOT NULL PRIMARY KEY,
                    Name nvarchar(100) NOT NULL
                );

                CREATE TABLE Animal (
                    Species nvarchar(100) NOT NULL PRIMARY KEY,
                    Name nvarchar(100) NOT NULL,
                    CountryId int NOT NULL ,
                    IsFlightless bit NOT NULL,
                    EagleId nvarchar(100),
                    'Group' int,
                    FoundOn tinyint,
                    Discriminator nvarchar(255),

                    FOREIGN KEY(countryId) REFERENCES Country(Id),
                    FOREIGN KEY(EagleId) REFERENCES Animal(Species)
                );

                CREATE TABLE Plant (
                    Genus int NOT NULL,
                    Species nvarchar(100) NOT NULL PRIMARY KEY,
                    Name nvarchar(100) NOT NULL,
                    CountryId int,
                    HasThorns bit,

                    FOREIGN KEY(countryId) REFERENCES Country(Id)
                );
            ");

            using (var context = CreateContext())
            {
                SeedData(context);
            }
            TestSqlLoggerFactory.Reset();
        }

        public override InheritanceContext CreateContext()
        {
            return new InheritanceContext(_serviceProvider, _options);
        }
    }
}
