## Release 2.1.0

### New Rules
Rule ID | Category | Severity | Notes
--------|----------|----------|-------
EF1000  | Usage    | Warning  | RawSqlStringInjectionDiagnosticAnalyzer, [Documentation](https://docs.microsoft.com/ef/core/querying/raw-sql)

## Release 3.0.0

### New Rules
Rule ID | Category | Severity | Notes
--------|----------|----------|-------
EF1001  | Usage    | Warning  | InternalUsageDiagnosticAnalyzer

### Removed Rules
Rule ID | Category | Severity | Notes
--------|----------|----------|--------------------
EF1000  | Security | Disabled | RawSqlStringInjectionDiagnosticAnalyzer, [Documentation](https://docs.microsoft.com/ef/core/querying/raw-sql)

## Release 8.0.0

### New Rules
Rule ID | Category | Severity | Notes
--------|----------|----------|-------
EF1002  | Security | Warning  | StringsUsageInRawQueriesDiagnosticAnalyzer, [Documentation](https://learn.microsoft.com/ef/core/querying/sql-queries#passing-parameters)

## Release 10.0.0

### New Rules
Rule ID | Category | Severity | Notes
--------|----------|----------|-------
EF1003  | Security | Warning  | StringsUsageInRawQueriesDiagnosticAnalyzer, [Documentation](https://learn.microsoft.com/ef/core/querying/sql-queries#passing-parameters)