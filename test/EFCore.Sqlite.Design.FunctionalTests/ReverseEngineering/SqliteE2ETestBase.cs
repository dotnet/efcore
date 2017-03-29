// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Relational.Design.Specification.Tests.ReverseEngineering;
using Microsoft.EntityFrameworkCore.Relational.Design.Specification.Tests.TestUtilities;
using Microsoft.EntityFrameworkCore.Scaffolding;
using Microsoft.EntityFrameworkCore.Scaffolding.Internal;
using Microsoft.EntityFrameworkCore.Sqlite.FunctionalTests;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Xunit.Abstractions;

#if NETCOREAPP2_0
using System.Reflection;
#endif

namespace Microsoft.EntityFrameworkCore.Sqlite.Design.FunctionalTests.ReverseEngineering
{
    public abstract class SqliteE2ETestBase : E2ETestBase
    {
        public const string TestProjectPath = "testout";
        public static readonly string TestProjectFullPath = Path.GetFullPath(TestProjectPath);

        protected SqliteE2ETestBase(ITestOutputHelper output)
            : base(output)
        {
        }

        [Fact]
        public async Task One_to_one()
        {
            using (var testStore = SqliteTestStore.GetOrCreateShared("OneToOne" + DbSuffix))
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

                var results = await Generator.GenerateAsync(new ReverseEngineeringConfiguration
                {
                    ConnectionString = testStore.ConnectionString,
                    ProjectPath = TestProjectPath,
                    ProjectRootNamespace = "E2E.Sqlite",
                    UseFluentApiOnly = UseFluentApiOnly,
                    TableSelectionSet = TableSelectionSet.All
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
                var actualFileSet = new FileSet(InMemoryFiles, TestProjectFullPath)
                {
                    Files = Enumerable.Repeat(results.ContextFile, 1).Concat(results.EntityTypeFiles).Select(Path.GetFileName).ToList()
                };
                AssertEqualFileContents(expectedFileSet, actualFileSet);
                AssertCompile(actualFileSet);
            }
        }

        [Fact]
        public async Task One_to_many()
        {
            using (var testStore = SqliteTestStore.GetOrCreateShared("OneToMany" + DbSuffix))
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

                var results = await Generator.GenerateAsync(new ReverseEngineeringConfiguration
                {
                    ConnectionString = testStore.ConnectionString,
                    ProjectPath = TestProjectPath,
                    ProjectRootNamespace = "E2E.Sqlite",
                    UseFluentApiOnly = UseFluentApiOnly,
                    TableSelectionSet = TableSelectionSet.All
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
                var actualFileSet = new FileSet(InMemoryFiles, TestProjectFullPath)
                {
                    Files = Enumerable.Repeat(results.ContextFile, 1).Concat(results.EntityTypeFiles).Select(Path.GetFileName).ToList()
                };
                AssertEqualFileContents(expectedFileSet, actualFileSet);
                AssertCompile(actualFileSet);
            }
        }

        [Fact]
        public async Task Many_to_many()
        {
            using (var testStore = SqliteTestStore.GetOrCreateShared("ManyToMany" + DbSuffix))
            {
                testStore.ExecuteNonQuery(@"
CREATE TABLE IF NOT EXISTS Users ( Id PRIMARY KEY);
CREATE TABLE IF NOT EXISTS Groups (Id PRIMARY KEY);
CREATE TABLE IF NOT EXISTS Users_Groups (
    Id PRIMARY KEY,
    UserId,
    GroupId,
    UNIQUE (UserId, GroupId),
    FOREIGN KEY (UserId) REFERENCES Users (Id),
    FOREIGN KEY (GroupId) REFERENCES Groups (Id)
);
");

                var results = await Generator.GenerateAsync(new ReverseEngineeringConfiguration
                {
                    ConnectionString = testStore.ConnectionString,
                    ProjectPath = TestProjectPath,
                    ProjectRootNamespace = "E2E.Sqlite",
                    UseFluentApiOnly = UseFluentApiOnly,
                    TableSelectionSet = TableSelectionSet.All
                });

                AssertLog(new LoggerMessages());

                var expectedFileSet = new FileSet(new FileSystemFileService(), Path.Combine(ExpectedResultsParentDir, "ManyToMany"))
                {
                    Files =
                    {
                        "ManyToMany" + DbSuffix + "Context.expected",
                        "Groups.expected",
                        "Users.expected",
                        "UsersGroups.expected"
                    }
                };
                var actualFileSet = new FileSet(InMemoryFiles, TestProjectFullPath)
                {
                    Files = Enumerable.Repeat(results.ContextFile, 1).Concat(results.EntityTypeFiles).Select(Path.GetFileName).ToList()
                };
                AssertEqualFileContents(expectedFileSet, actualFileSet);
                AssertCompile(actualFileSet);
            }
        }

        [Fact]
        public async Task Self_referencing()
        {
            using (var testStore = SqliteTestStore.GetOrCreateShared("SelfRef" + DbSuffix))
            {
                testStore.ExecuteNonQuery(@"CREATE TABLE IF NOT EXISTS SelfRef (
    Id INTEGER PRIMARY KEY,
    SelfForeignKey INTEGER,
    FOREIGN KEY (SelfForeignKey) REFERENCES SelfRef (Id)
);");

                var results = await Generator.GenerateAsync(new ReverseEngineeringConfiguration
                {
                    ConnectionString = testStore.ConnectionString,
                    ProjectPath = TestProjectPath,
                    ProjectRootNamespace = "E2E.Sqlite",
                    UseFluentApiOnly = UseFluentApiOnly,
                    TableSelectionSet = TableSelectionSet.All
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
                var actualFileSet = new FileSet(InMemoryFiles, TestProjectFullPath)
                {
                    Files = Enumerable.Repeat(results.ContextFile, 1).Concat(results.EntityTypeFiles).Select(Path.GetFileName).ToList()
                };
                AssertEqualFileContents(expectedFileSet, actualFileSet);
                AssertCompile(actualFileSet);
            }
        }

        [Fact]
        public async Task Missing_primary_key()
        {
            using (var testStore = SqliteTestStore.CreateScratch())
            {
                testStore.ExecuteNonQuery("CREATE TABLE Alicia ( Keys TEXT );");

                var results = await Generator.GenerateAsync(new ReverseEngineeringConfiguration
                {
                    ConnectionString = testStore.ConnectionString,
                    ProjectPath = TestProjectPath,
                    ProjectRootNamespace = "E2E.Sqlite",
                    UseFluentApiOnly = UseFluentApiOnly,
                    TableSelectionSet = TableSelectionSet.All
                });
                var errorMessage = RelationalDesignStrings.UnableToGenerateEntityType("Alicia");
                var expectedLog = new LoggerMessages
                {
                    Warn =
                    {
                        RelationalDesignStrings.MissingPrimaryKey("Alicia"),
                        errorMessage
                    }
                };
                AssertLog(expectedLog);
                Assert.Contains(errorMessage, InMemoryFiles.RetrieveFileContents(TestProjectFullPath, Path.GetFileName(results.ContextFile)));
            }
        }

        [Fact]
        public async Task Principal_missing_primary_key()
        {
            using (var testStore = SqliteTestStore.GetOrCreateShared("NoPrincipalPk" + DbSuffix))
            {
                testStore.ExecuteNonQuery(@"CREATE TABLE IF NOT EXISTS Dependent (
    Id PRIMARY KEY,
    PrincipalId INT,
    FOREIGN KEY (PrincipalId) REFERENCES Principal(Id)
);
CREATE TABLE IF NOT EXISTS Principal ( Id INT);");

                var results = await Generator.GenerateAsync(new ReverseEngineeringConfiguration
                {
                    ConnectionString = testStore.ConnectionString,
                    ProjectPath = TestProjectPath,
                    ProjectRootNamespace = "E2E.Sqlite",
                    UseFluentApiOnly = UseFluentApiOnly,
                    TableSelectionSet = TableSelectionSet.All
                });

                var expectedLog = new LoggerMessages
                {
                    Warn =
                    {
                        RelationalDesignStrings.MissingPrimaryKey("Principal"),
                        RelationalDesignStrings.UnableToGenerateEntityType("Principal"),
                        RelationalDesignStrings.ForeignKeyScaffoldErrorPrincipalTableScaffoldingError("Dependent(PrincipalId)", "Principal")
                    }
                };
                AssertLog(expectedLog);

                var expectedFileSet = new FileSet(new FileSystemFileService(), Path.Combine(ExpectedResultsParentDir, "NoPrincipalPk"))
                {
                    Files =
                    {
                        "NoPrincipalPk" + DbSuffix + "Context.expected",
                        "Dependent.expected"
                    }
                };
                var actualFileSet = new FileSet(InMemoryFiles, TestProjectFullPath)
                {
                    Files = Enumerable.Repeat(results.ContextFile, 1).Concat(results.EntityTypeFiles).Select(Path.GetFileName).ToList()
                };
                AssertEqualFileContents(expectedFileSet, actualFileSet);
                AssertCompile(actualFileSet);
            }
        }

        [Fact]
        public async Task It_handles_unsafe_names()
        {
            using (var testStore = SqliteTestStore.CreateScratch())
            {
                testStore.ExecuteNonQuery(@"
CREATE TABLE IF NOT EXISTS 'Named with space' ( Id PRIMARY KEY );
CREATE TABLE IF NOT EXISTS '123 Invalid Class Name' ( Id PRIMARY KEY);
CREATE TABLE IF NOT EXISTS 'Bad characters `~!@#$%^&*()+=-[];''"",.<>/?|\ ' ( Id PRIMARY KEY);
CREATE TABLE IF NOT EXISTS ' Bad columns ' (
    'Space jam' PRIMARY KEY,
    '123 Go`',
    'Bad to the bone. `~!@#$%^&*()+=-[];''"",.<>/?|\ ',
    'Next one is all bad',
    '@#$%^&*()'
);
CREATE TABLE IF NOT EXISTS Keywords (
    namespace PRIMARY KEY,
    virtual,
    public,
    class,
    string,
    FOREIGN KEY (class) REFERENCES string (string)
);
CREATE TABLE IF NOT EXISTS String (
    string PRIMARY KEY,
    FOREIGN KEY (string) REFERENCES String (string)
);
");

                var results = await Generator.GenerateAsync(new ReverseEngineeringConfiguration
                {
                    ConnectionString = testStore.ConnectionString,
                    ProjectPath = TestProjectPath,
                    ProjectRootNamespace = "E2E.Sqlite",
                    UseFluentApiOnly = UseFluentApiOnly,
                    TableSelectionSet = TableSelectionSet.All
                });

                AssertLog(new LoggerMessages());

                var files = new FileSet(InMemoryFiles, TestProjectFullPath)
                {
                    Files = Enumerable.Repeat(results.ContextFile, 1).Concat(results.EntityTypeFiles).Select(Path.GetFileName).ToList()
                };
                AssertCompile(files);
            }
        }

        [Fact]
        public virtual async Task Foreign_key_to_unique_index()
        {
            using (var testStore = SqliteTestStore.GetOrCreateShared("FkToAltKey" + DbSuffix))
            {
                testStore.ExecuteNonQuery(@"
CREATE TABLE IF NOT EXISTS User (
    Id INTEGER PRIMARY KEY,
    AltId INTEGER NOT NULL UNIQUE
);
CREATE TABLE IF NOT EXISTS Comment (
    Id INTEGER PRIMARY KEY,
    UserAltId INTEGER NOT NULL,
    Contents TEXT,
    FOREIGN KEY (UserAltId) REFERENCES User (AltId)
);");

                var results = await Generator.GenerateAsync(new ReverseEngineeringConfiguration
                {
                    ConnectionString = testStore.ConnectionString,
                    ContextClassName = "FkToAltKeyContext",
                    ProjectPath = TestProjectPath,
                    ProjectRootNamespace = "E2E.Sqlite",
                    UseFluentApiOnly = UseFluentApiOnly,
                    TableSelectionSet = TableSelectionSet.All
                });

                AssertLog(new LoggerMessages());

                var expectedFileSet = new FileSet(new FileSystemFileService(), Path.Combine(ExpectedResultsParentDir, "FkToAltKey"))
                {
                    Files =
                    {
                        "FkToAltKeyContext.expected",
                        "Comment.expected",
                        "User.expected"
                    }
                };
                var actualFileSet = new FileSet(InMemoryFiles, TestProjectFullPath)
                {
                    Files = Enumerable.Repeat(results.ContextFile, 1).Concat(results.EntityTypeFiles).Select(Path.GetFileName).ToList()
                };
                AssertEqualFileContents(expectedFileSet, actualFileSet);
                AssertCompile(actualFileSet);
            }
        }

        protected override ICollection<BuildReference> References { get; } = new List<BuildReference>
        {
#if NET46
            BuildReference.ByName("System, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089"),
            BuildReference.ByName("System.Core, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089"),
            BuildReference.ByName("System.Data, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089"),
            BuildReference.ByName("System.Data.Common, Version=4.1.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"),
            BuildReference.ByName("System.ComponentModel.DataAnnotations, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35"),
#elif NETCOREAPP2_0
            BuildReference.ByName("System.Collections"),
            BuildReference.ByName("System.Data.Common"),
            BuildReference.ByName("System.Linq.Expressions"),
            BuildReference.ByName("System.Reflection"),
            BuildReference.ByName("System.ComponentModel.Annotations"),
#else
#error target frameworks need to be updated.
#endif
            BuildReference.ByName("Microsoft.EntityFrameworkCore.Sqlite"),
            BuildReference.ByName("Microsoft.EntityFrameworkCore"),
            BuildReference.ByName("Microsoft.EntityFrameworkCore.Relational"),
            BuildReference.ByName("Microsoft.Extensions.Caching.Abstractions"),
            BuildReference.ByName("Microsoft.Extensions.Logging.Abstractions")
        };

        protected abstract string DbSuffix { get; } // will be used to create different databases so tests running in parallel don't interfere
        protected abstract string ExpectedResultsParentDir { get; }
        protected abstract bool UseFluentApiOnly { get; }

        protected override void ConfigureDesignTimeServices(IServiceCollection services)
            => new SqliteDesignTimeServices().ConfigureDesignTimeServices(services);
    }
}
