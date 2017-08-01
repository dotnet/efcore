// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.EntityFrameworkCore.Design.Internal;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Scaffolding.Internal;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.EntityFrameworkCore.ReverseEngineering
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
        public void One_to_one()
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

                var results = Generator.Generate(
                    testStore.ConnectionString,
                    Enumerable.Empty<string>(),
                    Enumerable.Empty<string>(),
                    TestProjectPath,
                    outputPath: null,
                    rootNamespace: "E2E.Sqlite",
                    contextName: null,
                    useDataAnnotations: UseDataAnnotations,
                    overwriteFiles: false,
                    useDatabaseNames: false);

                Assert.Empty(_reporter.Messages.Where(m => m.StartsWith("warn: ")));

                var expectedFileSet = new FileSet(new FileSystemFileService(), Path.Combine(ExpectedResultsParentDir, "OneToOne"))
                {
                    Files =
                    {
                        "OneToOne" + DbSuffix + "Context.cs",
                        "Dependent.cs",
                        "Principal.cs"
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
        public void One_to_many()
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

                var results = Generator.Generate(
                    testStore.ConnectionString,
                    Enumerable.Empty<string>(),
                    Enumerable.Empty<string>(),
                    TestProjectPath,
                    outputPath: null,
                    rootNamespace: "E2E.Sqlite",
                    contextName: null,
                    useDataAnnotations: UseDataAnnotations,
                    overwriteFiles: false,
                    useDatabaseNames: false);

                Assert.Empty(_reporter.Messages.Where(m => m.StartsWith("warn: ")));

                var expectedFileSet = new FileSet(new FileSystemFileService(), Path.Combine(ExpectedResultsParentDir, "OneToMany"))
                {
                    Files =
                    {
                        "OneToMany" + DbSuffix + "Context.cs",
                        "OneToManyDependent.cs",
                        "OneToManyPrincipal.cs"
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
        public void Many_to_many()
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

                var results = Generator.Generate(
                    testStore.ConnectionString,
                    Enumerable.Empty<string>(),
                    Enumerable.Empty<string>(),
                    TestProjectPath,
                    outputPath: null,
                    rootNamespace: "E2E.Sqlite",
                    contextName: null,
                    useDataAnnotations: UseDataAnnotations,
                    overwriteFiles: false,
                    useDatabaseNames: false);

                Assert.Empty(_reporter.Messages.Where(m => m.StartsWith("warn: ")));

                var expectedFileSet = new FileSet(new FileSystemFileService(), Path.Combine(ExpectedResultsParentDir, "ManyToMany"))
                {
                    Files =
                    {
                        "ManyToMany" + DbSuffix + "Context.cs",
                        "Groups.cs",
                        "Users.cs",
                        "UsersGroups.cs"
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
        public void Self_referencing()
        {
            using (var testStore = SqliteTestStore.GetOrCreateShared("SelfRef" + DbSuffix))
            {
                testStore.ExecuteNonQuery(@"CREATE TABLE IF NOT EXISTS SelfRef (
    Id INTEGER PRIMARY KEY,
    SelfForeignKey INTEGER,
    FOREIGN KEY (SelfForeignKey) REFERENCES SelfRef (Id)
);");

                var results = Generator.Generate(
                    testStore.ConnectionString,
                    Enumerable.Empty<string>(),
                    Enumerable.Empty<string>(),
                    TestProjectPath,
                    outputPath: null,
                    rootNamespace: "E2E.Sqlite",
                    contextName: null,
                    useDataAnnotations: UseDataAnnotations,
                    overwriteFiles: false,
                    useDatabaseNames: false);

                Assert.Empty(_reporter.Messages.Where(m => m.StartsWith("warn: ")));

                var expectedFileSet = new FileSet(new FileSystemFileService(), Path.Combine(ExpectedResultsParentDir, "SelfRef"))
                {
                    Files =
                    {
                        "SelfRef" + DbSuffix + "Context.cs",
                        "SelfRef.cs"
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
        public void Missing_primary_key()
        {
            using (var testStore = SqliteTestStore.CreateScratch())
            {
                testStore.ExecuteNonQuery("CREATE TABLE Alicia ( Keys TEXT );");

                var results = Generator.Generate(
                    testStore.ConnectionString,
                    Enumerable.Empty<string>(),
                    Enumerable.Empty<string>(),
                    TestProjectPath,
                    outputPath: null,
                    rootNamespace: "E2E.Sqlite",
                    contextName: null,
                    useDataAnnotations: UseDataAnnotations,
                    overwriteFiles: false,
                    useDatabaseNames: false);

                var errorMessage = DesignStrings.UnableToGenerateEntityType("Alicia");
                Assert.Contains("warn: " + DesignStrings.MissingPrimaryKey("Alicia"), _reporter.Messages);
                Assert.Contains("warn: " + errorMessage, _reporter.Messages);
                Assert.Contains(errorMessage, InMemoryFiles.RetrieveFileContents(TestProjectFullPath, Path.GetFileName(results.ContextFile)));
            }
        }

        [Fact]
        public void Principal_missing_primary_key()
        {
            using (var testStore = SqliteTestStore.GetOrCreateShared("NoPrincipalPk" + DbSuffix))
            {
                testStore.ExecuteNonQuery(@"CREATE TABLE IF NOT EXISTS Dependent (
    Id PRIMARY KEY,
    PrincipalId INT,
    FOREIGN KEY (PrincipalId) REFERENCES Principal(Id)
);
CREATE TABLE IF NOT EXISTS Principal ( Id INT);");

                var results = Generator.Generate(
                    testStore.ConnectionString,
                    Enumerable.Empty<string>(),
                    Enumerable.Empty<string>(),
                    TestProjectPath,
                    outputPath: null,
                    rootNamespace: "E2E.Sqlite",
                    contextName: null,
                    useDataAnnotations: UseDataAnnotations,
                    overwriteFiles: false,
                    useDatabaseNames: false);

                Assert.Contains("warn: " + DesignStrings.MissingPrimaryKey("Principal"), _reporter.Messages);
                Assert.Contains("warn: " + DesignStrings.UnableToGenerateEntityType("Principal"), _reporter.Messages);
                Assert.Contains("warn: " + DesignStrings.ForeignKeyScaffoldErrorPrincipalTableScaffoldingError("Dependent(PrincipalId)", "Principal"), _reporter.Messages);

                var expectedFileSet = new FileSet(new FileSystemFileService(), Path.Combine(ExpectedResultsParentDir, "NoPrincipalPk"))
                {
                    Files =
                    {
                        "NoPrincipalPk" + DbSuffix + "Context.cs",
                        "Dependent.cs"
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
        public void It_handles_unsafe_names()
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

                var results = Generator.Generate(
                    testStore.ConnectionString,
                    Enumerable.Empty<string>(),
                    Enumerable.Empty<string>(),
                    TestProjectPath,
                    outputPath: null,
                    rootNamespace: "E2E.Sqlite",
                    contextName: null,
                    useDataAnnotations: UseDataAnnotations,
                    overwriteFiles: false,
                    useDatabaseNames: false);

                Assert.Empty(_reporter.Messages.Where(m => m.StartsWith("warn: ")));

                var files = new FileSet(InMemoryFiles, TestProjectFullPath)
                {
                    Files = Enumerable.Repeat(results.ContextFile, 1).Concat(results.EntityTypeFiles).Select(Path.GetFileName).ToList()
                };
                AssertCompile(files);
            }
        }

        [Fact]
        public virtual void Foreign_key_to_unique_index()
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

                var results = Generator.Generate(
                    testStore.ConnectionString,
                    new[] { "User", "Comment" },
                    Enumerable.Empty<string>(),
                    TestProjectPath,
                    outputPath: null,
                    rootNamespace: "E2E.Sqlite",
                    contextName: "FkToAltKeyContext",
                    useDataAnnotations: UseDataAnnotations,
                    overwriteFiles: false,
                    useDatabaseNames: false);

                Assert.Empty(_reporter.Messages.Where(m => m.StartsWith("warn: ")));

                var expectedFileSet = new FileSet(new FileSystemFileService(), Path.Combine(ExpectedResultsParentDir, "FkToAltKey"))
                {
                    Files =
                    {
                        "FkToAltKeyContext.cs",
                        "Comment.cs",
                        "User.cs"
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
            BuildReference.ByName("Microsoft.EntityFrameworkCore.Sqlite"),
            BuildReference.ByName("Microsoft.EntityFrameworkCore"),
            BuildReference.ByName("Microsoft.EntityFrameworkCore.Relational")
        };

        protected abstract string DbSuffix { get; } // will be used to create different databases so tests running in parallel don't interfere
        protected abstract string ExpectedResultsParentDir { get; }
        protected abstract bool UseDataAnnotations { get; }

        protected override void ConfigureDesignTimeServices(IServiceCollection services)
            => new SqliteDesignTimeServices().ConfigureDesignTimeServices(services);
    }
}
