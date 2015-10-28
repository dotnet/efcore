// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Data.Entity;
using Microsoft.Data.Entity.FunctionalTests;
using Microsoft.Data.Entity.Internal;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Metadata.Internal;
using Microsoft.Data.Entity.Scaffolding;
using Microsoft.Data.Entity.Scaffolding.Internal;
using Microsoft.Data.Entity.Sqlite.FunctionalTests;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;

namespace EntityFramework.Sqlite.Design.FunctionalTests
{
    public class SqliteDatabaseModelFactoryTest
    {
        private readonly RelationalScaffoldingModelFactory _scaffoldingModelFactory;
        private readonly SqliteTestStore _testStore;
        private readonly TestLogger _logger;

        public SqliteDatabaseModelFactoryTest()
        {
            _testStore = SqliteTestStore.CreateScratch();
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddLogging()
                .AddSingleton<ModelUtilities>()
                .AddSingleton<CSharpUtilities>();
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

        [Theory]
        [InlineData("Id INT PRIMARY KEY", new[] { "Id" })]
        [InlineData("Id INT, AltId INT, PRIMARY KEY (Id, AltId)", new[] { "Id", "AltId" })]
        public void It_loads_primary_key(string def, string[] keys)
        {
            var entityType = GetModel($"CREATE TABLE Keys ({def});").FindEntityType("Keys");

            Assert.NotNull(entityType);
            Assert.Equal(keys, entityType.GetKeys().First().Properties.Select(p => p.Sqlite().ColumnName).ToArray());
        }

        [Theory]
        [InlineData(new[] { "Id" }, "CREATE TABLE t (Id int, AltId int PRIMARY KEY, Unique(id))")]
        [InlineData(new[] { "Id" }, "CREATE TABLE t (Id int, AltId int PRIMARY KEY); CREATE UNIQUE INDEX idx_1 on t (id);")]
        [InlineData(new[] { "Qu\"oted" }, "CREATE TABLE t (\"Qu\"\"oted\" text, AltId int PRIMARY KEY, Unique(\"Qu\"\"oted\"))")]
        [InlineData(new[] { "Qu\"oted" }, "CREATE TABLE t (\"Qu\"\"oted\" text UNIQUE, AltId int PRIMARY KEY);")]
        [InlineData(new[] { "Qu\"oted" }, "CREATE TABLE t (\"Qu\"\"oted\" text, AltId int PRIMARY KEY); CREATE Unique INDEX idx_1 on t(\"Qu\"\"oted\");")]
        [InlineData(new[] { "a", "b" }, "CREATE TABLE t (a int, b int, AltId int PRIMARY KEY, UNIQUE(a,b));")]
        [InlineData(new[] { "z", "y" }, "CREATE TABLE t (y int, z int, AltId int PRIMARY KEY, UNIQUE(z,y));")]
        public void It_gets_unique_indexes(string[] columns, string create)
        {
            var entityType = GetModel(create).FindEntityType("t");

            var idx = entityType.GetIndexes().Last();
            Assert.True(idx.IsUnique);
            Assert.Equal(
                columns,
                idx.Properties.Select(c => c.Sqlite().ColumnName).ToArray());

            var props = columns.Select(c => entityType.GetProperties().First(p => p.Sqlite().ColumnName == c)).ToArray();
            var key = entityType.FindKey(props);

            Assert.NotNull(key);
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
        public void It_loads_multiple_key_references()
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
        public void It_does_not_assigns_uniqueness_to_foreign_key()
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
    }

    public class TestLoggerProvider : ILoggerProvider
    {
        private readonly ILogger _logger;

        public TestLoggerProvider(ILogger logger)
        {
            _logger = logger;
        }

        public ILogger CreateLogger(string name) => _logger;

        public void Dispose()
        {
        }
    }

    public class TestLogger : ILogger
    {
        public IDisposable BeginScopeImpl(object state) => NullScope.Instance;
        public string FullLog => _sb.ToString();
        private readonly StringBuilder _sb = new StringBuilder();

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log(LogLevel logLevel, int eventId, object state, Exception exception, Func<object, Exception, string> formatter)
        {
            _sb.Append(logLevel)
                .Append(": ")
                .Append(formatter(state, exception));
        }
    }
}
