// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Microsoft.EntityFrameworkCore.Tools.Properties;

namespace Microsoft.EntityFrameworkCore.Tools
{
    internal abstract class OperationExecutorBase : IOperationExecutor
    {
        private const string DataDirEnvName = "ADONET_DATA_DIR";
        public const string DesignAssemblyName = "Microsoft.EntityFrameworkCore.Design";
        protected const string ExecutorTypeName = "Microsoft.EntityFrameworkCore.Design.OperationExecutor";

        private static readonly IDictionary EmptyArguments = new Dictionary<string, object>(0);
        public string AppBasePath { get; }

        protected string AssemblyFileName { get; set; }
        protected string StartupAssemblyFileName { get; set; }
        protected string ContentRootPath { get; }
        protected string ProjectDirectory { get; }
        protected string RootNamespace { get; }
        protected string EnvironmentName { get; }

        protected OperationExecutorBase(
            string assembly,
            string startupAssembly,
            string projectDir,
            string contentRootPath,
            string dataDirectory,
            string rootNamespace,
            string environment)
        {
            AssemblyFileName = Path.GetFileNameWithoutExtension(assembly);
            StartupAssemblyFileName = startupAssembly == null
                ? AssemblyFileName
                : Path.GetFileNameWithoutExtension(startupAssembly);

            AppBasePath = Path.GetDirectoryName(startupAssembly ?? assembly);
            if (!Path.IsPathRooted(AppBasePath))
            {
                AppBasePath = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), AppBasePath));
            }

            ContentRootPath = contentRootPath ?? AppBasePath;
            RootNamespace = rootNamespace ?? AssemblyFileName;
            ProjectDirectory = projectDir ?? Directory.GetCurrentDirectory();
            EnvironmentName = environment;

            Reporter.WriteVerbose(Resources.UsingAssembly(AssemblyFileName));
            Reporter.WriteVerbose(Resources.UsingStartupAssembly(StartupAssemblyFileName));
            Reporter.WriteVerbose(Resources.UsingApplicationBase(AppBasePath));
            Reporter.WriteVerbose(Resources.UsingContentRoot(ContentRootPath));
            Reporter.WriteVerbose(Resources.UsingRootNamespace(RootNamespace));
            Reporter.WriteVerbose(Resources.UsingProjectDir(ProjectDirectory));

            if (dataDirectory != null)
            {
                Reporter.WriteVerbose(Resources.UsingDataDir(dataDirectory));
                Environment.SetEnvironmentVariable(DataDirEnvName, dataDirectory);
            }
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

            var currentDirectory = Directory.GetCurrentDirectory();
            Directory.SetCurrentDirectory(AppBasePath);
            try
            {
                Execute(operationName, resultHandler, arguments);
            }
            finally
            {
                Directory.SetCurrentDirectory(currentDirectory);
            }

            if (resultHandler.ErrorType != null)
            {
                throw new WrappedException(
                    resultHandler.ErrorType,
                    resultHandler.ErrorMessage,
                    resultHandler.ErrorStackTrace);
            }

            return resultHandler.Result;
        }

        public IDictionary AddMigration(string name, string outputDir, string contextType)
            => InvokeOperation<IDictionary>("AddMigration",
                new Dictionary<string, string>
                {
                    ["name"] = name,
                    ["outputDir"] = outputDir,
                    ["contextType"] = contextType
                });

        public IEnumerable<string> RemoveMigration(string contextType, bool force)
            => InvokeOperation<IEnumerable<string>>("RemoveMigration",
                new Dictionary<string, object>
                {
                    ["contextType"] = contextType,
                    ["force"] = force
                });

        public IEnumerable<IDictionary> GetMigrations(string contextType)
            => InvokeOperation<IEnumerable<IDictionary>>("GetMigrations",
                new Dictionary<string, object>
                {
                    ["contextType"] = contextType
                });

        public void DropDatabase(string contextType)
            => InvokeOperation("DropDatabase",
                new Dictionary<string, object>
                {
                    ["contextType"] = contextType
                });

        public IDictionary GetContextInfo(string name)
            => InvokeOperation<IDictionary>("GetContextInfo",
                new Dictionary<string, object>
                {
                    ["contextType"] = name
                });

        public void UpdateDatabase(string migration, string contextType)
            => InvokeOperation("UpdateDatabase",
                new Dictionary<string, string>
                {
                    ["targetMigration"] = migration,
                    ["contextType"] = contextType
                });

        public IEnumerable<IDictionary> GetContextTypes()
            => InvokeOperation<IEnumerable<IDictionary>>("GetContextTypes");

        public IEnumerable<string> ScaffoldContext(string provider,
            string connectionString,
            string outputDir,
            string dbContextClassName,
            IEnumerable<string> schemaFilters,
            IEnumerable<string> tableFilters,
            bool useDataAnnotations,
            bool overwriteFiles)
            => InvokeOperation<IEnumerable<string>>("ScaffoldContext",
                new Dictionary<string, object>
                {
                    ["provider"] = provider,
                    ["connectionString"] = connectionString,
                    ["outputDir"] = outputDir,
                    ["dbContextClassName"] = dbContextClassName,
                    ["schemaFilters"] = schemaFilters,
                    ["tableFilters"] = tableFilters,
                    ["useDataAnnotations"] = useDataAnnotations,
                    ["overwriteFiles"] = overwriteFiles
                });

        public string ScriptMigration(
            string fromMigration,
            string toMigration,
            bool idempotent,
            string contextType)
            => InvokeOperation<string>("ScriptMigration",
                new Dictionary<string, object>
                {
                    ["fromMigration"] = fromMigration,
                    ["toMigration"] = toMigration,
                    ["idempotent"] = idempotent,
                    ["contextType"] = contextType
                });

        public string GetContextType(string name)
            => InvokeOperation<string>("GetContextType",
                new Dictionary<string, string>
                {
                    ["name"] = name
                });
    }
}
