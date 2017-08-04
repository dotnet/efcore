// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Microsoft.EntityFrameworkCore
{
    public abstract class GraphUpdatesSqlServerTest
    {
        public abstract class GraphUpdatesSqlServerTestBase<TFixture> : GraphUpdatesTestBase<TFixture>
            where TFixture : GraphUpdatesSqlServerTestBase<TFixture>.GraphUpdatesSqlServerFixtureBase, new()
        {
            protected GraphUpdatesSqlServerTestBase(TFixture fixture)
                : base(fixture)
            {
            }

            protected override void UseTransaction(DatabaseFacade facade, IDbContextTransaction transaction)
                => facade.UseTransaction(transaction.GetDbTransaction());

            public abstract class GraphUpdatesSqlServerFixtureBase : GraphUpdatesFixtureBase
            {
                public TestSqlLoggerFactory TestSqlLoggerFactory => (TestSqlLoggerFactory)ServiceProvider.GetRequiredService<ILoggerFactory>();
                protected override ITestStoreFactory<TestStore> TestStoreFactory => SqlServerTestStoreFactory.Instance;
            }
        }

        public class Identity : GraphUpdatesSqlServerTestBase<Identity.GraphUpdatesWithIdentitySqlServerFixture>
        {
            public Identity(GraphUpdatesWithIdentitySqlServerFixture fixture)
                : base(fixture)
            {
            }

            public class GraphUpdatesWithIdentitySqlServerFixture : GraphUpdatesSqlServerFixtureBase
            {
                protected override string StoreName { get; } = "GraphIdentityUpdatesTest";
                
                protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
                {
                    modelBuilder.ForSqlServerUseIdentityColumns();

                    base.OnModelCreating(modelBuilder, context);
                }
            }
        }

        public class Restrict : GraphUpdatesSqlServerTestBase<Restrict.GraphUpdatesWithRestrictSqlServerFixture>
        {
            public Restrict(GraphUpdatesWithRestrictSqlServerFixture fixture)
                : base(fixture)
            {
            }

            public class GraphUpdatesWithRestrictSqlServerFixture : GraphUpdatesSqlServerFixtureBase
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

        [SqlServerCondition(SqlServerCondition.SupportsSequences)]
        public class Sequence : GraphUpdatesSqlServerTestBase<Sequence.GraphUpdatesWithSequenceSqlServerFixture>
        {
            public Sequence(GraphUpdatesWithSequenceSqlServerFixture fixture)
                : base(fixture)
            {
            }

            public class GraphUpdatesWithSequenceSqlServerFixture : GraphUpdatesSqlServerFixtureBase
            {
                protected override string StoreName { get; } = "GraphSequenceUpdatesTest";

                protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
                {
                    modelBuilder.ForSqlServerUseSequenceHiLo(); // ensure model uses sequences
                    base.OnModelCreating(modelBuilder, context);
                }
            }
        }
    }
}
