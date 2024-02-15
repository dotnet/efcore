// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections;

namespace Microsoft.EntityFrameworkCore.Tools;

internal interface IOperationExecutor : IDisposable
{
    IDictionary AddMigration(string name, string? outputDir, string? contextType, string? @namespace);
    IDictionary RemoveMigration(string? contextType, bool force);
    IEnumerable<IDictionary> GetMigrations(string? contextType, string? connectionString, bool noConnect);
    void DropDatabase(string? contextType);
    IDictionary GetContextInfo(string? name);
    void UpdateDatabase(string? migration, string? connectionString, string? contextType);
    IEnumerable<IDictionary> GetContextTypes();
    IEnumerable<string> OptimizeContext(string? outputDir, string? modelNamespace, string? contextType);

    IDictionary ScaffoldContext(
        string provider,
        string connectionString,
        string? outputDir,
        string? outputDbContextDir,
        string? dbContextClassName,
        IEnumerable<string> schemaFilters,
        IEnumerable<string> tableFilters,
        bool useDataAnnotations,
        bool overwriteFiles,
        bool useDatabaseNames,
        string? entityNamespace,
        string? dbContextNamespace,
        bool suppressOnConfiguring,
        bool noPluralize);

    string ScriptMigration(string? fromMigration, string? toMigration, bool idempotent, bool noTransactions, string? contextType);

    string ScriptDbContext(string? contextType);
    void HasPendingModelChanges(string? contextType);
}
