# Issue #38836 triage

Issue: [Named default constraint removal incorrectly targets temporal history table](https://github.com/dotnet/efcore/issues/38836)

## Result

**Still reproduces on `main`.**

Tested on 2026-08-21 at commit `3772265dcb488ddf97e667c05e40f8a87e0e26f8`, which matched `origin/main`.

## Method

I adapted the existing
`MigrationsSqlServerTest.Temporal_table_with_default_constraint_can_alter_column`
test to isolate removal of a named default constraint:

- The source temporal table's `Name` property used
  `.HasMaxLength(50).HasDefaultValue("DefaultName", "DF_Customer_Name")`.
- The target model retained `.HasMaxLength(50)` and removed only the default.
- No production code was changed.

After restoring and activating the repository environment, I built
`EFCore.SqlServer.FunctionalTests` and ran only the adapted test:

```text
artifacts/bin/EFCore.SqlServer.FunctionalTests/Debug/net11.0/Microsoft.EntityFrameworkCore.SqlServer.FunctionalTests \
  --filter-method '*Temporal_table_with_default_constraint_can_alter_column*'
```

## Observed output

The project built successfully with no warnings or errors. The focused test
failed while applying the generated migration to SQL Server:

```text
Microsoft.Data.SqlClient.SqlException : 'DF_Customer_Name' is not a constraint.
Could not drop constraint. See previous errors.
```

The exception originated while executing the source-to-target migration in
`MigrationsTestBase.Test` at the point where generated migration commands are
applied. This matches the failure reported in the issue: EF Core successfully
drops the named constraint from the current temporal table, then attempts to
drop the same constraint from the history table, where it does not exist.

The temporary test adaptation was reverted after verification.
