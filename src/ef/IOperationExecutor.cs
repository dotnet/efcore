// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;

namespace Microsoft.EntityFrameworkCore.Tools
{
    internal interface IOperationExecutor : IDisposable
    {
        IDictionary AddMigration(string name, string outputDir, string contextType);
        IDictionary RemoveMigration(string contextType, bool force);
        IEnumerable<IDictionary> GetMigrations(string contextType);
        void DropDatabase(string contextType);
        IDictionary GetContextInfo(string name);
        void UpdateDatabase(string migration, string contextType);
        IEnumerable<IDictionary> GetContextTypes();

        IDictionary ScaffoldContext(
            string provider,
            string connectionString,
            string outputDir,
            string outputDbContextDir,
            string dbContextClassName,
            IEnumerable<string> schemaFilters,
            IEnumerable<string> tableFilters,
            bool useDataAnnotations,
            bool overwriteFiles,
            bool useDatabaseNames);

        string ScriptMigration(string fromMigration, string toMigration, bool idempotent, string contextType);

        string ScriptDbContext(string contextType);
    }
}
