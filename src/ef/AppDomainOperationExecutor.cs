// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#if NET472
using System;
using System.Collections;
using System.IO;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Design.Internal;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Tools.Properties;

namespace Microsoft.EntityFrameworkCore.Tools
{
    internal class AppDomainOperationExecutor : OperationExecutorBase
    {
        private readonly object _executor;
        private readonly AppDomain _domain;
        private bool _disposed;
        private const string ReportHandlerTypeName = "Microsoft.EntityFrameworkCore.Design.OperationReportHandler";

        public AppDomainOperationExecutor(
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
            var info = new AppDomainSetup { ApplicationBase = AppBasePath };

            var reporter = new OperationReporter(reportHandler);
            var configurationFile = (startupAssembly ?? assembly) + ".config";
            if (File.Exists(configurationFile))
            {
                reporter.WriteVerbose(Resources.UsingConfigurationFile(configurationFile));
                info.ConfigurationFile = configurationFile;
            }

            _domain = AppDomain.CreateDomain("EntityFrameworkCore.DesignDomain", null, info);

            if (dataDirectory != null)
            {
                reporter.WriteVerbose(Resources.UsingDataDir(dataDirectory));
                _domain.SetData("DataDirectory", dataDirectory);
            }

            var designReportHandler = _domain.CreateInstanceAndUnwrap(
                DesignAssemblyName,
                ReportHandlerTypeName,
                false,
                BindingFlags.Default,
                null,
                [
                    (Action<string>)reportHandler.OnError,
                    (Action<string>)reportHandler.OnWarning,
                    (Action<string>)reportHandler.OnInformation,
                    (Action<string>)reportHandler.OnVerbose
                ],
                null,
                null);

            _executor = _domain.CreateInstanceAndUnwrap(
                DesignAssemblyName,
                ExecutorTypeName,
                false,
                BindingFlags.Default,
                null,
                [
                    designReportHandler,
                    new Hashtable
                    {
                        { "targetName", AssemblyFileName },
                        { "startupTargetName", StartupAssemblyFileName },
                        { "projectDir", ProjectDirectory },
                        { "rootNamespace", RootNamespace },
                        { "language", Language },
                        { "nullable", Nullable },
                        { "toolsVersion", ProductInfo.GetVersion() },
                        { "remainingArguments", RemainingArguments }
                    }
                ],
                null,
                null);
        }

        protected override object CreateResultHandler()
            => new OperationResultHandler();

        protected override void Execute(string operationName, object resultHandler, IDictionary arguments)
            => _domain.CreateInstance(
                DesignAssemblyName,
                ExecutorTypeName + "+" + operationName,
                false,
                BindingFlags.Default,
                null,
                [_executor, resultHandler, arguments],
                null,
                null);

        public override void Dispose()
        {
            if (!_disposed)
            {
                _disposed = true;
                AppDomain.Unload(_domain);
            }
        }
    }
}
#elif NETCOREAPP2_0_OR_GREATER
#else
#error target frameworks need to be updated.
#endif
