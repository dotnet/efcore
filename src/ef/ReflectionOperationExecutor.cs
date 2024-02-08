// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Design.Internal;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Tools.Properties;

namespace Microsoft.EntityFrameworkCore.Tools;

internal class ReflectionOperationExecutor : OperationExecutorBase
{
    private readonly object _executor;
    private readonly Assembly _commandsAssembly;
    private const string ReportHandlerTypeName = "Microsoft.EntityFrameworkCore.Design.OperationReportHandler";
    private const string ResultHandlerTypeName = "Microsoft.EntityFrameworkCore.Design.OperationResultHandler";
    private readonly Type _resultHandlerType;

    public ReflectionOperationExecutor(
        string assembly,
        string? startupAssembly,
        string? projectDir,
        string? dataDirectory,
        string? rootNamespace,
        string? language,
        bool nullable,
        string[] remainingArguments,
        IOperationReportHandler reportHandler)
        : base(assembly, startupAssembly, projectDir, rootNamespace, language, nullable, remainingArguments, reportHandler)
    {
        var reporter = new OperationReporter(reportHandler);
        var configurationFile = (startupAssembly ?? assembly) + ".config";
        if (File.Exists(configurationFile))
        {
            reporter.WriteVerbose(Resources.UsingConfigurationFile(configurationFile));
            AppDomain.CurrentDomain.SetData("APP_CONFIG_FILE", configurationFile);
        }

        if (dataDirectory != null)
        {
            reporter.WriteVerbose(Resources.UsingDataDir(dataDirectory));
            AppDomain.CurrentDomain.SetData("DataDirectory", dataDirectory);
        }

        AppDomain.CurrentDomain.AssemblyResolve += ResolveAssembly;

        _commandsAssembly = Assembly.Load(new AssemblyName { Name = DesignAssemblyName });
        var reportHandlerType = _commandsAssembly.GetType(ReportHandlerTypeName, throwOnError: true, ignoreCase: false)!;

        var designReportHandler = Activator.CreateInstance(
            reportHandlerType,
            (Action<string>)reportHandler.OnError,
            (Action<string>)reportHandler.OnWarning,
            (Action<string>)reportHandler.OnInformation,
            (Action<string>)reportHandler.OnVerbose)!;

        _executor = Activator.CreateInstance(
            _commandsAssembly.GetType(ExecutorTypeName, throwOnError: true, ignoreCase: false)!,
            designReportHandler,
            new Dictionary<string, object?>
            {
                { "targetName", AssemblyFileName },
                { "startupTargetName", StartupAssemblyFileName },
                { "projectDir", ProjectDirectory },
                { "rootNamespace", RootNamespace },
                { "language", Language },
                { "nullable", Nullable },
                { "toolsVersion", ProductInfo.GetVersion() },
                { "remainingArguments", RemainingArguments }
            })!;

        _resultHandlerType = _commandsAssembly.GetType(ResultHandlerTypeName, throwOnError: true, ignoreCase: false)!;
    }

    protected override object CreateResultHandler()
        => Activator.CreateInstance(_resultHandlerType)!;

    protected override void Execute(string operationName, object resultHandler, IDictionary arguments)
        => Activator.CreateInstance(
            _commandsAssembly.GetType(ExecutorTypeName + "+" + operationName, throwOnError: true, ignoreCase: true)!,
            _executor,
            resultHandler,
            arguments);

    private Assembly? ResolveAssembly(object? sender, ResolveEventArgs args)
    {
        var assemblyName = new AssemblyName(args.Name);

        foreach (var extension in new[] { ".dll", ".exe" })
        {
            var path = Path.Combine(AppBasePath, assemblyName.Name + extension);
            if (File.Exists(path))
            {
                try
                {
                    return Assembly.LoadFrom(path);
                }
                catch
                {
                }
            }
        }

        return null;
    }

    public override void Dispose()
        => AppDomain.CurrentDomain.AssemblyResolve -= ResolveAssembly;
}
