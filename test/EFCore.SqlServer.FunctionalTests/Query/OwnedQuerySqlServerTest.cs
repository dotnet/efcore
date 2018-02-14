// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class OwnedQuerySqlServerTest : OwnedQueryTestBase<OwnedQuerySqlServerTest.OwnedQuerySqlServerFixture>
    {
        public OwnedQuerySqlServerTest(OwnedQuerySqlServerFixture fixture)
            : base(fixture)
        {
            fixture.TestSqlLoggerFactory.Clear();
        }

        public override void Query_for_base_type_loads_all_owned_navs()
        {
            base.Query_for_base_type_loads_all_owned_navs();


            // See issue#10067
            AssertSql(
                @"SELECT [o].[Id], [o].[Discriminator], [t].[Id], [t0].[Id], [t0].[LeafBAddress_Country_Name], [t1].[Id], [t2].[Id], [t2].[LeafAAddress_Country_Name], [t3].[Id], [t4].[Id], [t4].[BranchAddress_Country_Name], [t5].[Id], [t6].[Id], [t6].[PersonAddress_Country_Name]
FROM [OwnedPerson] AS [o]
LEFT JOIN (
    SELECT [l.LeafBAddress].*
    FROM [OwnedPerson] AS [l.LeafBAddress]
    WHERE [l.LeafBAddress].[Discriminator] = N'LeafB'
) AS [t] ON [o].[Id] = [t].[Id]
LEFT JOIN (
    SELECT [l.LeafBAddress.Country].*
    FROM [OwnedPerson] AS [l.LeafBAddress.Country]
    WHERE [l.LeafBAddress.Country].[Discriminator] = N'LeafB'
) AS [t0] ON [t].[Id] = [t0].[Id]
LEFT JOIN (
    SELECT [l.LeafAAddress].*
    FROM [OwnedPerson] AS [l.LeafAAddress]
    WHERE [l.LeafAAddress].[Discriminator] = N'LeafA'
) AS [t1] ON [o].[Id] = [t1].[Id]
LEFT JOIN (
    SELECT [l.LeafAAddress.Country].*
    FROM [OwnedPerson] AS [l.LeafAAddress.Country]
    WHERE [l.LeafAAddress.Country].[Discriminator] = N'LeafA'
) AS [t2] ON [t1].[Id] = [t2].[Id]
LEFT JOIN (
    SELECT [b.BranchAddress].*
    FROM [OwnedPerson] AS [b.BranchAddress]
    WHERE [b.BranchAddress].[Discriminator] IN (N'LeafA', N'Branch')
) AS [t3] ON [o].[Id] = [t3].[Id]
LEFT JOIN (
    SELECT [b.BranchAddress.Country].*
    FROM [OwnedPerson] AS [b.BranchAddress.Country]
    WHERE [b.BranchAddress.Country].[Discriminator] IN (N'LeafA', N'Branch')
) AS [t4] ON [t3].[Id] = [t4].[Id]
LEFT JOIN (
    SELECT [o.PersonAddress].*
    FROM [OwnedPerson] AS [o.PersonAddress]
    WHERE [o.PersonAddress].[Discriminator] IN (N'LeafB', N'LeafA', N'Branch', N'OwnedPerson')
) AS [t5] ON [o].[Id] = [t5].[Id]
LEFT JOIN (
    SELECT [o.PersonAddress.Country].*
    FROM [OwnedPerson] AS [o.PersonAddress.Country]
    WHERE [o.PersonAddress.Country].[Discriminator] IN (N'LeafB', N'LeafA', N'Branch', N'OwnedPerson')
) AS [t6] ON [t5].[Id] = [t6].[Id]
WHERE [o].[Discriminator] IN (N'LeafB', N'LeafA', N'Branch', N'OwnedPerson')");
        }

        public override void No_ignored_include_warning_when_implicit_load()
        {
            base.No_ignored_include_warning_when_implicit_load();

            AssertSql(
                @"SELECT COUNT(*)
FROM [OwnedPerson] AS [o]
WHERE [o].[Discriminator] IN (N'LeafB', N'LeafA', N'Branch', N'OwnedPerson')");
        }

        public override void Query_for_branch_type_loads_all_owned_navs()
        {
            base.Query_for_branch_type_loads_all_owned_navs();

            AssertSql(
                @"SELECT [o].[Id], [o].[Discriminator], [t].[Id], [t0].[Id], [t0].[LeafAAddress_Country_Name], [t1].[Id], [t2].[Id], [t2].[BranchAddress_Country_Name], [t3].[Id], [t4].[Id], [t4].[PersonAddress_Country_Name]
FROM [OwnedPerson] AS [o]
LEFT JOIN (
    SELECT [l.LeafAAddress].*
    FROM [OwnedPerson] AS [l.LeafAAddress]
    WHERE [l.LeafAAddress].[Discriminator] = N'LeafA'
) AS [t] ON [o].[Id] = [t].[Id]
LEFT JOIN (
    SELECT [l.LeafAAddress.Country].*
    FROM [OwnedPerson] AS [l.LeafAAddress.Country]
    WHERE [l.LeafAAddress.Country].[Discriminator] = N'LeafA'
) AS [t0] ON [t].[Id] = [t0].[Id]
LEFT JOIN (
    SELECT [b.BranchAddress].*
    FROM [OwnedPerson] AS [b.BranchAddress]
    WHERE [b.BranchAddress].[Discriminator] IN (N'LeafA', N'Branch')
) AS [t1] ON [o].[Id] = [t1].[Id]
LEFT JOIN (
    SELECT [b.BranchAddress.Country].*
    FROM [OwnedPerson] AS [b.BranchAddress.Country]
    WHERE [b.BranchAddress.Country].[Discriminator] IN (N'LeafA', N'Branch')
) AS [t2] ON [t1].[Id] = [t2].[Id]
LEFT JOIN (
    SELECT [o.PersonAddress].*
    FROM [OwnedPerson] AS [o.PersonAddress]
    WHERE [o.PersonAddress].[Discriminator] IN (N'LeafB', N'LeafA', N'Branch', N'OwnedPerson')
) AS [t3] ON [o].[Id] = [t3].[Id]
LEFT JOIN (
    SELECT [o.PersonAddress.Country].*
    FROM [OwnedPerson] AS [o.PersonAddress.Country]
    WHERE [o.PersonAddress.Country].[Discriminator] IN (N'LeafB', N'LeafA', N'Branch', N'OwnedPerson')
) AS [t4] ON [t3].[Id] = [t4].[Id]
WHERE [o].[Discriminator] IN (N'LeafA', N'Branch')");
        }

        public override void Query_for_leaf_type_loads_all_owned_navs()
        {
            base.Query_for_leaf_type_loads_all_owned_navs();

            AssertSql(
                @"SELECT [o].[Id], [o].[Discriminator], [t].[Id], [t0].[Id], [t0].[LeafAAddress_Country_Name], [t1].[Id], [t2].[Id], [t2].[BranchAddress_Country_Name], [t3].[Id], [t4].[Id], [t4].[PersonAddress_Country_Name]
FROM [OwnedPerson] AS [o]
LEFT JOIN (
    SELECT [l.LeafAAddress].*
    FROM [OwnedPerson] AS [l.LeafAAddress]
    WHERE [l.LeafAAddress].[Discriminator] = N'LeafA'
) AS [t] ON [o].[Id] = [t].[Id]
LEFT JOIN (
    SELECT [l.LeafAAddress.Country].*
    FROM [OwnedPerson] AS [l.LeafAAddress.Country]
    WHERE [l.LeafAAddress.Country].[Discriminator] = N'LeafA'
) AS [t0] ON [t].[Id] = [t0].[Id]
LEFT JOIN (
    SELECT [b.BranchAddress].*
    FROM [OwnedPerson] AS [b.BranchAddress]
    WHERE [b.BranchAddress].[Discriminator] IN (N'LeafA', N'Branch')
) AS [t1] ON [o].[Id] = [t1].[Id]
LEFT JOIN (
    SELECT [b.BranchAddress.Country].*
    FROM [OwnedPerson] AS [b.BranchAddress.Country]
    WHERE [b.BranchAddress.Country].[Discriminator] IN (N'LeafA', N'Branch')
) AS [t2] ON [t1].[Id] = [t2].[Id]
LEFT JOIN (
    SELECT [o.PersonAddress].*
    FROM [OwnedPerson] AS [o.PersonAddress]
    WHERE [o.PersonAddress].[Discriminator] IN (N'LeafB', N'LeafA', N'Branch', N'OwnedPerson')
) AS [t3] ON [o].[Id] = [t3].[Id]
LEFT JOIN (
    SELECT [o.PersonAddress.Country].*
    FROM [OwnedPerson] AS [o.PersonAddress.Country]
    WHERE [o.PersonAddress.Country].[Discriminator] IN (N'LeafB', N'LeafA', N'Branch', N'OwnedPerson')
) AS [t4] ON [t3].[Id] = [t4].[Id]
WHERE [o].[Discriminator] = N'LeafA'");
        }

        public override void Query_when_group_by()
        {
            base.Query_when_group_by();

            AssertSql(" ");
        }

        public override void Query_when_subquery()
        {
            base.Query_when_subquery();

            AssertSql(
                @"@__p_0='5'

SELECT TOP(@__p_0) [t].[Id], [t].[Discriminator], [t0].[Id], [t1].[Id], [t1].[LeafBAddress_Country_Name], [t2].[Id], [t3].[Id], [t3].[LeafAAddress_Country_Name], [t4].[Id], [t5].[Id], [t5].[BranchAddress_Country_Name], [t6].[Id], [t7].[Id], [t7].[PersonAddress_Country_Name]
FROM (
    SELECT DISTINCT [o].[Id], [o].[Discriminator]
    FROM [OwnedPerson] AS [o]
    WHERE [o].[Discriminator] IN (N'LeafB', N'LeafA', N'Branch', N'OwnedPerson')
) AS [t]
LEFT JOIN (
    SELECT [p.LeafBAddress].*
    FROM [OwnedPerson] AS [p.LeafBAddress]
    WHERE [p.LeafBAddress].[Discriminator] = N'LeafB'
) AS [t0] ON [t].[Id] = [t0].[Id]
LEFT JOIN (
    SELECT [p.LeafBAddress.Country].*
    FROM [OwnedPerson] AS [p.LeafBAddress.Country]
    WHERE [p.LeafBAddress.Country].[Discriminator] = N'LeafB'
) AS [t1] ON [t0].[Id] = [t1].[Id]
LEFT JOIN (
    SELECT [p.LeafAAddress].*
    FROM [OwnedPerson] AS [p.LeafAAddress]
    WHERE [p.LeafAAddress].[Discriminator] = N'LeafA'
) AS [t2] ON [t].[Id] = [t2].[Id]
LEFT JOIN (
    SELECT [p.LeafAAddress.Country].*
    FROM [OwnedPerson] AS [p.LeafAAddress.Country]
    WHERE [p.LeafAAddress.Country].[Discriminator] = N'LeafA'
) AS [t3] ON [t2].[Id] = [t3].[Id]
LEFT JOIN (
    SELECT [p.BranchAddress].*
    FROM [OwnedPerson] AS [p.BranchAddress]
    WHERE [p.BranchAddress].[Discriminator] IN (N'LeafA', N'Branch')
) AS [t4] ON [t].[Id] = [t4].[Id]
LEFT JOIN (
    SELECT [p.BranchAddress.Country].*
    FROM [OwnedPerson] AS [p.BranchAddress.Country]
    WHERE [p.BranchAddress.Country].[Discriminator] IN (N'LeafA', N'Branch')
) AS [t5] ON [t4].[Id] = [t5].[Id]
LEFT JOIN (
    SELECT [p.PersonAddress].*
    FROM [OwnedPerson] AS [p.PersonAddress]
    WHERE [p.PersonAddress].[Discriminator] IN (N'LeafB', N'LeafA', N'Branch', N'OwnedPerson')
) AS [t6] ON [t].[Id] = [t6].[Id]
LEFT JOIN (
    SELECT [p.PersonAddress.Country].*
    FROM [OwnedPerson] AS [p.PersonAddress.Country]
    WHERE [p.PersonAddress.Country].[Discriminator] IN (N'LeafB', N'LeafA', N'Branch', N'OwnedPerson')
) AS [t7] ON [t6].[Id] = [t7].[Id]
ORDER BY [t].[Id]");
        }

        private void AssertSql(params string[] expected)
            => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);

        public class OwnedQuerySqlServerFixture : OwnedQueryFixtureBase
        {
            protected override ITestStoreFactory TestStoreFactory => SqlServerTestStoreFactory.Instance;
            public TestSqlLoggerFactory TestSqlLoggerFactory => (TestSqlLoggerFactory)ServiceProvider.GetRequiredService<ILoggerFactory>();
        }
    }
}
