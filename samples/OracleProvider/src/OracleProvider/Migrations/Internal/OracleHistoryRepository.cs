// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Text;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Oracle.Migrations.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class OracleHistoryRepository : HistoryRepository
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public OracleHistoryRepository([NotNull] HistoryRepositoryDependencies dependencies)
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
                var builder = new StringBuilder();

                builder
                    .Append("SELECT t.table_name ")
                    .Append("FROM all_tables t ")
                    .Append("WHERE t.table_name = ")
                    .Append(stringTypeMapping.GenerateSqlLiteral(TableName));

                if (TableSchema != null)
                {
                    builder
                        .Append(" AND t.tablespace_name = ")
                        .Append(stringTypeMapping.GenerateSqlLiteral(TableSchema));
                }

                return builder.ToString();
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override bool InterpretExistsResult(object value) => value != null;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override string GetCreateIfNotExistsScript()
        {
            var builder = new IndentedStringBuilder();

            return builder.Append(
@"BEGIN
  EXECUTE IMMEDIATE '" + GetCreateScript() + @"';
EXCEPTION
WHEN OTHERS THEN
  IF(SQLCODE != -942)THEN
      RAISE;
  END IF;
END;").ToString();
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
                .AppendLine("DECLARE")
                .AppendLine("    v_Count INTEGER;")
                .AppendLine("BEGIN")
                .Append("SELECT COUNT(*) INTO v_Count FROM ")
                .Append(SqlGenerationHelper.DelimitIdentifier(TableName, TableSchema))
                .Append(" WHERE ")
                .Append(SqlGenerationHelper.DelimitIdentifier(MigrationIdColumnName))
                .Append(" = ")
                .AppendLine(stringTypeMapping.GenerateSqlLiteral(migrationId))
                .AppendLine("IF v_Count = 0 THEN")
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
                .AppendLine("DECLARE")
                .AppendLine("    v_Count INTEGER;")
                .AppendLine("BEGIN")
                .Append("SELECT COUNT(*) INTO v_Count FROM ")
                .Append(SqlGenerationHelper.DelimitIdentifier(TableName, TableSchema))
                .Append(" WHERE ")
                .Append(SqlGenerationHelper.DelimitIdentifier(MigrationIdColumnName))
                .Append(" = ")
                .AppendLine(stringTypeMapping.GenerateSqlLiteral(migrationId))
                .AppendLine("IF v_Count = 1 THEN")
                .ToString();
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override string GetEndIfScript()
            => new StringBuilder()
                .AppendLine(" END IF")
                .AppendLine("END")
                .ToString();
    }
}
