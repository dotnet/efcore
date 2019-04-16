// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using Microsoft.EntityFrameworkCore.Utilities;
using Oracle.ManagedDataAccess.Client;

namespace Microsoft.EntityFrameworkCore.Oracle.Storage.Internal
{
    public class OracleRelationalCommandBuilderFactory : RelationalCommandBuilderFactory
    {
        public OracleRelationalCommandBuilderFactory(
            [NotNull] IDiagnosticsLogger<DbLoggerCategory.Database.Command> logger,
            [NotNull] IRelationalTypeMappingSource typeMappingSource)
            : base(logger, typeMappingSource)
        {
        }

        protected override IRelationalCommandBuilder CreateCore(
            IDiagnosticsLogger<DbLoggerCategory.Database.Command> logger,
            IRelationalTypeMappingSource relationalTypeMappingSource)
            => new OracleRelationalCommandBuilder(logger, relationalTypeMappingSource);

        private sealed class OracleRelationalCommandBuilder : RelationalCommandBuilder
        {
            public OracleRelationalCommandBuilder(
                IDiagnosticsLogger<DbLoggerCategory.Database.Command> logger,
                IRelationalTypeMappingSource typeMappingSource)
                : base(logger, typeMappingSource)
            {
            }

            protected override IRelationalCommand BuildCore(
                IDiagnosticsLogger<DbLoggerCategory.Database.Command> logger,
                string commandText,
                IReadOnlyList<IRelationalParameter> parameters)
                => new OracleRelationalCommand(logger, commandText, parameters);

            private sealed class OracleRelationalCommand : RelationalCommand
            {
                public OracleRelationalCommand(
                    IDiagnosticsLogger<DbLoggerCategory.Database.Command> logger,
                    string commandText,
                    IReadOnlyList<IRelationalParameter> parameters)
                    : base(logger, commandText, parameters)
                {
                }

                protected override void ConfigureCommand(DbCommand command)
                    => ((OracleCommand)command).BindByName = true;

                protected override string AdjustCommandText(string commandText)
                {
                    commandText = commandText.Trim();

                    return !commandText.StartsWith("BEGIN", StringComparison.OrdinalIgnoreCase)
                           && !commandText.StartsWith("DECLARE", StringComparison.OrdinalIgnoreCase)
                           && !commandText.StartsWith("CREATE OR REPLACE", StringComparison.OrdinalIgnoreCase)
                           && commandText.EndsWith(";", StringComparison.Ordinal)
                        ? commandText.Substring(0, commandText.Length - 1)
                        : commandText;
                }
            }
        }
    }
}
