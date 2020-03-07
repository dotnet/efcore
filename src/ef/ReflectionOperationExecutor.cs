// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Tools.Properties;

namespace Microsoft.EntityFrameworkCore.Tools
{
    internal class ReflectionOperationExecutor : OperationExecutorBase
    {
        private readonly object _executor;
        private readonly Assembly _commandsAssembly;
        private const string ReportHandlerTypeName = "Microsoft.EntityFrameworkCore.Design.OperationReportHandler";
        private const string ResultHandlerTypeName = "Microsoft.EntityFrameworkCore.Design.OperationResultHandler";
        private readonly Type _resultHandlerType;

        public ReflectionOperationExecutor(
            string assembly,
            string startupAssembly,
            string projectDir,
            string dataDirectory,
            string rootNamespace,
            string language)
            : base(assembly, startupAssembly, projectDir, rootNamespace, language)
        {
            if (dataDirectory != null)
            {
                Reporter.WriteVerbose(Resources.UsingDataDir(dataDirectory));
                AppDomain.CurrentDomain.SetData("DataDirectory", dataDirectory);
            }

            AppDomain.CurrentDomain.AssemblyResolve += ResolveAssembly;

            _commandsAssembly = Assembly.Load(new AssemblyName { Name = DesignAssemblyName });
            var reportHandlerType = _commandsAssembly.GetType(ReportHandlerTypeName, throwOnError: true, ignoreCase: false);

            var reportHandler = Activator.CreateInstance(
                reportHandlerType,
                (Action<string>)Reporter.WriteError,
                (Action<string>)Reporter.WriteWarning,
                (Action<string>)Reporter.WriteInformation,
                (Action<string>)Reporter.WriteVerbose);

            _executor = Activator.CreateInstance(
                _commandsAssembly.GetType(ExecutorTypeName, throwOnError: true, ignoreCase: false),
                reportHandler,
                new Dictionary<string, string>
                {
                    { "targetName", AssemblyFileName },
                    { "startupTargetName", StartupAssemblyFileName },
                    { "projectDir", ProjectDirectory },
                    { "rootNamespace", RootNamespace },
                    { "language", Language },
                    { "toolsVersion", ProductInfo.GetVersion() }
                });

            _resultHandlerType = _commandsAssembly.GetType(ResultHandlerTypeName, throwOnError: true, ignoreCase: false);
        }

        protected override object CreateResultHandler()
            => Activator.CreateInstance(_resultHandlerType);

        protected override void Execute(string operationName, object resultHandler, IDictionary arguments)
            => Activator.CreateInstance(
                _commandsAssembly.GetType(ExecutorTypeName + "+" + operationName, throwOnError: true, ignoreCase: true),
                _executor,
                resultHandler,
                arguments);

        private Assembly ResolveAssembly(object sender, ResolveEventArgs args)
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
}
