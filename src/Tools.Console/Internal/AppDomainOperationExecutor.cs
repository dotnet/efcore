// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

#if NET451
using System;
using System.Collections;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Design;

namespace Microsoft.EntityFrameworkCore.Tools.Internal
{
    public class AppDomainOperationExecutor : OperationExecutorBase
    {
        private readonly object _executor;
        private readonly AppDomain _domain;
        private bool _disposed;

        public AppDomainOperationExecutor([NotNull] string assembly,
            [NotNull] string startupAssembly,
            [NotNull] string projectDir,
            [CanBeNull] string contentRootPath,
            [CanBeNull] string dataDirectory,
            [CanBeNull] string rootNamespace,
            [CanBeNull] string environment,
            [CanBeNull] string configFile)
            : base(assembly, startupAssembly, projectDir, contentRootPath, dataDirectory, rootNamespace, environment)
        {
            var info = new AppDomainSetup
            {
                ApplicationBase = AppBasePath,
                ConfigurationFile = configFile
            };

            _domain = AppDomain.CreateDomain("EntityFrameworkCore.DesignDomain", null, info);

            if (!string.IsNullOrEmpty(dataDirectory))
            {
                _domain.SetData("DataDirectory", dataDirectory);
            }

            _executor = _domain.CreateInstanceAndUnwrap(DesignAssemblyName,
                ExecutorTypeName,
                false,
                BindingFlags.Default,
                null,
                new object[]
                {
                    LogHandler,
                    new Hashtable
                    {
                        { "targetName", AssemblyFileName },
                        { "startupTargetName", StartupAssemblyFileName },
                        { "projectDir", ProjectDirectory },
                        { "contentRootPath", ContentRootPath },
                        { "rootNamespace", RootNamespace },
                        { "environment", EnvironmentName }
                    }
                },
                null, null);
        }

        protected override void Execute(string operationName, IOperationResultHandler resultHandler, IDictionary arguments)
        {
            _domain.CreateInstance(
                DesignAssemblyName,
                ExecutorTypeName + "+" + operationName,
                false,
                BindingFlags.Default,
                null,
                new[] { _executor, resultHandler, arguments },
                null,
                null);
        }

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
#endif