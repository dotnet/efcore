// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.TestUtilities;

namespace Microsoft.EntityFrameworkCore
{
    public class GraphUpdatesSqlServerTest
    {
        public class ClientCascade : GraphUpdatesSqlServerTestBase<ClientCascade.SqlServerFixture>
        {
            public ClientCascade(SqlServerFixture fixture)
                : base(fixture)
            {
            }

            protected override void UseTransaction(DatabaseFacade facade, IDbContextTransaction transaction)
                => facade.UseTransaction(transaction.GetDbTransaction());

            public class SqlServerFixture : GraphUpdatesSqlServerFixtureBase
            {
                public override bool NoStoreCascades => true;

                protected override string StoreName { get; } = "GraphClientCascadeUpdatesTest";

                protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
                {
                    base.OnModelCreating(modelBuilder, context);

                    foreach (var foreignKey in modelBuilder.Model
                        .GetEntityTypes()
                        .SelectMany(e => MutableEntityTypeExtensions.GetDeclaredForeignKeys(e))
                        .Where(e => e.DeleteBehavior == DeleteBehavior.Cascade))
                    {
                        foreignKey.DeleteBehavior = DeleteBehavior.ClientCascade;
                    }
                }
            }
        }

        public class ClientNoAction : GraphUpdatesSqlServerTestBase<ClientNoAction.SqlServerFixture>
        {
            public ClientNoAction(SqlServerFixture fixture)
                : base(fixture)
            {
            }

            protected override void UseTransaction(DatabaseFacade facade, IDbContextTransaction transaction)
                => facade.UseTransaction(transaction.GetDbTransaction());

            public class SqlServerFixture : GraphUpdatesSqlServerFixtureBase
            {
                public override bool ForceClientNoAction => true;

                protected override string StoreName { get; } = "GraphClientNoActionUpdatesTest";

                protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
                {
                    base.OnModelCreating(modelBuilder, context);

                    foreach (var foreignKey in modelBuilder.Model
                        .GetEntityTypes()
                        .SelectMany(e => e.GetDeclaredForeignKeys()))
                    {
                        foreignKey.DeleteBehavior = DeleteBehavior.ClientNoAction;
                    }
                }
            }
        }

        public class Identity : GraphUpdatesSqlServerTestBase<Identity.SqlServerFixture>
        {
            public Identity(SqlServerFixture fixture)
                : base(fixture)
            {
            }

            protected override void UseTransaction(DatabaseFacade facade, IDbContextTransaction transaction)
                => facade.UseTransaction(transaction.GetDbTransaction());

            public class SqlServerFixture : GraphUpdatesSqlServerFixtureBase
            {
                protected override string StoreName { get; } = "GraphIdentityUpdatesTest";

                protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
                {
                    modelBuilder.UseIdentityColumns();

                    base.OnModelCreating(modelBuilder, context);
                }
            }
        }

        public class HiLo : GraphUpdatesSqlServerTestBase<HiLo.SqlServerFixture>
        {
            public HiLo(SqlServerFixture fixture)
                : base(fixture)
            {
            }

            protected override void UseTransaction(DatabaseFacade facade, IDbContextTransaction transaction)
                => facade.UseTransaction(transaction.GetDbTransaction());

            public class SqlServerFixture : GraphUpdatesSqlServerFixtureBase
            {
                protected override string StoreName { get; } = "GraphHiLoUpdatesTest";

                protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
                {
                    modelBuilder.UseHiLo();

                    base.OnModelCreating(modelBuilder, context);
                }
            }
        }

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
                public TestSqlLoggerFactory TestSqlLoggerFactory => (TestSqlLoggerFactory)ListLoggerFactory;
                protected override ITestStoreFactory TestStoreFactory => SqlServerTestStoreFactory.Instance;
            }
        }
    }
}
