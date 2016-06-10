// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

#if NET451

using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Design;

namespace Microsoft.EntityFrameworkCore.Tools.FunctionalTests.TestUtilities
{
    // NOTE: If you break this file, you probably broke the PowerShell module too.
    public class AppDomainOperationExecutor : IDisposable
    {
        private const string AssemblyName = "Microsoft.EntityFrameworkCore.Tools";
        private const string TypeName = "Microsoft.EntityFrameworkCore.Design.OperationExecutor";

        private readonly AppDomain _domain;
        private readonly object _executor;

        public AppDomainOperationExecutor(
            string targetDir,
            string targetName,
            string projectDir,
            string contentRootPath,
            string rootNamespace)
        {
            _domain = AppDomain.CreateDomain(
                "ExecutorWrapper",
                null,
                new AppDomainSetup
                {
                    ApplicationBase = targetDir,
                    ConfigurationFile = AppDomain.CurrentDomain.SetupInformation.ConfigurationFile,
                    ShadowCopyFiles = "true"
                });
            _executor = _domain.CreateInstanceAndUnwrap(
                AssemblyName,
                TypeName,
                false,
                0,
                null,
                new object[]
                {
                    // TODO: Pass this in
                    new OperationLogHandler(),
                    new Hashtable
                    {
                        { "targetName", targetName },
                        { "startupTargetName", targetName },
                        { "projectDir", projectDir },
                        { "contentRootPath", contentRootPath },
                        { "rootNamespace", rootNamespace }
                    }
                },
                null,
                null);
        }

        public string GetContextType(string name)
            => InvokeOperation<string>("GetContextType", new Hashtable { { "name", name } });

        public IDictionary AddMigration(string name, string outputDir, string contextType)
            => InvokeOperation<IDictionary>(
                "AddMigration",
                new Hashtable { { "name", name }, { "outputDir", outputDir }, { "contextType", contextType } });

        public string ScriptMigration(
            string fromMigration,
            string toMigration,
            bool idempotent,
            string contextType)
            => InvokeOperation<string>(
                "ScriptMigration",
                new Hashtable
                {
                    { "fromMigration", fromMigration },
                    { "toMigration", toMigration },
                    { "idempotent", idempotent },
                    { "contextType", contextType }
                });

        public IEnumerable<IDictionary> GetContextTypes()
            => InvokeOperation<IEnumerable<IDictionary>>("GetContextTypes");

        public IEnumerable<IDictionary> GetMigrations(string contextType)
            => InvokeOperation<IEnumerable<IDictionary>>(
                "GetMigrations",
                new Hashtable { { "contextType", contextType } });

        public void Dispose()
            => AppDomain.Unload(_domain);

        private TResult InvokeOperation<TResult>(string operation)
            => InvokeOperation<TResult>(operation, new Hashtable());

        private TResult InvokeOperation<TResult>(string operation, Hashtable arguments)
            => (TResult)InvokeOperationImpl(operation, arguments);

        private void InvokeOperation(string operation, Hashtable arguments)
            => InvokeOperationImpl(operation, arguments, isVoid: true);

        private object InvokeOperationImpl(string operation, Hashtable arguments, bool isVoid = false)
        {
            var resultHandler = new OperationResultHandler();

            // TODO: Set current directory
            _domain.CreateInstance(
                AssemblyName,
                TypeName + "+" + operation,
                false,
                0,
                null,
                new[] { _executor, resultHandler, arguments },
                null,
                null);

            if (resultHandler.ErrorType != null)
            {
                throw new OperationException(resultHandler.ErrorMessage);
            }
            if (!isVoid
                && !resultHandler.HasResult)
            {
                throw new InvalidOperationException(
                    $"A value was not returned for operation '{operation}'.");
            }

            return resultHandler.Result;
        }
    }
}

#endif
