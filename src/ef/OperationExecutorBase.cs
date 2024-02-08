// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections;
using Microsoft.EntityFrameworkCore.Tools.Properties;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Design.Internal;

namespace Microsoft.EntityFrameworkCore.Tools;

internal abstract class OperationExecutorBase : IOperationExecutor
{
    public const string DesignAssemblyName = "Microsoft.EntityFrameworkCore.Design";
    protected const string ExecutorTypeName = "Microsoft.EntityFrameworkCore.Design.OperationExecutor";

    private static readonly IDictionary EmptyArguments = new Dictionary<string, object>(0);
    public string AppBasePath { get; }

    protected string AssemblyFileName { get; set; }
    protected string StartupAssemblyFileName { get; set; }
    protected string ProjectDirectory { get; }
    protected string RootNamespace { get; }
    protected string? Language { get; }
    protected bool Nullable { get; }
    protected string[] RemainingArguments { get; }

    protected OperationExecutorBase(
        string assembly,
        string? startupAssembly,
        string? projectDir,
        string? rootNamespace,
        string? language,
        bool nullable,
        string[] remainingArguments,
        IOperationReportHandler reportHandler)
    {
        AssemblyFileName = Path.GetFileNameWithoutExtension(assembly);
        StartupAssemblyFileName = startupAssembly == null
            ? AssemblyFileName
            : Path.GetFileNameWithoutExtension(startupAssembly);

        AppBasePath = Path.GetFullPath(
            Path.Combine(Directory.GetCurrentDirectory(), Path.GetDirectoryName(startupAssembly ?? assembly)!));

        RootNamespace = rootNamespace ?? AssemblyFileName;
        ProjectDirectory = projectDir ?? Directory.GetCurrentDirectory();
        Language = language;
        Nullable = nullable;
        RemainingArguments = remainingArguments ?? [];

        var reporter = new OperationReporter(reportHandler);
        reporter.WriteVerbose(Resources.UsingAssembly(AssemblyFileName));
        reporter.WriteVerbose(Resources.UsingStartupAssembly(StartupAssemblyFileName));
        reporter.WriteVerbose(Resources.UsingApplicationBase(AppBasePath));
        reporter.WriteVerbose(Resources.UsingWorkingDirectory(Directory.GetCurrentDirectory()));
        reporter.WriteVerbose(Resources.UsingRootNamespace(RootNamespace));
        reporter.WriteVerbose(Resources.UsingProjectDir(ProjectDirectory));
        reporter.WriteVerbose(Resources.RemainingArguments(string.Join(",", RemainingArguments.Select(s => "'" + s + "'"))));
    }

    public virtual void Dispose()
    {
    }

    protected abstract dynamic CreateResultHandler();
    protected abstract void Execute(string operationName, object resultHandler, IDictionary arguments);

    private TResult InvokeOperation<TResult>(string operation)
        => InvokeOperation<TResult>(operation, EmptyArguments);

    private TResult InvokeOperation<TResult>(string operation, IDictionary arguments)
        => (TResult)InvokeOperationImpl(operation, arguments);

    private void InvokeOperation(string operation, IDictionary arguments)
        => InvokeOperationImpl(operation, arguments);

    private object InvokeOperationImpl(string operationName, IDictionary arguments)
    {
        var resultHandler = CreateResultHandler();

        Execute(operationName, resultHandler, arguments);

        if (resultHandler.ErrorType != null)
        {
            throw new WrappedException(
                resultHandler.ErrorType,
                resultHandler.ErrorMessage,
                resultHandler.ErrorStackTrace);
        }

        return resultHandler.Result;
    }

    public IDictionary AddMigration(string name, string? outputDir, string? contextType, string? @namespace)
        => InvokeOperation<IDictionary>(
            "AddMigration",
            new Dictionary<string, object?>
            {
                ["name"] = name,
                ["outputDir"] = outputDir,
                ["contextType"] = contextType,
                ["namespace"] = @namespace
            });

    public IDictionary RemoveMigration(string? contextType, bool force)
        => InvokeOperation<IDictionary>(
            "RemoveMigration",
            new Dictionary<string, object?> { ["contextType"] = contextType, ["force"] = force });

    public IEnumerable<IDictionary> GetMigrations(string? contextType, string? connectionString, bool noConnect)
        => InvokeOperation<IEnumerable<IDictionary>>(
            "GetMigrations",
            new Dictionary<string, object?>
            {
                ["contextType"] = contextType,
                ["connectionString"] = connectionString,
                ["noConnect"] = noConnect
            });

    public void DropDatabase(string? contextType)
        => InvokeOperation(
            "DropDatabase",
            new Dictionary<string, object?> { ["contextType"] = contextType });

    public IDictionary GetContextInfo(string? name)
        => InvokeOperation<IDictionary>(
            "GetContextInfo",
            new Dictionary<string, object?> { ["contextType"] = name });

    public void UpdateDatabase(string? migration, string? connectionString, string? contextType)
        => InvokeOperation(
            "UpdateDatabase",
            new Dictionary<string, object?>
            {
                ["targetMigration"] = migration,
                ["connectionString"] = connectionString,
                ["contextType"] = contextType
            });

    public IEnumerable<IDictionary> GetContextTypes()
        => InvokeOperation<IEnumerable<IDictionary>>("GetContextTypes");

    public IEnumerable<string> OptimizeContext(string? outputDir, string? modelNamespace, string? contextType)
        => InvokeOperation<IEnumerable<string>>(
            "OptimizeContext",
            new Dictionary<string, object?>
            {
                ["outputDir"] = outputDir,
                ["modelNamespace"] = modelNamespace,
                ["contextType"] = contextType
            });

    public IDictionary ScaffoldContext(
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
        string? modelNamespace,
        string? contextNamespace,
        bool suppressOnConfiguring,
        bool noPluralize)
        => InvokeOperation<IDictionary>(
            "ScaffoldContext",
            new Dictionary<string, object?>
            {
                ["provider"] = provider,
                ["connectionString"] = connectionString,
                ["outputDir"] = outputDir,
                ["outputDbContextDir"] = outputDbContextDir,
                ["dbContextClassName"] = dbContextClassName,
                ["schemaFilters"] = schemaFilters,
                ["tableFilters"] = tableFilters,
                ["useDataAnnotations"] = useDataAnnotations,
                ["overwriteFiles"] = overwriteFiles,
                ["useDatabaseNames"] = useDatabaseNames,
                ["modelNamespace"] = modelNamespace,
                ["contextNamespace"] = contextNamespace,
                ["suppressOnConfiguring"] = suppressOnConfiguring,
                ["noPluralize"] = noPluralize
            });

    public string ScriptMigration(
        string? fromMigration,
        string? toMigration,
        bool idempotent,
        bool noTransactions,
        string? contextType)
        => InvokeOperation<string>(
            "ScriptMigration",
            new Dictionary<string, object?>
            {
                ["fromMigration"] = fromMigration,
                ["toMigration"] = toMigration,
                ["idempotent"] = idempotent,
                ["noTransactions"] = noTransactions,
                ["contextType"] = contextType
            });

    public string ScriptDbContext(string? contextType)
        => InvokeOperation<string>(
            "ScriptDbContext",
            new Dictionary<string, object?> { ["contextType"] = contextType });

    public void HasPendingModelChanges(string? contextType)
        => InvokeOperation<string>(
            "HasPendingModelChanges",
            new Dictionary<string, object?> { ["contextType"] = contextType });
}
