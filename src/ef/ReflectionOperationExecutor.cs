// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections;
using System.Reflection;
#if NET
using System.Runtime.Loader;
#endif
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
    private string? _efcoreVersion;
#if NET
    private AssemblyLoadContext? _assemblyLoadContext;
#endif

    public ReflectionOperationExecutor(
        string assembly,
        string? startupAssembly,
        string? designAssembly,
        string? project,
        string? projectDir,
        string? dataDirectory,
        string? rootNamespace,
        string? language,
        bool nullable,
        string[] remainingArguments,
        IOperationReportHandler reportHandler)
        : base(assembly, startupAssembly, designAssembly, project, projectDir, rootNamespace, language, nullable, remainingArguments, reportHandler)
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

#if NET
        _commandsAssembly = AssemblyLoadContext.LoadFromAssemblyName(new AssemblyName(DesignAssemblyName));
#else
        if (DesignAssemblyPath != null)
        {
            var assemblyPath = Path.GetDirectoryName(DesignAssemblyPath);
            assemblyPath = Path.Combine(assemblyPath, DesignAssemblyName + ".dll");
            _commandsAssembly = Assembly.LoadFrom(assemblyPath);
        }
        else
        {
            _commandsAssembly = Assembly.Load(DesignAssemblyName);
        }
#endif
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
                { "project", Project },
                { "projectDir", ProjectDirectory },
                { "rootNamespace", RootNamespace },
                { "language", Language },
                { "nullable", Nullable },
                { "toolsVersion", ProductInfo.GetVersion() },
                { "remainingArguments", RemainingArguments }
            })!;

        _resultHandlerType = _commandsAssembly.GetType(ResultHandlerTypeName, throwOnError: true, ignoreCase: false)!;
    }

#if NET
    protected AssemblyLoadContext AssemblyLoadContext
    {
        get
        {
            if (_assemblyLoadContext != null)
            {
                return _assemblyLoadContext;
            }

            if (DesignAssemblyPath != null)
            {
                AssemblyLoadContext.Default.Resolving += (context, name) =>
                {
                    var assemblyPath = Path.GetDirectoryName(DesignAssemblyPath)!;
                    assemblyPath = Path.Combine(assemblyPath, name.Name + ".dll");
                    return File.Exists(assemblyPath) ? context.LoadFromAssemblyPath(assemblyPath) : null;
                };
                _assemblyLoadContext = AssemblyLoadContext.Default;
            }

            return AssemblyLoadContext.Default;
        }
    }
#endif

    public override string? EFCoreVersion
    {
        get
        {
            if (_efcoreVersion != null)
            {
                return _efcoreVersion;
            }

            Assembly? assembly = null;
#if NET
            assembly = AssemblyLoadContext.LoadFromAssemblyName(new AssemblyName(DesignAssemblyName));
#else
            if (DesignAssemblyPath != null)
            {
                var assemblyPath = Path.GetDirectoryName(DesignAssemblyPath);
                assemblyPath = Path.Combine(assemblyPath, DesignAssemblyName + ".dll");
                assembly = Assembly.LoadFrom(assemblyPath);
            }
            else
            {
                assembly = Assembly.Load(DesignAssemblyName);
            }
#endif
            _efcoreVersion = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()
                    ?.InformationalVersion;
            return _efcoreVersion;
        }
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
