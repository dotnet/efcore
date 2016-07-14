// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

#if NETCOREAPP1_0
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Design;

// ReSharper disable ArgumentsStyleLiteral
namespace Microsoft.EntityFrameworkCore.Tools.Internal
{
    public class ReflectionOperationExecutor : OperationExecutorBase
    {
        private readonly object _executor;
        private readonly Assembly _commandsAssembly;

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

            _executor = Activator.CreateInstance(
                _commandsAssembly.GetType(ExecutorTypeName, throwOnError: true, ignoreCase: false),
                LogHandler,
                new Dictionary<string, string>
                {
                    { "targetName", AssemblyFileName },
                    { "startupTargetName", StartupAssemblyFileName },
                    { "projectDir", ProjectDirectory },
                    { "contentRootPath", ContentRootPath },
                    { "rootNamespace", RootNamespace },
                    { "environment", EnvironmentName }
                });
        }

        public override void Dispose()
        {
        }

        protected override void Execute(string operationName, IOperationResultHandler resultHandler, IDictionary arguments)
        {
            Activator.CreateInstance(
                _commandsAssembly.GetType(ExecutorTypeName + "+" + operationName, throwOnError: true, ignoreCase: true),
                _executor,
                resultHandler,
                arguments);
        }
    }
}
#endif
