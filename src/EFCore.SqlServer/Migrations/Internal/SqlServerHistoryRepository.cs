// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Text;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.SqlServer.Migrations.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class SqlServerHistoryRepository : HistoryRepository
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public SqlServerHistoryRepository([NotNull] HistoryRepositoryDependencies dependencies)
            : base(dependencies)
        {
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override string ExistsSql
        {
            get
            {
                var stringTypeMapping = Dependencies.TypeMappingSource.GetMapping(typeof(string));

                return "SELECT OBJECT_ID(" +
                       stringTypeMapping.GenerateSqlLiteral(
                           SqlGenerationHelper.DelimitIdentifier(TableName, TableSchema)) +
                       ")" + SqlGenerationHelper.StatementTerminator;
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override bool InterpretExistsResult(object value)
            => value != DBNull.Value;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override string GetCreateIfNotExistsScript()
        {
            var stringTypeMapping = Dependencies.TypeMappingSource.GetMapping(typeof(string));

            return new IndentedStringBuilder()
                .Append("IF OBJECT_ID(")
                .Append(
                    stringTypeMapping.GenerateSqlLiteral(
                        SqlGenerationHelper.DelimitIdentifier(TableName, TableSchema)))
                .AppendLine(") IS NULL")
                .AppendLine("BEGIN")
                .IncrementIndent()
                .AppendLines(GetCreateScript())
                .DecrementIndent()
                .Append("END")
                .AppendLine(SqlGenerationHelper.StatementTerminator)
                .ToString();
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override string GetBeginIfNotExistsScript(string migrationId)
        {
            Check.NotEmpty(migrationId, nameof(migrationId));

            var stringTypeMapping = Dependencies.TypeMappingSource.GetMapping(typeof(string));

            return new StringBuilder()
                .Append("IF NOT EXISTS(SELECT * FROM ")
                .Append(SqlGenerationHelper.DelimitIdentifier(TableName, TableSchema))
                .Append(" WHERE ")
                .Append(SqlGenerationHelper.DelimitIdentifier(MigrationIdColumnName))
                .Append(" = ")
                .Append(stringTypeMapping.GenerateSqlLiteral(migrationId))
                .AppendLine(")")
                .Append("BEGIN")
                .ToString();
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override string GetBeginIfExistsScript(string migrationId)
        {
            Check.NotEmpty(migrationId, nameof(migrationId));

            var stringTypeMapping = Dependencies.TypeMappingSource.GetMapping(typeof(string));

            return new StringBuilder()
                .Append("IF EXISTS(SELECT * FROM ")
                .Append(SqlGenerationHelper.DelimitIdentifier(TableName, TableSchema))
                .Append(" WHERE ")
                .Append(SqlGenerationHelper.DelimitIdentifier(MigrationIdColumnName))
                .Append(" = ")
                .Append(stringTypeMapping.GenerateSqlLiteral(migrationId))
                .AppendLine(")")
                .Append("BEGIN")
                .ToString();
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override string GetEndIfScript()
            => new StringBuilder()
                .Append("END")
                .AppendLine(SqlGenerationHelper.StatementTerminator)
                .ToString();
    }
}
