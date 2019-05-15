// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections;
using System.Collections.Generic;
using System.IO;
using Microsoft.EntityFrameworkCore.Tools.Properties;

namespace Microsoft.EntityFrameworkCore.Tools
{
    internal abstract class OperationExecutorBase : IOperationExecutor
    {
        public const string DesignAssemblyName = "Microsoft.EntityFrameworkCore.Design";
        protected const string ExecutorTypeName = "Microsoft.EntityFrameworkCore.Design.OperationExecutor";

        private static readonly IDictionary _emptyArguments = new Dictionary<string, object>(0);
        public string AppBasePath { get; }

        protected string AssemblyFileName { get; set; }
        protected string StartupAssemblyFileName { get; set; }
        protected string ProjectDirectory { get; }
        protected string RootNamespace { get; }
        protected string Language { get; }

        protected OperationExecutorBase(
            string assembly,
            string startupAssembly,
            string projectDir,
            string rootNamespace,
            string language)
        {
            AssemblyFileName = Path.GetFileNameWithoutExtension(assembly);
            StartupAssemblyFileName = startupAssembly == null
                ? AssemblyFileName
                : Path.GetFileNameWithoutExtension(startupAssembly);

            AppBasePath = Path.GetFullPath(
                Path.Combine(Directory.GetCurrentDirectory(), Path.GetDirectoryName(startupAssembly ?? assembly)));

            RootNamespace = rootNamespace ?? AssemblyFileName;
            ProjectDirectory = projectDir ?? Directory.GetCurrentDirectory();
            Language = language;

            Reporter.WriteVerbose(Resources.UsingAssembly(AssemblyFileName));
            Reporter.WriteVerbose(Resources.UsingStartupAssembly(StartupAssemblyFileName));
            Reporter.WriteVerbose(Resources.UsingApplicationBase(AppBasePath));
            Reporter.WriteVerbose(Resources.UsingWorkingDirectory(Directory.GetCurrentDirectory()));
            Reporter.WriteVerbose(Resources.UsingRootNamespace(RootNamespace));
            Reporter.WriteVerbose(Resources.UsingProjectDir(ProjectDirectory));
        }

        public virtual void Dispose()
        {
        }

        protected abstract dynamic CreateResultHandler();
        protected abstract void Execute(string operationName, object resultHandler, IDictionary arguments);

        private TResult InvokeOperation<TResult>(string operation)
            => InvokeOperation<TResult>(operation, _emptyArguments);

        private TResult InvokeOperation<TResult>(string operation, IDictionary arguments)
            => (TResult)InvokeOperationImpl(operation, arguments);

        private void InvokeOperation(string operation, IDictionary arguments)
            => InvokeOperationImpl(operation, arguments);

        private object InvokeOperationImpl(string operationName, IDictionary arguments)
        {
            var resultHandler = CreateResultHandler();

            Execute(operationName, resultHandler, arguments);

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
            => InvokeOperation<IDictionary>(
                "AddMigration",
                new Dictionary<string, string>
                {
                    ["name"] = name,
                    ["outputDir"] = outputDir,
                    ["contextType"] = contextType
                });

        public IDictionary RemoveMigration(string contextType, bool force)
            => InvokeOperation<IDictionary>(
                "RemoveMigration",
                new Dictionary<string, object>
                {
                    ["contextType"] = contextType,
                    ["force"] = force
                });

        public IEnumerable<IDictionary> GetMigrations(string contextType)
            => InvokeOperation<IEnumerable<IDictionary>>(
                "GetMigrations",
                new Dictionary<string, object>
                {
                    ["contextType"] = contextType
                });

        public void DropDatabase(string contextType)
            => InvokeOperation(
                "DropDatabase",
                new Dictionary<string, object>
                {
                    ["contextType"] = contextType
                });

        public IDictionary GetContextInfo(string name)
            => InvokeOperation<IDictionary>(
                "GetContextInfo",
                new Dictionary<string, object>
                {
                    ["contextType"] = name
                });

        public void UpdateDatabase(string migration, string contextType)
            => InvokeOperation(
                "UpdateDatabase",
                new Dictionary<string, string>
                {
                    ["targetMigration"] = migration,
                    ["contextType"] = contextType
                });

        public IEnumerable<IDictionary> GetContextTypes()
            => InvokeOperation<IEnumerable<IDictionary>>("GetContextTypes");

        public IDictionary ScaffoldContext(
            string provider,
            string connectionString,
            string outputDir,
            string outputDbContextDir,
            string dbContextClassName,
            IEnumerable<string> schemaFilters,
            IEnumerable<string> tableFilters,
            bool useDataAnnotations,
            bool overwriteFiles,
            bool useDatabaseNames)
            => InvokeOperation<IDictionary>(
                "ScaffoldContext",
                new Dictionary<string, object>
                {
                    ["provider"] = provider,
                    ["connectionString"] = connectionString,
                    ["outputDir"] = outputDir,
                    ["outputDbContextDir"] = outputDbContextDir,
                    ["dbContextClassName"] = dbContextClassName,
                    ["schemaFilters"] = schemaFilters,
                    ["tableFilters"] = tableFilters,
                    ["useDataAnnotations"] = useDataAnnotations,
                    ["overwriteFiles"] = overwriteFiles,
                    ["useDatabaseNames"] = useDatabaseNames
                });

        public string ScriptMigration(
            string fromMigration,
            string toMigration,
            bool idempotent,
            string contextType)
            => InvokeOperation<string>(
                "ScriptMigration",
                new Dictionary<string, object>
                {
                    ["fromMigration"] = fromMigration,
                    ["toMigration"] = toMigration,
                    ["idempotent"] = idempotent,
                    ["contextType"] = contextType
                });

        public string ScriptDbContext(string contextType)
            => InvokeOperation<string>(
                "ScriptDbContext",
                new Dictionary<string, object>
                {
                    ["contextType"] = contextType
                });
    }
}
