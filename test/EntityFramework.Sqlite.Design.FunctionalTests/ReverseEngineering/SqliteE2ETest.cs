// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.Data.Entity.Relational.Design.FunctionalTests.ReverseEngineering;
using Microsoft.Data.Entity.Relational.Design.ReverseEngineering;
using Microsoft.Data.Entity.Sqlite.Design.ReverseEngineering;
using Microsoft.Data.Entity.Sqlite.FunctionalTests;
using Xunit;
using Xunit.Abstractions;

namespace EntityFramework.Sqlite.Design.FunctionalTests.ReverseEngineering
{
    public class SqliteE2ETest : E2ETestBase
    {
        public SqliteE2ETest(ITestOutputHelper output)
            : base(output)
        {
        }

        [Fact]
        public void One_to_one()
        {
            using (var testStore = SqliteTestStore.GetOrCreateShared("OneToOne", () => { }).AsTransient())
            {
                testStore.ExecuteNonQuery(@"
CREATE TABLE IF NOT EXISTS Principal ( 
    Id INTEGER PRIMARY KEY AUTOINCREMENT
);
CREATE TABLE IF NOT EXISTS Dependent (
    Id INT,
    PrincipalId INT NOT NULL UNIQUE,
    PRIMARY KEY (Id),
    FOREIGN KEY (PrincipalId) REFERENCES Principal (Id)
);
");
                testStore.Transaction.Commit();

                var results = Generator.GenerateAsync(new ReverseEngineeringConfiguration
                {
                    Provider = MetadataModelProvider,
                    ConnectionString = testStore.Connection.ConnectionString,
                    Namespace = "E2E.Sqlite",
                    OutputPath = "testout"
                }).GetAwaiter().GetResult();
                var expectedFileSet = new FileSet(new FileSystemFileService(), "ReverseEngineering/Expected/OneToOne")
                {
                    Files =
                        {
                            "ModelContext.expected",
                            "Dependent.expected",
                            "Principal.expected",
                        }
                };
                var actualFileSet = new FileSet(InMemoryFiles, "testout")
                {
                    Files = results.Select(Path.GetFileName).ToList()
                };
                AssertEqualFileContents(expectedFileSet, actualFileSet);
                AssertCompile(actualFileSet);
            }
        }

        [Fact]
        public void One_to_many()
        {
            using (var testStore = SqliteTestStore.GetOrCreateShared("SimpleReveng", () => { }).AsTransient())
            {

                testStore.ExecuteNonQuery(@"
CREATE TABLE IF NOT EXISTS parent ( 
    Id INT PRIMARY KEY 
);
CREATE TABLE IF NOT EXISTS child (
    Id INT PRIMARY KEY, 
    ParentId INT NOT NULL, 
    FOREIGN KEY (ParentId) REFERENCES parent (Id)
);
");
                testStore.Transaction.Commit();

                var results = Generator.GenerateAsync(new ReverseEngineeringConfiguration
                {
                    Provider = MetadataModelProvider,
                    ConnectionString = testStore.Connection.ConnectionString,
                    Namespace = "E2E.Sqlite",
                    OutputPath = "testout"
                }).GetAwaiter().GetResult();
                var expectedFileSet = new FileSet(new FileSystemFileService(), "ReverseEngineering/Expected/Simple")
                {
                    Files =
                        {
                            "ModelContext.expected",
                            "child.expected",
                            "parent.expected",
                        }
                };
                var actualFileSet = new FileSet(InMemoryFiles, "testout")
                {
                    Files = results.Select(Path.GetFileName).ToList()
                };
                AssertEqualFileContents(expectedFileSet, actualFileSet);
                AssertCompile(actualFileSet);
            }
        }

        [Fact]
        public void Many_to_many()
        {
            using (var testStore = SqliteTestStore.GetOrCreateShared("ManyToMany", () => { }).AsTransient())
            {

                testStore.ExecuteNonQuery(@"
CREATE TABLE Users ( Id PRIMARY KEY);
CREATE TABLE Groups (Id PRIMARY KEY);
CREATE TABLE Users_Groups (
    UserId, 
    GroupId, 
    UNIQUE (UserId, GroupId), 
    FOREIGN KEY (UserId) REFERENCES Users (Id), 
    FOREIGN KEY (GroupId) REFERENCES Groups (Id)
);
");
                testStore.Transaction.Commit();

                var results = Generator.GenerateAsync(new ReverseEngineeringConfiguration
                {
                    Provider = MetadataModelProvider,
                    ConnectionString = testStore.Connection.ConnectionString,
                    Namespace = "E2E.Sqlite",
                    OutputPath = "testout"
                }).GetAwaiter().GetResult();
                var expectedFileSet = new FileSet(new FileSystemFileService(), "ReverseEngineering/Expected/ManyToMany")
                {
                    Files =
                        {
                            "ModelContext.expected",
                            "Groups.expected",
                            "Users.expected",
                            "Users_Groups.expected",
                        }
                };
                var actualFileSet = new FileSet(InMemoryFiles, "testout")
                {
                    Files = results.Select(Path.GetFileName).ToList()
                };
                AssertEqualFileContents(expectedFileSet, actualFileSet);
                AssertCompile(actualFileSet);
            }
        }

        [Fact]
        public async void It_handles_unsafe_names()
        {
            using (var testStore = SqliteTestStore.CreateScratch())
            {

                testStore.ExecuteNonQuery(@"
CREATE TABLE 'Named with space' ( Id );
CREATE TABLE '123 Invalid Class Name' ( Id );
CREATE TABLE 'Bad characters `~!@#$%^&*()+=-[];''"",.<>/?|\ ' ( Id );
CREATE TABLE ' Bad columns ' (
    'Space jam',
    '123 Go`',
    'Bad to the bone. `~!@#$%^&*()+=-[];''"",.<>/?|\ ',
    'Next one is all bad',
    '@#$%^&*()'
);
CREATE TABLE Keywords (
    namespace,
    virtual,
    public,
    class,
    string,
    FOREIGN KEY (class) REFERENCES string (string)
);
CREATE TABLE String (
    string PRIMARY KEY,
    FOREIGN KEY (string) REFERENCES String (string)
);
");

                var results = await Generator.GenerateAsync(new ReverseEngineeringConfiguration
                    {
                        Provider = MetadataModelProvider,
                        ConnectionString = testStore.Connection.ConnectionString,
                        Namespace = "E2E.Sqlite",
                        OutputPath = "testout"
                    });

                var files = new FileSet(InMemoryFiles, "testout")
                    {
                        Files = results.Select(Path.GetFileName).ToList()
                    };
                AssertCompile(files);
            }
        }

        protected override E2ECompiler GetCompiler() => new E2ECompiler
        {
            NamedReferences =
                    {
                        "EntityFramework.Core",
                        "EntityFramework.Relational",
                        "EntityFramework.Sqlite",
                        "Microsoft.Data.Sqlite",
#if DNXCORE50
                        "System.Data.Common",
                        "System.Linq.Expressions",
                        "System.Reflection",
#else
                    },
            References =
                    {
                        MetadataReference.CreateFromFile(
                            Assembly.Load(new AssemblyName(
                                "System.Data, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089")).Location)
#endif
                    }
        };

        protected override string ProviderName => "EntityFramework.Sqlite.Design";
        protected override IDesignTimeMetadataProviderFactory GetFactory() => new SqliteDesignTimeMetadataProviderFactory();
    }
}
