// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

#if NET451

using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Design;

namespace Microsoft.EntityFrameworkCore.Commands.TestUtilities
{
    // NOTE: If you break this file, you probably broke the PowerShell module too.
    public class OperationExecutorWrapper : IDisposable
    {
        private const string AssemblyName = "Microsoft.EntityFrameworkCore.Commands";
        private const string TypeName = "Microsoft.EntityFrameworkCore.Design.OperationExecutor";

        private readonly string _targetDir;
        private readonly AppDomain _domain;
        private readonly object _executor;

        public OperationExecutorWrapper(string targetDir, string targetName, string projectDir, string rootNamespace)
        {
            _targetDir = targetDir;
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
                                { "rootNamespace", rootNamespace }
                            }
                    },
                null,
                null);
        }

        public string GetContextType(string name)
        {
            return InvokeOperation<string>("GetContextType", new Hashtable { { "name", name } });
        }

        public IEnumerable<string> AddMigration(string name, string outputDir, string contextType)
        {
            return InvokeOperation<IEnumerable<string>>(
                "AddMigration",
                new Hashtable { { "name", name }, { "outputDir", outputDir }, { "contextType", contextType } });
        }

        public void UpdateDatabase(string targetMigration, string contextType)
        {
            InvokeOperation(
                "UpdateDatabase",
                new Hashtable { { "targetMigration", targetMigration }, { "contextType", contextType } });
        }

        public string ScriptMigration(
            string fromMigration,
            string toMigration,
            bool idempotent,
            string contextType)
        {
            return InvokeOperation<string>(
                "ScriptMigration",
                new Hashtable
                    {
                        { "fromMigration", fromMigration },
                        { "toMigration", toMigration },
                        { "idempotent", idempotent },
                        { "contextType", contextType }
                    });
        }

        public string RemoveMigration(string contextType)
            => InvokeOperation<string>("RemoveMigration", new Hashtable { { "contextType", contextType } });

        public IEnumerable<IDictionary> GetContextTypes()
        {
            return InvokeOperation<IEnumerable<IDictionary>>("GetContextTypes");
        }

        public IEnumerable<IDictionary> GetMigrations(string contextType)
        {
            return InvokeOperation<IEnumerable<IDictionary>>(
                "GetMigrations",
                new Hashtable { { "contextType", contextType } });
        }

        public IEnumerable<string> ReverseEngineer(
            string connectionString,
            string provider,
            string relativeOutputDir,
            bool useFluentApiOnly)
            => InvokeOperation<IEnumerable<string>>(
                "ReverseEngineer",
                new Hashtable
                {
                    { "connectionString", connectionString },
                    { "provider", provider },
                    { "relativeOutputDir", relativeOutputDir },
                    { "useFluentApiOnly", useFluentApiOnly }
                });

        public IEnumerable<string> ScaffoldRuntimeDirectives()
            => InvokeOperation<IEnumerable<string>>("ScaffoldRuntimeDirectives", new Hashtable());

        public void Dispose()
        {
            AppDomain.Unload(_domain);
        }

        private TResult InvokeOperation<TResult>(string operation)
        {
            return InvokeOperation<TResult>(operation, new Hashtable());
        }

        private TResult InvokeOperation<TResult>(string operation, Hashtable arguments)
        {
            return (TResult)InvokeOperationImpl(operation, arguments);
        }

        private void InvokeOperation(string operation, Hashtable arguments)
        {
            InvokeOperationImpl(operation, arguments, isVoid: true);
        }

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
                throw new WrappedOperationException(
                    resultHandler.ErrorMessage,
                    resultHandler.ErrorStackTrace,
                    resultHandler.ErrorType);
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
