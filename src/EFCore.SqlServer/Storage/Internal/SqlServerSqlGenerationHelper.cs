// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Text;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.SqlServer.Storage.Internal
{
    /// <summary>
    ///     <para>
    ///         This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///         the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///         any release. You should only use it directly in your code with extreme caution and knowing that
    ///         doing so can result in application failures when updating to a new Entity Framework Core release.
    ///     </para>
    ///     <para>
    ///         The service lifetime is <see cref="ServiceLifetime.Singleton" />. This means a single instance
    ///         is used by many <see cref="DbContext" /> instances. The implementation must be thread-safe.
    ///         This service cannot depend on services registered as <see cref="ServiceLifetime.Scoped" />.
    ///     </para>
    /// </summary>
    public class SqlServerSqlGenerationHelper : RelationalSqlGenerationHelper
    {
        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public SqlServerSqlGenerationHelper([NotNull] RelationalSqlGenerationHelperDependencies dependencies)
            : base(dependencies)
        {
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public override string BatchTerminator
            => "GO" + Environment.NewLine + Environment.NewLine;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public override string StartTransactionStatement
            => "BEGIN TRANSACTION" + StatementTerminator;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public override string EscapeIdentifier(string identifier)
            => Check.NotEmpty(identifier, nameof(identifier)).Replace("]", "]]");

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public override void EscapeIdentifier(StringBuilder builder, string identifier)
        {
            Check.NotEmpty(identifier, nameof(identifier));

            var initialLength = builder.Length;
            builder.Append(identifier);
            builder.Replace("]", "]]", initialLength, identifier.Length);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public override string DelimitIdentifier(string identifier)
            => $"[{EscapeIdentifier(Check.NotEmpty(identifier, nameof(identifier)))}]"; // Interpolation okay; strings

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public override void DelimitIdentifier(StringBuilder builder, string identifier)
        {
            Check.NotEmpty(identifier, nameof(identifier));

            builder.Append('[');
            EscapeIdentifier(builder, identifier);
            builder.Append(']');
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public override void StartMigrationScript(IndentedStringBuilder builder, bool noTransactions)
        {
            if (!noTransactions)
            {
                // "null" out the context, which we will use to verify that SQLCMD is available
                builder.Append("SET CONTEXT_INFO 0x0").AppendLine(StatementTerminator);
                builder.Append(BatchTerminator);

                // Instruct SQLCMD to exit when an error is encountered in a batch
                builder.Append(GenerateComment("Abort execution if an error is encountered in a batch"));
                builder.AppendLine(":ON ERROR EXIT").AppendLine();

                // Set the context to "EF" after the SQLCMD statement so that we can detect in the next batch
                // if there was an error (meaning that SQLCMD is not available)
                builder.Append("SET CONTEXT_INFO 0xEF").AppendLine(StatementTerminator);
                builder.Append(BatchTerminator);

                // If the previous batch didn't set the context to EF then we can assume SQLCMD is not available
                // so we print an error message to the console and set NOEXEC so that the rest of the migration
                // script does not execute
                builder.AppendLine("IF CONTEXT_INFO() <> 0xEF");
                builder.AppendLine("BEGIN");

                using (builder.Indent())
                {
                    builder.Append("PRINT 'ERROR: Entity Framework scripts should be run in SQLCMD mode. Enable SQL > Execution Settings > SQLCMD Mode and try again.'");
                    builder.AppendLine(StatementTerminator).AppendLine();

                    builder.AppendLines(GenerateComment("Disable execution if not running in SQLCMD mode"));
                    builder.Append("SET NOEXEC ON").AppendLine(StatementTerminator);
                }

                builder.Append("END").AppendLine(StatementTerminator);
                builder.Append(BatchTerminator);

                // Enable XACT_ABORT to instruct SQL server to rollback the current transaction on failure
                builder.Append(GenerateComment("Automatically rollback the current transaction when a SQL statement raises a runtime error"));
                builder.Append("SET XACT_ABORT ON").AppendLine(StatementTerminator).AppendLine();
            }

            // Enable QUOTED_IDENTIFIER which is required to create filtered indexes when using SQLCMD
            // see: https://docs.microsoft.com/en-us/sql/t-sql/statements/set-quoted-identifier-transact-sql?view=sql-server-ver15#remarks
            builder.Append(GenerateComment("Must be ON when you are creating a filtered index."));
            builder.Append("SET QUOTED_IDENTIFIER ON").AppendLine(StatementTerminator);
            builder.Append(BatchTerminator);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public override void EndMigrationScript(IndentedStringBuilder builder, bool noTransactions)
        {
            if (!noTransactions)
            {
                builder.Append("SET NOEXEC OFF").AppendLine(StatementTerminator);
            }

            builder.Append("SET QUOTED_IDENTIFIER OFF").AppendLine(StatementTerminator);
            builder.Append(BatchTerminator);
        }
    }
}
