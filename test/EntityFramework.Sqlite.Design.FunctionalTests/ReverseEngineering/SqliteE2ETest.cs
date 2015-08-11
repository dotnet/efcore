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
        public void It_generates_for_simple_db()
        {
            var testStore = SqliteTestStore.GetOrCreateShared("SimpleReveng", ()=> {} );
            testStore.ExecuteNonQuery(@"CREATE TABLE IF NOT EXISTS parent ( Id INT PRIMARY KEY);
CREATE TABLE IF NOT EXISTS child (Id INT PRIMARY KEY, ParentId INT, FOREIGN KEY (ParentId) REFERENCES parent (Id));
");
            testStore.Transaction.Commit();

            var results = Generator.GenerateAsync(new ReverseEngineeringConfiguration
                {
                    Provider = MetadataModelProvider,
                    ConnectionString = testStore.Connection.ConnectionString,
                    Namespace = "E2E.Sqlite",
                    ProjectPath = "testout"
                }).GetAwaiter().GetResult();
            var expectedFileSet = new FileSet(new FileSystemFileService(), "ReverseEngineering/Expected")
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
