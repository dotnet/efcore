// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using JetBrains.Annotations;

// ReSharper disable ArgumentsStyleLiteral
namespace Microsoft.EntityFrameworkCore.Tools.Internal
{
    public class ReflectionOperationExecutor : OperationExecutorBase
    {
        private readonly object _executor;
        private readonly Assembly _commandsAssembly;
        private const string LogHandlerTypeName = "Microsoft.EntityFrameworkCore.Design.OperationLogHandler";
        private const string ResultHandlerTypeName = "Microsoft.EntityFrameworkCore.Design.OperationResultHandler";
        private readonly Type _resultHandlerType;

        public ReflectionOperationExecutor([NotNull] string assembly,
            [NotNull] string startupAssembly,
            [NotNull] string projectDir,
            [CanBeNull] string contentRootPath,
            [CanBeNull] string dataDirectory,
            [CanBeNull] string rootNamespace,
            [CanBeNull] string environment)
            : base(assembly, startupAssembly, projectDir, contentRootPath, dataDirectory, rootNamespace, environment)
        {
            _commandsAssembly = Assembly.Load(new AssemblyName { Name = DesignAssemblyName });
            var logHandlerType = _commandsAssembly.GetType(LogHandlerTypeName, throwOnError: true, ignoreCase: false);

            var logHandler = Activator.CreateInstance(logHandlerType, new object[]
            {
                 (Action<string>)Reporter.Error,
                 (Action<string>)Reporter.Warning,
                 (Action<string>)Reporter.Output,
                 (Action<string>)Reporter.Verbose,
                 (Action<string>)Reporter.Verbose
            });

            _executor = Activator.CreateInstance(
                _commandsAssembly.GetType(ExecutorTypeName, throwOnError: true, ignoreCase: false),
                logHandler,
                new Dictionary<string, string>
                {
                    { "targetName", AssemblyFileName },
                    { "startupTargetName", StartupAssemblyFileName },
                    { "projectDir", ProjectDirectory },
                    { "contentRootPath", ContentRootPath },
                    { "rootNamespace", RootNamespace },
                    { "environment", EnvironmentName }
                });

            _resultHandlerType = _commandsAssembly.GetType(ResultHandlerTypeName, throwOnError: true, ignoreCase: false);
        }

        protected override object CreateResultHandler()
            => Activator.CreateInstance(_resultHandlerType);

        protected override void Execute(string operationName, object resultHandler, IDictionary arguments)
        {
            Activator.CreateInstance(
                _commandsAssembly.GetType(ExecutorTypeName + "+" + operationName, throwOnError: true, ignoreCase: true),
                _executor,
                resultHandler,
                arguments);
        }

        public override void Dispose()
        {
        }
    }
}
