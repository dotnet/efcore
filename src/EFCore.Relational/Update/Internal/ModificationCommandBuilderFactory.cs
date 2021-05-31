// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Microsoft.EntityFrameworkCore.Update.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public class ModificationCommandBuilderFactory : IModificationCommandBuilderFactory
    {
        private readonly IModificationCommandFactory _modificationCommandFactory;
        private readonly IColumnModificationFactory _columnModificationFactory;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public ModificationCommandBuilderFactory(ModificationCommandBuilderFactoryDependencies dependencies)
        {
            _modificationCommandFactory = dependencies.ModificationCommandFactory;
            _columnModificationFactory = dependencies.ColumnModificationFactory;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual IModificationCommandBuilder CreateModificationCommandBuilder(
            string tableName,
            string? schemaName,
            Func<string> generateParameterName,
            bool sensitiveLoggingEnabled,
            IComparer<IUpdateEntry>? comparer,
            IDiagnosticsLogger<DbLoggerCategory.Update>? logger)
        {
            return new ModificationCommandBuilder(
                tableName,
                schemaName,
                generateParameterName,
                sensitiveLoggingEnabled,
                comparer,
                _modificationCommandFactory,
                _columnModificationFactory,
                logger);
        }
    }
}
