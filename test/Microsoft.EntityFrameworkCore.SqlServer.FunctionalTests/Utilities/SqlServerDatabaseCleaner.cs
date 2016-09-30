// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Scaffolding.Internal;
using Microsoft.EntityFrameworkCore.Scaffolding.Metadata;
using Microsoft.EntityFrameworkCore.Tests;
using Microsoft.Extensions.Logging;

namespace Microsoft.EntityFrameworkCore.SqlServer.FunctionalTests.Utilities
{
    public class SqlServerDatabaseCleaner : RelationalDatabaseCleaner
    {
        protected override IInternalDatabaseModelFactory CreateDatabaseModelFactory(ILoggerFactory loggerFactory)
            => new SqlServerDatabaseModelFactory(loggerFactory.CreateLogger<SqlServerDatabaseModelFactory>());

        protected override bool AcceptIndex(IndexModel index)
            => false;

        protected override string BuildCustomSql(DatabaseModel databaseModel)
            => @"
DECLARE @name VARCHAR(MAX) = '__dummy__', @SQL VARCHAR(MAX);

WHILE @name IS NOT NULL
BEGIN
	SELECT @name =
	(SELECT TOP 1 QUOTENAME(s.[name]) + '.' + QUOTENAME(o.[name])
	 FROM sysobjects o
	 INNER JOIN sys.views v ON o.id = v.object_id
	 INNER JOIN sys.schemas s ON s.schema_id = v.schema_id
	 WHERE (s.name = 'dbo' OR s.principal_id <> s.schema_id) AND o.[type] = 'V' AND o.category = 0 AND o.[name] NOT IN
	 (
		SELECT referenced_entity_name
		FROM sys.sql_expression_dependencies AS sed
		INNER JOIN sys.objects AS o ON sed.referencing_id = o.object_id
	 )
	 ORDER BY v.[name])

    SELECT @SQL = 'DROP VIEW ' + @name
    EXEC (@SQL)
END";

        protected override string BuildCustomEndingSql(DatabaseModel databaseModel)
            => @"
DECLARE @SQL VARCHAR(MAX) = '';
SELECT @SQL = @SQL + 'DROP FUNCTION ' + QUOTENAME(ROUTINE_SCHEMA) + '.' + QUOTENAME(ROUTINE_NAME) + ';'
  FROM [INFORMATION_SCHEMA].[ROUTINES] WHERE ROUTINE_TYPE = 'FUNCTION' AND ROUTINE_BODY = 'SQL';
EXEC (@SQL);

SET @SQL ='';
SELECT @SQL = @SQL + 'DROP AGGREGATE ' + QUOTENAME(ROUTINE_SCHEMA) + '.' + QUOTENAME(ROUTINE_NAME) + ';'
  FROM [INFORMATION_SCHEMA].[ROUTINES] WHERE ROUTINE_TYPE = 'FUNCTION' AND ROUTINE_BODY = 'EXTERNAL';
EXEC (@SQL);

SET @SQL ='';
SELECT @SQL = @SQL + 'DROP PROC ' + QUOTENAME(schema_name(schema_id)) + '.' + QUOTENAME(name) + ';' FROM sys.procedures;
EXEC (@SQL);

SET @SQL ='';
SELECT @SQL = @SQL + 'DROP TYPE ' + QUOTENAME(schema_name(schema_id)) + '.' + QUOTENAME(name) + ';' FROM sys.types WHERE is_user_defined = 1;
EXEC (@SQL);

SET @SQL ='';
SELECT @SQL = @SQL + 'DROP SCHEMA ' + QUOTENAME(name) + ';' FROM sys.schemas WHERE principal_id <> schema_id;
EXEC (@SQL);";

        protected override DropTableOperation Drop(TableModel table)
            => AddMemoryOptimizedAnnotation(base.Drop(table), table);

        protected override DropForeignKeyOperation Drop(ForeignKeyModel foreignKey)
            => AddMemoryOptimizedAnnotation(base.Drop(foreignKey), foreignKey.Table);

        protected override DropIndexOperation Drop(IndexModel index)
            => AddMemoryOptimizedAnnotation(base.Drop(index), index.Table);

        private static TOperation AddMemoryOptimizedAnnotation<TOperation>(TOperation operation, TableModel table)
            where TOperation : MigrationOperation
        {
            operation[SqlServerFullAnnotationNames.Instance.MemoryOptimized]
                = table[SqlServerFullAnnotationNames.Instance.MemoryOptimized] as bool?;

            return operation;
        }
    }
}
