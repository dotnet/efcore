// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Data.Entity.Internal;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Metadata.Internal;
using Microsoft.Data.Entity.Scaffolding;
using Microsoft.Data.Entity.Scaffolding.Internal;
using Microsoft.Data.Entity.Sqlite.FunctionalTests;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Microsoft.Data.Entity.Sqlite.Design.FunctionalTests
{
    public class SqliteScaffoldingModelFactoryTest : IDisposable
    {
        private readonly RelationalScaffoldingModelFactory _scaffoldingModelFactory;
        private readonly SqliteTestStore _testStore;
        private readonly TestLogger _logger;

        public SqliteScaffoldingModelFactoryTest()
        {
            _testStore = SqliteTestStore.CreateScratch();
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddLogging();
            new SqliteDesignTimeServices().ConfigureDesignTimeServices(serviceCollection);
            serviceCollection.AddSingleton<IFileService, FileSystemFileService>();

            var serviceProvider = serviceCollection
                .BuildServiceProvider();

            _logger = new TestLogger();
            serviceProvider.GetService<ILoggerFactory>().AddProvider(new TestLoggerProvider(_logger));

            _scaffoldingModelFactory = serviceProvider
                .GetService<IScaffoldingModelFactory>() as RelationalScaffoldingModelFactory;
        }

        [Fact]
        public void It_loads_column_types()
        {
            var entityType = GetModel(@"CREATE TABLE ""Column Types"" (
                                        col1 text PRIMARY KEY,
                                        col2 unsigned big int );").FindEntityType("Column_Types");

            Assert.NotNull(entityType);
            Assert.Equal("Column Types", entityType.Sqlite().TableName);

            Assert.Equal("text", entityType.FindProperty("col1").Sqlite().ColumnType);
            Assert.Equal(typeof(string), entityType.FindProperty("col1").ClrType);

            Assert.Equal("unsigned big int", entityType.FindProperty("col2").Sqlite().ColumnType);
            Assert.Equal(typeof(long?), entityType.FindProperty("col2").ClrType);
        }

        [Fact]
        public void It_loads_default_values()
        {
            var entityType = GetModel(@"CREATE TABLE Jobs (
                                            id PRIMARY KEY,
                                            occupation text default ""dev"",
                                            pay int default 2,
                                            hiredate datetime default current_timestamp,
                                            iq float default (100 + 19.4),
                                            name text
                                            );").FindEntityType("Jobs");

            Assert.NotNull(entityType);

            Assert.Equal("\"dev\"", entityType.FindProperty("occupation").Sqlite().GeneratedValueSql);
            Assert.Equal("2", entityType.FindProperty("pay").Sqlite().GeneratedValueSql);
            Assert.Equal("current_timestamp", entityType.FindProperty("hiredate").Sqlite().GeneratedValueSql);
            Assert.Equal("100 + 19.4", entityType.FindProperty("iq").Sqlite().GeneratedValueSql);
            Assert.Null(entityType.FindProperty("name").Sqlite().GeneratedValueSql);
        }

        [Fact]
        public void It_identifies_not_null()
        {
            var entityType = GetModel(@"CREATE TABLE Restaurants (
                                        Id int primary key,
                                        Name text not null,
                                        MenuUrl text );").FindEntityType("Restaurants");

            Assert.NotNull(entityType);

            Assert.False(entityType.FindProperty("Name").IsNullable);
            Assert.True(entityType.FindProperty("MenuUrl").IsNullable);
        }

        [Fact]
        public void It_gets_unique_indexes()
        {
            var sql = "CREATE TABLE t (Id int, AltId int PRIMARY KEY, Unique(id))";
            var entityType = GetModel(sql).FindEntityType("t");

            var idx = entityType.GetIndexes().Last();
            Assert.True(idx.IsUnique);
        }

        [Fact]
        public void It_gets_indexes()
        {
            var entityType = GetModel("CREATE TABLE t (Id int, A text PRIMARY KEY); CREATE INDEX idx_1 on t (id, a);").FindEntityType("t");

            var idx = entityType.GetIndexes().Last();
            Assert.False(idx.IsUnique);
            Assert.Equal(
                new[] { "Id", "A" },
                idx.Properties.Select(c => c.Sqlite().ColumnName).ToArray());

            Assert.Single(entityType.GetKeys());
        }

        [Fact]
        public void It_loads_simple_references()
        {
            var sql = @"CREATE TABLE Parent ( Id INT PRIMARY KEY );
                        CREATE TABLE Children (
                            Id INT PRIMARY KEY,
                            ParentId INT,
                            FOREIGN KEY (parentid) REFERENCES parent (id)
                        );";

            var model = GetModel(sql);
            var parent = model.FindEntityType("Parent");
            var children = model.FindEntityType("Children");

            Assert.NotEmpty(parent.GetReferencingForeignKeys());
            Assert.NotEmpty(children.GetForeignKeys());

            var principalKey = children.FindForeignKeys(children.FindProperty("ParentId")).SingleOrDefault().PrincipalKey;
            Assert.Equal("Parent", principalKey.DeclaringEntityType.Name);
            Assert.Equal("Id", principalKey.Properties[0].Name);
        }

        [Fact]
        public void It_loads_composite_key_references()
        {
            var sql = @"CREATE TABLE Parent ( Id_A INT, Id_B INT, PRIMARY KEY (ID_A, id_b) );
                        CREATE TABLE Children (
                            Id INT PRIMARY KEY,
                            ParentId_A INT,
                            ParentId_B INT,
                            FOREIGN KEY (parentid_A, parentid_B) REFERENCES parent (id_a, id_b)
                        );";

            var model = GetModel(sql);
            var parent = model.FindEntityType("Parent");
            var children = model.FindEntityType("Children");

            Assert.NotEmpty(parent.GetReferencingForeignKeys());
            Assert.NotEmpty(children.GetForeignKeys());

            var propList = new List<Property>
            {
                (Property)children.FindProperty("ParentId_A"),
                (Property)children.FindProperty("ParentId_B")
            };
            var principalKey = children.FindForeignKeys(propList.AsReadOnly()).SingleOrDefault().PrincipalKey;
            Assert.Equal("Parent", principalKey.DeclaringEntityType.Name);
            Assert.Equal("Id_A", principalKey.Properties[0].Name);
            Assert.Equal("Id_B", principalKey.Properties[1].Name);
        }

        [Fact]
        public void It_loads_self_referencing_foreign_key()
        {
            var sql = @"CREATE TABLE ItemsList (
                            Id INT PRIMARY KEY,
                            ParentId INT,
                            FOREIGN KEY (ParentId) REFERENCES ItemsList (Id)
                        );";

            var model = GetModel(sql);
            var list = model.FindEntityType("ItemsList");

            Assert.NotEmpty(list.GetReferencingForeignKeys());
            Assert.NotEmpty(list.GetForeignKeys());

            var principalKey = list.FindForeignKeys(list.FindProperty("ParentId")).SingleOrDefault().PrincipalKey;
            Assert.Equal("ItemsList", principalKey.DeclaringEntityType.Name);
            Assert.Equal("Id", principalKey.Properties[0].Name);
        }

        [Fact]
        public void It_logs_warning_for_bad_foreign_key()
        {
            // will fail because Id is not found
            var sql = @"CREATE TABLE Parent ( Name PRIMARY KEY);
                        CREATE TABLE Children (
                            Id INT PRIMARY KEY,
                            ParentId INT,
                            FOREIGN KEY (ParentId) REFERENCES Parent (Id)
                        );";

            GetModel(sql);

            Assert.Contains("Warning: " +
                RelationalDesignStrings.ForeignKeyScaffoldErrorPropertyNotFound("Children(ParentId)"),
                _logger.FullLog);
        }

        [Fact]
        public void It_assigns_uniqueness_to_foreign_key()
        {
            var sql = @"CREATE TABLE Friends (
    Id PRIMARY KEY,
    BuddyId UNIQUE,
    FOREIGN KEY (BuddyId) REFERENCES Friends(Id)
);";
            var table = GetModel(sql).FindEntityType("Friends");

            var fk = table.FindForeignKeys(new[] { table.FindProperty("BuddyId") }).SingleOrDefault();

            Assert.True(fk.IsUnique);
            Assert.Equal(table.FindPrimaryKey(), fk.PrincipalKey);
        }

        [Fact]
        public void It_assigns_uniqueness_to_pk_foreign_key()
        {
            var sql = @"
CREATE TABLE Family (Id PRIMARY KEY);
CREATE TABLE Friends (
    Id PRIMARY KEY,
    FOREIGN KEY (Id) REFERENCES Family(Id)
);";
            var table = GetModel(sql).FindEntityType("Friends");

            var fk = table.FindForeignKeys(new[] { table.FindProperty("Id") }).SingleOrDefault();

            Assert.True(fk.IsUnique);
        }

        [Fact]
        public void It_does_not_assign_uniqueness_to_foreign_key()
        {
            var sql = @"CREATE TABLE Friends (
    Id PRIMARY KEY,
    BuddyId,
    FOREIGN KEY (BuddyId) REFERENCES Friends(Id)
);";
            var table = GetModel(sql).FindEntityType("Friends");

            var fk = table.FindForeignKeys(new[] { table.FindProperty("BuddyId") }).SingleOrDefault();

            Assert.False(fk.IsUnique);
        }

        [Fact]
        public void It_assigns_uniqueness_to_composite_foreign_key()
        {
            var sql = @"CREATE TABLE DoubleMint ( A , B, PRIMARY KEY (A,B));
CREATE TABLE Gum ( A, B PRIMARY KEY,
    UNIQUE (A,B),
    FOREIGN KEY (A, B) REFERENCES DoubleMint (A, B)
);";
            var dependent = GetModel(sql).FindEntityType("Gum");
            var foreignKey = dependent.FindForeignKeys(new[] { dependent.FindProperty("A"), dependent.FindProperty("B") }).SingleOrDefault();

            Assert.True(foreignKey.IsUnique);
        }

        [Fact]
        public void It_does_not_assign_uniqueness_to_composite_foreign_key()
        {
            var sql = @"CREATE TABLE DoubleMint ( A , B PRIMARY KEY, UNIQUE (A,B));
CREATE TABLE Gum ( A, B PRIMARY KEY,
    FOREIGN KEY (A, B) REFERENCES DoubleMint (A, B)
);";
            var dependent = GetModel(sql).FindEntityType("Gum");
            var foreignKey = dependent.FindForeignKeys(new[] { dependent.FindProperty("A"), dependent.FindProperty("B") }).SingleOrDefault();

            Assert.False(foreignKey.IsUnique);
        }

        private IModel GetModel(string createSql)
        {
            _testStore.ExecuteNonQuery(createSql);
            return _scaffoldingModelFactory.Create(_testStore.Connection.ConnectionString, TableSelectionSet.All);
        }

        public void Dispose() => _testStore.Dispose();
    }
}
