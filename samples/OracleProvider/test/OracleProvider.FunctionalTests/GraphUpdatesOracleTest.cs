// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.TestUtilities;

namespace Microsoft.EntityFrameworkCore
{
    public abstract class GraphUpdatesOracleTest
    {
        public abstract class GraphUpdatesOracleTestBase<TFixture> : GraphUpdatesTestBase<TFixture>
            where TFixture : GraphUpdatesOracleTestBase<TFixture>.GraphUpdatesOracleFixtureBase, new()
        {
            protected GraphUpdatesOracleTestBase(TFixture fixture)
                : base(fixture)
            {
            }

            protected override void UseTransaction(DatabaseFacade facade, IDbContextTransaction transaction)
                => facade.UseTransaction(transaction.GetDbTransaction());

            public abstract class GraphUpdatesOracleFixtureBase : GraphUpdatesFixtureBase
            {
                public TestSqlLoggerFactory TestSqlLoggerFactory => (TestSqlLoggerFactory)ListLoggerFactory;
                protected override ITestStoreFactory TestStoreFactory => OracleTestStoreFactory.Instance;
            }
        }

        public class Identity : GraphUpdatesOracleTestBase<Identity.GraphUpdatesWithIdentityOracleFixture>
        {
            public Identity(GraphUpdatesWithIdentityOracleFixture fixture)
                : base(fixture)
            {
            }

            public class GraphUpdatesWithIdentityOracleFixture : GraphUpdatesOracleFixtureBase
            {
                protected override string StoreName { get; } = "GraphIdentityUpdatesTest";

                protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
                {
                    modelBuilder.ForOracleUseIdentityColumns();

                    base.OnModelCreating(modelBuilder, context);
                }
            }
        }

        public class Restrict : GraphUpdatesOracleTestBase<Restrict.GraphUpdatesWithRestrictOracleFixture>
        {
            public Restrict(GraphUpdatesWithRestrictOracleFixture fixture)
                : base(fixture)
            {
            }

            public class GraphUpdatesWithRestrictOracleFixture : GraphUpdatesOracleFixtureBase
            {
                protected override string StoreName { get; } = "GraphRestrictUpdatesTest";
                public override bool ForceRestrict => true;

                protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
                {
                    base.OnModelCreating(modelBuilder, context);

                    foreach (var foreignKey in modelBuilder.Model
                        .GetEntityTypes()
                        .SelectMany(e => e.GetDeclaredForeignKeys()))
                    {
                        foreignKey.DeleteBehavior = DeleteBehavior.Restrict;
                    }
                }
            }
        }

        public class Sequence : GraphUpdatesOracleTestBase<Sequence.GraphUpdatesWithSequenceOracleFixture>
        {
            public Sequence(GraphUpdatesWithSequenceOracleFixture fixture)
                : base(fixture)
            {
            }

            public class GraphUpdatesWithSequenceOracleFixture : GraphUpdatesOracleFixtureBase
            {
                protected override string StoreName { get; } = "GraphSequenceUpdatesTest";

                protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
                {
                    modelBuilder.ForOracleUseSequenceHiLo(); // ensure model uses sequences
                    base.OnModelCreating(modelBuilder, context);
                }
            }
        }
    }
}
