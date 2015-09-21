// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.Data.Entity.Relational.Design.FunctionalTests.ReverseEngineering;
using Microsoft.Data.Entity.Relational.Design.ReverseEngineering;
using Microsoft.Data.Entity.Sqlite.Design;
using Microsoft.Data.Entity.Sqlite.Design.ReverseEngineering;
using Microsoft.Data.Entity.Sqlite.FunctionalTests;
using Xunit;
using Xunit.Abstractions;

namespace EntityFramework.Sqlite.Design.FunctionalTests.ReverseEngineering
{
    public abstract class SqliteE2ETestBase : E2ETestBase
    {
        public SqliteE2ETestBase(ITestOutputHelper output)
            : base(output)
        {
        }

        [Fact]
        public async void One_to_one()
        {
            using (var testStore = SqliteTestStore.GetOrCreateShared("OneToOne" + DbSuffix).AsTransient())
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

                var results = await Generator.GenerateAsync(new ReverseEngineeringConfiguration
                {
                    ConnectionString = testStore.Connection.ConnectionString,
                    ProjectPath = "testout",
                    ProjectRootNamespace = "E2E.Sqlite",
                    UseFluentApiOnly = UseFluentApiOnly
                });

                AssertLog(new LoggerMessages());

                var expectedFileSet = new FileSet(new FileSystemFileService(), Path.Combine(ExpectedResultsParentDir, "OneToOne"))
                {
                    Files =
                    {
                        "OneToOne" + DbSuffix + "Context.expected",
                        "Dependent.expected",
                        "Principal.expected"
                    }
                };
                var actualFileSet = new FileSet(InMemoryFiles, "testout")
                {
                    Files = Enumerable.Repeat(results.ContextFile, 1).Concat(results.EntityTypeFiles).Select(Path.GetFileName).ToList()
                };
                AssertEqualFileContents(expectedFileSet, actualFileSet);
                AssertCompile(actualFileSet);
            }
        }

        [Fact]
        public async void One_to_many()
        {
            using (var testStore = SqliteTestStore.GetOrCreateShared("OneToMany" + DbSuffix).AsTransient())
            {
                testStore.ExecuteNonQuery(@"
CREATE TABLE IF NOT EXISTS OneToManyPrincipal (
    OneToManyPrincipalID1 INT,
    OneToManyPrincipalID2 INT,
    Other TEXT NOT NULL,
    PRIMARY KEY (OneToManyPrincipalID1, OneToManyPrincipalID2)
);
CREATE TABLE IF NOT EXISTS OneToManyDependent (
    OneToManyDependentID1 INT NOT NULL,
    OneToManyDependentID2 INT NOT NULL,
    SomeDependentEndColumn VARCHAR NOT NULL,
    OneToManyDependentFK1 INT,
    OneToManyDependentFK2 INT,
    PRIMARY KEY (OneToManyDependentID1, OneToManyDependentID2),
    FOREIGN KEY ( OneToManyDependentFK1, OneToManyDependentFK2)
        REFERENCES OneToManyPrincipal ( OneToManyPrincipalID1, OneToManyPrincipalID2  )
);
");
                testStore.Transaction.Commit();

                var results = await Generator.GenerateAsync(new ReverseEngineeringConfiguration
                {
                    ConnectionString = testStore.Connection.ConnectionString,
                    ProjectPath = "testout",
                    ProjectRootNamespace = "E2E.Sqlite",
                    UseFluentApiOnly = UseFluentApiOnly
                });

                AssertLog(new LoggerMessages());

                var expectedFileSet = new FileSet(new FileSystemFileService(), Path.Combine(ExpectedResultsParentDir, "OneToMany"))
                {
                    Files =
                    {
                        "OneToMany" + DbSuffix + "Context.expected",
                        "OneToManyDependent.expected",
                        "OneToManyPrincipal.expected"
                    }
                };
                var actualFileSet = new FileSet(InMemoryFiles, "testout")
                {
                    Files = Enumerable.Repeat(results.ContextFile, 1).Concat(results.EntityTypeFiles).Select(Path.GetFileName).ToList()
                };
                AssertEqualFileContents(expectedFileSet, actualFileSet);
                AssertCompile(actualFileSet);
            }
        }

        [Fact]
        public async void Many_to_many()
        {
            using (var testStore = SqliteTestStore.GetOrCreateShared("ManyToMany" + DbSuffix).AsTransient())
            {
                testStore.ExecuteNonQuery(@"
CREATE TABLE Users ( Id PRIMARY KEY);
CREATE TABLE Groups (Id PRIMARY KEY);
CREATE TABLE Users_Groups (
    Id PRIMARY KEY,
    UserId,
    GroupId,
    UNIQUE (UserId, GroupId),
    FOREIGN KEY (UserId) REFERENCES Users (Id),
    FOREIGN KEY (GroupId) REFERENCES Groups (Id)
);
");
                testStore.Transaction.Commit();

                var results = await Generator.GenerateAsync(new ReverseEngineeringConfiguration
                {
                    ConnectionString = testStore.Connection.ConnectionString,
                    ProjectPath = "testout",
                    ProjectRootNamespace = "E2E.Sqlite",
                    UseFluentApiOnly = UseFluentApiOnly
                });

                AssertLog(new LoggerMessages());

                var expectedFileSet = new FileSet(new FileSystemFileService(), Path.Combine(ExpectedResultsParentDir, "ManyToMany"))
                {
                    Files =
                    {
                        "ManyToMany" + DbSuffix + "Context.expected",
                        "Groups.expected",
                        "Users.expected",
                        "Users_Groups.expected"
                    }
                };
                var actualFileSet = new FileSet(InMemoryFiles, "testout")
                {
                    Files = Enumerable.Repeat(results.ContextFile, 1).Concat(results.EntityTypeFiles).Select(Path.GetFileName).ToList()
                };
                AssertEqualFileContents(expectedFileSet, actualFileSet);
                AssertCompile(actualFileSet);
            }
        }

        [Fact]
        public async void Self_referencing()
        {
            using (var testStore = SqliteTestStore.GetOrCreateShared("SelfRef" + DbSuffix).AsTransient())
            {
                testStore.ExecuteNonQuery(@"CREATE TABLE SelfRef (
    Id INTEGER PRIMARY KEY,
    SelfForeignKey INTEGER,
    FOREIGN KEY (SelfForeignKey) REFERENCES SelfRef (Id)
);");
                testStore.Transaction.Commit();

                var results = await Generator.GenerateAsync(new ReverseEngineeringConfiguration
                {
                    ConnectionString = testStore.Connection.ConnectionString,
                    ProjectPath = "testout",
                    ProjectRootNamespace = "E2E.Sqlite",
                    UseFluentApiOnly = UseFluentApiOnly
                });

                AssertLog(new LoggerMessages());

                var expectedFileSet = new FileSet(new FileSystemFileService(), Path.Combine(ExpectedResultsParentDir, "SelfRef"))
                {
                    Files =
                    {
                        "SelfRef" + DbSuffix + "Context.expected",
                        "SelfRef.expected"
                    }
                };
                var actualFileSet = new FileSet(InMemoryFiles, "testout")
                {
                    Files = Enumerable.Repeat(results.ContextFile, 1).Concat(results.EntityTypeFiles).Select(Path.GetFileName).ToList()
                };
                AssertEqualFileContents(expectedFileSet, actualFileSet);
                AssertCompile(actualFileSet);
            }
        }

        [Fact]
        public async void Missing_primary_key()
        {
            using (var testStore = SqliteTestStore.CreateScratch())
            {
                testStore.ExecuteNonQuery("CREATE TABLE Alicia ( Keys TEXT );");

                var results = await Generator.GenerateAsync(new ReverseEngineeringConfiguration
                {
                    ConnectionString = testStore.Connection.ConnectionString,
                    ProjectPath = "testout",
                    ProjectRootNamespace = "E2E.Sqlite",
                    UseFluentApiOnly = UseFluentApiOnly
                });
                var errorMessage = Strings.MissingPrimaryKey("Alicia");
                var expectedLog = new LoggerMessages
                {
                    Warn =
                    {
                        errorMessage
                    }
                };
                AssertLog(expectedLog);
                Assert.Contains(errorMessage, InMemoryFiles.RetrieveFileContents("testout", "Alicia.cs"));
            }
        }

        [Fact]
        public async void Principal_missing_primary_key()
        {
            using (var testStore = SqliteTestStore.GetOrCreateShared("NoPrincipalPk" + DbSuffix).AsTransient())
            {
                testStore.ExecuteNonQuery(@"CREATE TABLE Dependent (
    Id PRIMARY KEY,
    PrincipalId INT,
    FOREIGN KEY (PrincipalId) REFERENCES Principal(Id)
);
CREATE TABLE Principal ( Id INT);");
                testStore.Transaction.Commit();

                var results = await Generator.GenerateAsync(new ReverseEngineeringConfiguration
                {
                    ConnectionString = testStore.Connection.ConnectionString,
                    ProjectPath = "testout",
                    ProjectRootNamespace = "E2E.Sqlite",
                    UseFluentApiOnly = UseFluentApiOnly
                });

                var expectedLog = new LoggerMessages
                {
                    Warn =
                    {
                        Strings.MissingPrimaryKey("Principal"),
                        Strings.ForeignKeyScaffoldError("Dependent", "PrincipalId")
                    }
                };
                AssertLog(expectedLog);

                var expectedFileSet = new FileSet(new FileSystemFileService(), Path.Combine(ExpectedResultsParentDir, "NoPrincipalPk"))
                {
                    Files =
                    {
                        "NoPrincipalPk" + DbSuffix + "Context.expected",
                        "Dependent.expected",
                        "Principal.expected"
                    }
                };
                var actualFileSet = new FileSet(InMemoryFiles, "testout")
                {
                    Files = Enumerable.Repeat(results.ContextFile, 1).Concat(results.EntityTypeFiles).Select(Path.GetFileName).ToList()
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
CREATE TABLE 'Named with space' ( Id PRIMARY KEY );
CREATE TABLE '123 Invalid Class Name' ( Id PRIMARY KEY);
CREATE TABLE 'Bad characters `~!@#$%^&*()+=-[];''"",.<>/?|\ ' ( Id PRIMARY KEY);
CREATE TABLE ' Bad columns ' (
    'Space jam' PRIMARY KEY,
    '123 Go`',
    'Bad to the bone. `~!@#$%^&*()+=-[];''"",.<>/?|\ ',
    'Next one is all bad',
    '@#$%^&*()'
);
CREATE TABLE Keywords (
    namespace PRIMARY KEY,
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
                    ConnectionString = testStore.Connection.ConnectionString,
                    ProjectPath = "testout",
                    ProjectRootNamespace = "E2E.Sqlite",
                    UseFluentApiOnly = UseFluentApiOnly
                });

                AssertLog(new LoggerMessages());

                var files = new FileSet(InMemoryFiles, "testout")
                {
                    Files = Enumerable.Repeat(results.ContextFile, 1).Concat(results.EntityTypeFiles).Select(Path.GetFileName).ToList()
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
                // ReSharper disable once RedundantCommaInInitializer
                "Microsoft.Data.Sqlite",
#if DNXCORE50
                        "System.Data.Common",
                        "System.Linq.Expressions",
                        "System.Reflection",
                        "System.ComponentModel.Annotations",
#else
            },
            References =
            {
                MetadataReference.CreateFromFile(
                    Assembly.Load(new AssemblyName(
                        "System.Data, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089")).Location),
                MetadataReference.CreateFromFile(
                    Assembly.Load(new AssemblyName(
                        "System.ComponentModel.DataAnnotations, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35")).Location)
#endif
            }
        };

        protected abstract string DbSuffix { get; } // will be used to create different databases so tests running in parallel don't interfere
        protected abstract string ExpectedResultsParentDir { get; }
        protected abstract bool UseFluentApiOnly { get; }
        protected override IDesignTimeMetadataProviderFactory GetFactory() => new SqliteDesignTimeMetadataProviderFactory();
    }
}
