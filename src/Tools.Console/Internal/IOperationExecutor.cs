// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Tools.Internal
{
    public interface IOperationExecutor : IDisposable
    {
        IDictionary AddMigration([NotNull] string name, [CanBeNull] string outputDir, [CanBeNull] string contextType);
        IEnumerable<string> RemoveMigration([CanBeNull] string contextType, bool force);
        IEnumerable<IDictionary> GetMigrations([CanBeNull] string contextType);
        void DropDatabase([CanBeNull] string contextType);
        IDictionary GetDatabase([CanBeNull] string name);
        string GetContextType([NotNull] string name);
        void UpdateDatabase([CanBeNull] string migration, [CanBeNull] string contextType);
        IEnumerable<IDictionary> GetContextTypes();

        IEnumerable<string> ReverseEngineer(
            [NotNull] string provider,
            [NotNull] string connectionString,
            [CanBeNull] string outputDir,
            [CanBeNull] string dbContextClassName,
            [NotNull] IEnumerable<string> schemaFilters,
            [NotNull] IEnumerable<string> tableFilters,
            bool useDataAnnotations,
            bool overwriteFiles);

        string ScriptMigration([CanBeNull] string fromMigration, [CanBeNull] string toMigration, bool idempotent, [CanBeNull] string contextType);
    }
}
