// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.FunctionalTests;
using Microsoft.EntityFrameworkCore.FunctionalTests.TestModels.Inheritance;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Microsoft.EntityFrameworkCore.Sqlite.FunctionalTests
{
    public class InheritanceSqliteFixture : InheritanceRelationalFixture, IDisposable
    {
        private readonly DbContextOptions _options;
        private readonly SqliteTestStore _testStore;

        public InheritanceSqliteFixture()
        {
            _testStore = SqliteTestStore.CreateScratch();

            _options = new DbContextOptionsBuilder()
                .UseSqlite(_testStore.Connection)
                .UseInternalServiceProvider(new ServiceCollection()
                    .AddEntityFrameworkSqlite()
                    .AddSingleton(TestSqliteModelSource.GetFactory(OnModelCreating))
                    .AddSingleton<ILoggerFactory>(new TestSqlLoggerFactory())
                    .BuildServiceProvider())
                .Options;

            // TODO: Do this via migrations & update pipeline

            _testStore.ExecuteNonQuery(@"
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

        public override InheritanceContext CreateContext() => new InheritanceContext(_options);

        public void Dispose() => _testStore.Dispose();
    }
}
