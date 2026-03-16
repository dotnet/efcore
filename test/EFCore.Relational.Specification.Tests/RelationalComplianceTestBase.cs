// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore;

#nullable disable

public abstract class RelationalComplianceTestBase : ComplianceTestBase
{
    protected override IEnumerable<Type> GetBaseTestClasses()
        => base.GetBaseTestClasses().Concat(
            typeof(RelationalComplianceTestBase).Assembly.ExportedTypes.Where(t => t.Name.Contains("TestBase")));

    [ConditionalFact]
    public virtual void All_query_test_fixtures_must_implement_ITestSqlLoggerFactory()
    {
        var queryFixturesWithoutTestSqlLogger = TargetAssembly
            .GetTypes()
            .Where(x => x.BaseType != typeof(object) && (x.IsPublic || x.IsNestedPublic))
            .Where(x => typeof(IQueryFixtureBase).IsAssignableFrom(x))
            .Where(x => !x.GetInterfaces().Contains(typeof(ITestSqlLoggerFactory)))
            .ToList();

        Assert.False(
            queryFixturesWithoutTestSqlLogger.Count > 0,
            "\r\n-- Missing ITestSqlLoggerFactory implementation for relational QueryFixtures --\r\n"
            + string.Join(Environment.NewLine, queryFixturesWithoutTestSqlLogger));
    }
}
