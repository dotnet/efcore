namespace Microsoft.EntityFrameworkCore.Query;

public abstract class OwnedEntityQueryRelationalTestBase(NonSharedFixture fixture)
    : OwnedEntityQueryTestBase(fixture)
{
    protected TestSqlLoggerFactory TestSqlLoggerFactory => (TestSqlLoggerFactory)ListLoggerFactory;

    protected override void ClearLog() => TestSqlLoggerFactory.Clear();

    protected void AssertSql(params string[] expected) => TestSqlLoggerFactory.AssertBaseline(expected);
}