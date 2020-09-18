// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;

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
                public override bool NoStoreCascades
                    => true;

                protected override string StoreName { get; } = "GraphClientCascadeUpdatesTest";

                protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
                {
                    base.OnModelCreating(modelBuilder, context);

                    foreach (var foreignKey in modelBuilder.Model
                        .GetEntityTypes()
                        .SelectMany(e => e.GetDeclaredForeignKeys())
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
                public override bool ForceClientNoAction
                    => true;

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

        public class TptIdentity : GraphUpdatesSqlServerTestBase<TptIdentity.SqlServerFixture>
        {
            public TptIdentity(SqlServerFixture fixture)
                : base(fixture)
            {
            }

            [ConditionalFact(Skip = "Issue #22582")]
            public override void Can_add_multiple_dependents_when_multiple_possible_principal_sides()
            {
            }

            [ConditionalFact(Skip = "Issue #22582")]
            public override void Can_add_valid_first_dependent_when_multiple_possible_principal_sides()
            {
            }

            [ConditionalFact(Skip = "Issue #22582")]
            public override void Can_add_valid_second_dependent_when_multiple_possible_principal_sides()
            {
            }

            protected override void UseTransaction(DatabaseFacade facade, IDbContextTransaction transaction)
                => facade.UseTransaction(transaction.GetDbTransaction());

            public class SqlServerFixture : GraphUpdatesSqlServerFixtureBase
            {
                protected override string StoreName { get; } = "GraphTptIdentityUpdatesTest";

                protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
                {
                    modelBuilder.UseIdentityColumns();

                    base.OnModelCreating(modelBuilder, context);

                    modelBuilder.Entity<Root>().ToTable(nameof(Root));
                    modelBuilder.Entity<Required1>().ToTable(nameof(Required1));
                    modelBuilder.Entity<Required1Derived>().ToTable(nameof(Required1Derived));
                    modelBuilder.Entity<Required1MoreDerived>().ToTable(nameof(Required1MoreDerived));
                    modelBuilder.Entity<Required2Derived>().ToTable(nameof(Required2Derived));
                    modelBuilder.Entity<Required2MoreDerived>().ToTable(nameof(Required2MoreDerived));
                    modelBuilder.Entity<Optional1>().ToTable(nameof(Optional1));
                    modelBuilder.Entity<Optional1Derived>().ToTable(nameof(Optional1Derived));
                    modelBuilder.Entity<Optional1MoreDerived>().ToTable(nameof(Optional1MoreDerived));
                    modelBuilder.Entity<Optional2Derived>().ToTable(nameof(Optional2Derived));
                    modelBuilder.Entity<Optional2MoreDerived>().ToTable(nameof(Optional2MoreDerived));
                    modelBuilder.Entity<RequiredSingle1>().ToTable(nameof(RequiredSingle1));
                    modelBuilder.Entity<OptionalSingle1>().ToTable(nameof(OptionalSingle1));
                    modelBuilder.Entity<OptionalSingle2>().ToTable(nameof(OptionalSingle2));
                    modelBuilder.Entity<RequiredNonPkSingle1>().ToTable(nameof(RequiredNonPkSingle1));
                    modelBuilder.Entity<RequiredNonPkSingle2Derived>().ToTable(nameof(RequiredNonPkSingle2Derived));
                    modelBuilder.Entity<RequiredNonPkSingle2MoreDerived>().ToTable(nameof(RequiredNonPkSingle2MoreDerived));
                    modelBuilder.Entity<RequiredAk1>().ToTable(nameof(RequiredAk1));
                    modelBuilder.Entity<RequiredAk1Derived>().ToTable(nameof(RequiredAk1Derived));
                    modelBuilder.Entity<RequiredAk1MoreDerived>().ToTable(nameof(RequiredAk1MoreDerived));
                    modelBuilder.Entity<OptionalAk1>().ToTable(nameof(OptionalAk1));
                    modelBuilder.Entity<OptionalAk1Derived>().ToTable(nameof(OptionalAk1Derived));
                    modelBuilder.Entity<OptionalAk1MoreDerived>().ToTable(nameof(OptionalAk1MoreDerived));
                    modelBuilder.Entity<RequiredSingleAk1>().ToTable(nameof(RequiredSingleAk1));
                    modelBuilder.Entity<OptionalSingleAk1>().ToTable(nameof(OptionalSingleAk1));
                    modelBuilder.Entity<OptionalSingleAk2Derived>().ToTable(nameof(OptionalSingleAk2Derived));
                    modelBuilder.Entity<OptionalSingleAk2MoreDerived>().ToTable(nameof(OptionalSingleAk2MoreDerived));
                    modelBuilder.Entity<RequiredNonPkSingleAk1>().ToTable(nameof(RequiredNonPkSingleAk1));
                    modelBuilder.Entity<RequiredAk2>().ToTable(nameof(RequiredAk2));
                    modelBuilder.Entity<RequiredAk2Derived>().ToTable(nameof(RequiredAk2Derived));
                    modelBuilder.Entity<RequiredAk2MoreDerived>().ToTable(nameof(RequiredAk2MoreDerived));
                    modelBuilder.Entity<OptionalAk2>().ToTable(nameof(OptionalAk2));
                    modelBuilder.Entity<OptionalAk2Derived>().ToTable(nameof(OptionalAk2Derived));
                    modelBuilder.Entity<OptionalAk2MoreDerived>().ToTable(nameof(OptionalAk2MoreDerived));
                    modelBuilder.Entity<RequiredSingleAk2>().ToTable(nameof(RequiredSingleAk2));
                    modelBuilder.Entity<RequiredNonPkSingleAk2>().ToTable(nameof(RequiredNonPkSingleAk2));
                    modelBuilder.Entity<RequiredNonPkSingleAk2Derived>().ToTable(nameof(RequiredNonPkSingleAk2Derived));
                    modelBuilder.Entity<RequiredNonPkSingleAk2MoreDerived>().ToTable(nameof(RequiredNonPkSingleAk2MoreDerived));
                    modelBuilder.Entity<OptionalSingleAk2>().ToTable(nameof(OptionalSingleAk2));
                    modelBuilder.Entity<RequiredComposite1>().ToTable(nameof(RequiredComposite1));
                    modelBuilder.Entity<OptionalOverlapping2>().ToTable(nameof(OptionalOverlapping2));
                    modelBuilder.Entity<BadCustomer>().ToTable(nameof(BadCustomer));
                    modelBuilder.Entity<BadOrder>().ToTable(nameof(BadOrder));
                    modelBuilder.Entity<QuestTask>().ToTable(nameof(QuestTask));
                    modelBuilder.Entity<QuizTask>().ToTable(nameof(QuizTask));
                    modelBuilder.Entity<HiddenAreaTask>().ToTable(nameof(HiddenAreaTask));
                    modelBuilder.Entity<TaskChoice>().ToTable(nameof(TaskChoice));
                    modelBuilder.Entity<ParentAsAChild>().ToTable(nameof(ParentAsAChild));
                    modelBuilder.Entity<ChildAsAParent>().ToTable(nameof(ChildAsAParent));
                    modelBuilder.Entity<Poost>().ToTable(nameof(Poost));
                    modelBuilder.Entity<Bloog>().ToTable(nameof(Bloog));
                    modelBuilder.Entity<Produce>().ToTable(nameof(Produce));
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

            protected override IQueryable<Root> ModifyQueryRoot(IQueryable<Root> query)
                => query.AsSplitQuery();

            protected override void UseTransaction(DatabaseFacade facade, IDbContextTransaction transaction)
                => facade.UseTransaction(transaction.GetDbTransaction());

            public abstract class GraphUpdatesSqlServerFixtureBase : GraphUpdatesFixtureBase
            {
                public TestSqlLoggerFactory TestSqlLoggerFactory
                    => (TestSqlLoggerFactory)ListLoggerFactory;

                protected override ITestStoreFactory TestStoreFactory
                    => SqlServerTestStoreFactory.Instance;
            }
        }
    }
}
