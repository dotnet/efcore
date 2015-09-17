// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Data.Entity;
using Microsoft.Data.Entity.FunctionalTests;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Relational.Design.ReverseEngineering;
using Microsoft.Data.Entity.Sqlite.Design;
using Microsoft.Data.Entity.Sqlite.Design.ReverseEngineering;
using Microsoft.Data.Entity.Sqlite.FunctionalTests;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.Logging;
using Xunit;

namespace EntityFramework.Sqlite.Design.FunctionalTests
{
    public class SqliteMetadataModelProviderTest
    {
        private readonly SqliteMetadataModelProvider _metadataModelProvider;
        private readonly SqliteTestStore _testStore;
        private TestLogger _logger;

        public SqliteMetadataModelProviderTest()
        {
            _testStore = SqliteTestStore.CreateScratch();
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddLogging();
            new SqliteDesignTimeMetadataProviderFactory().AddMetadataProviderServices(serviceCollection);
            serviceCollection.AddScoped(typeof(ILogger), sp => { return _logger = new TestLogger(); });
            serviceCollection.AddScoped<IFileService, FileSystemFileService>();

            _metadataModelProvider = serviceCollection
                .BuildServiceProvider()
                .GetService<IDatabaseMetadataModelProvider>() as SqliteMetadataModelProvider;
        }

        [Fact]
        public void It_loads_column_types()
        {
            var entityType = GetModel(@"CREATE TABLE ""Column Types"" ( 
                                        col1 text, 
                                        col2 unsigned big int );")
                .GetEntityType("Column Types");

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
                                            occupation text default ""dev"", 
                                            pay int default 2,
                                            hiredate datetime default current_timestamp,
                                            iq float default (100 + 19.4),
                                            name text
                                            );")
                .GetEntityType("Jobs");

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
                                        Name text not null,
                                        MenuUrl text );")
                .GetEntityType("Restaurants");

            Assert.NotNull(entityType);

            Assert.False(entityType.FindProperty("Name").IsNullable);
            Assert.True(entityType.FindProperty("MenuUrl").IsNullable);
        }

        [Theory]
        [InlineData("Id INT PRIMARY KEY", new[] { "Id" })]
        [InlineData("Id INT, AltId INT, PRIMARY KEY (Id, AltId)", new[] { "Id", "AltId" })]
        public void It_loads_primary_key(string def, string[] keys)
        {
            var entityType = GetModel($"CREATE TABLE Keys ({def});").GetEntityType("Keys");

            Assert.NotNull(entityType);
            Assert.Equal(keys, entityType.GetKeys().First().Properties.Select(p => p.Sqlite().ColumnName).ToArray());
        }

        [Theory]
        [InlineData(new[] { "Id" }, "CREATE TABLE t (Id int, Unique(id))")]
        [InlineData(new[] { "Id" }, "CREATE TABLE t (Id int); CREATE UNIQUE INDEX idx_1 on t (id);")]
        [InlineData(new[] { "Qu\"oted" }, "CREATE TABLE t (\"Qu\"\"oted\" text, Unique(\"Qu\"\"oted\"))")]
        [InlineData(new[] { "Qu\"oted" }, "CREATE TABLE t (\"Qu\"\"oted\" text UNIQUE);")]
        [InlineData(new[] { "Qu\"oted" }, "CREATE TABLE t (\"Qu\"\"oted\" text); CREATE Unique INDEX idx_1 on t(\"Qu\"\"oted\");")]
        public void It_gets_unique_indexes(string[] columns, string create)
        {
            var entityType = GetModel(create).GetEntityType("t");

            var idx = entityType.GetIndexes().First();
            Assert.True(idx.IsUnique);
            Assert.Equal(
                columns,
                idx.Properties.Select(c => c.Sqlite().ColumnName).ToArray());
        }

        [Fact]
        public void It_gets_indexes()
        {
            var entityType = GetModel("CREATE TABLE t (Id int, A text); CREATE INDEX idx_1 on t (id, a);").GetEntityType("t");

            var idx = entityType.GetIndexes().First();
            Assert.False(idx.IsUnique);
            Assert.Equal(
                new[] { "Id", "A" },
                idx.Properties.Select(c => c.Sqlite().ColumnName).ToArray());
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
            var parent = model.GetEntityType("Parent");
            var children = model.GetEntityType("Children");

            Assert.NotEmpty(parent.FindReferencingForeignKeys());
            Assert.NotEmpty(children.GetForeignKeys());

            var principalKey = children.GetForeignKey(children.FindProperty("ParentId")).PrincipalKey;
            Assert.Equal("Parent", principalKey.EntityType.Name);
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
            var parent = model.GetEntityType("Parent");
            var children = model.GetEntityType("Children");

            Assert.NotEmpty(parent.FindReferencingForeignKeys());
            Assert.NotEmpty(children.GetForeignKeys());

            var propList = new List<Property>
            {
                (Property)children.FindProperty("ParentId_A"),
                (Property)children.FindProperty("ParentId_B")
            };
            var principalKey = children.GetForeignKey(propList.AsReadOnly()).PrincipalKey;
            Assert.Equal("Parent", principalKey.EntityType.Name);
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
            var list = model.GetEntityType("ItemsList");

            Assert.NotEmpty(list.FindReferencingForeignKeys());
            Assert.NotEmpty(list.GetForeignKeys());

            var principalKey = list.GetForeignKey(list.FindProperty("ParentId")).PrincipalKey;
            Assert.Equal("ItemsList", principalKey.EntityType.Name);
            Assert.Equal("Id", principalKey.Properties[0].Name);
        }

        [Fact]
        public void It_logs_warning_for_bad_foreign_key()
        {
            // will fail because Id is not found
            var sql = @"CREATE TABLE Parent ( Name ); 
                        CREATE TABLE Children (
                            Id INT PRIMARY KEY,
                            ParentId INT,
                            FOREIGN KEY (ParentId) REFERENCES Parent (Id)
                        );";

            GetModel(sql);

            Assert.Contains("Warning: " + Strings.ForeignKeyScaffoldError("Children", "ParentId"), _logger.FullLog);
        }

        [Fact]
        public void It_assigns_uniqueness_to_foreign_key()
        {
            var sql = @"CREATE TABLE Friends ( 
    Id PRIMARY KEY, 
    BuddyId UNIQUE, 
    FOREIGN KEY (BuddyId) REFERENCES Friends(Id)
);";
            var table = GetModel(sql).GetEntityType("Friends");

            var fk = table.GetForeignKey(new[] { table.GetProperty("BuddyId") });

            Assert.True(fk.IsUnique);
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
            var table = GetModel(sql).GetEntityType("Friends");

            var fk = table.GetForeignKey(new[] { table.GetProperty("Id") });

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
            var table = GetModel(sql).GetEntityType("Friends");

            var fk = table.GetForeignKey(new[] { table.GetProperty("BuddyId") });

            Assert.False(fk.IsUnique);
        }

        [Fact]
        public void It_assigns_uniqueness_to_composite_foreign_key()
        {
            var sql = @"CREATE TABLE DoubleMint ( A , B, PRIMARY KEY (A,B));
CREATE TABLE Gum ( A, B, 
    UNIQUE (A,B),
    FOREIGN KEY (A, B) REFERENCES DoubleMint (A, B)
);";
            var dependent = GetModel(sql).GetEntityType("Gum");
            var foreignKey = dependent.GetForeignKey(new[] { dependent.GetProperty("A"), dependent.GetProperty("B") });

            Assert.True(foreignKey.IsUnique);
        }

        [Fact]
        public void It_does_not_assign_uniqueness_to_composite_foreign_key()
        {
            var sql = @"CREATE TABLE DoubleMint ( A , B, UNIQUE (A,B));
CREATE TABLE Gum ( A, B, 
    FOREIGN KEY (A, B) REFERENCES DoubleMint (A, B)
);";
            var dependent = GetModel(sql).GetEntityType("Gum");
            var foreignKey = dependent.GetForeignKey(new[] { dependent.GetProperty("A"), dependent.GetProperty("B") });

            Assert.False(foreignKey.IsUnique);
        }

        private IModel GetModel(string createSql)
        {
            _testStore.ExecuteNonQuery(createSql);
            return _metadataModelProvider.ConstructRelationalModel(_testStore.Connection.ConnectionString);
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
