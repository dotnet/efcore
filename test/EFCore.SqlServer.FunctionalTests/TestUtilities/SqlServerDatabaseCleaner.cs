// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Scaffolding.Metadata;
using Microsoft.EntityFrameworkCore.SqlServer.Design.Internal;
using Microsoft.EntityFrameworkCore.SqlServer.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.TestUtilities;

public class SqlServerDatabaseCleaner : RelationalDatabaseCleaner
{
    protected override IDatabaseModelFactory CreateDatabaseModelFactory(ILoggerFactory loggerFactory)
    {
        var services = new ServiceCollection();
        services.AddEntityFrameworkSqlServer();

        new SqlServerDesignTimeServices().ConfigureDesignTimeServices(services);

        return services
            .BuildServiceProvider() // No scope validation; cleaner violates scopes, but only resolve services once.
            .GetRequiredService<IDatabaseModelFactory>();
    }

    protected override bool AcceptTable(DatabaseTable table)
        => table is not DatabaseView;

    protected override bool AcceptIndex(DatabaseIndex index)
        => false;

    private readonly string _dropViewsSql = @"
DECLARE @name varchar(max) = '__dummy__', @SQL varchar(max) = '';

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

    protected override string BuildCustomSql(DatabaseModel databaseModel)
        => _dropViewsSql;

    protected override string BuildCustomEndingSql(DatabaseModel databaseModel)
        => _dropViewsSql
            + @"
GO

DECLARE @SQL varchar(max) = '';
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

    protected override MigrationOperation Drop(DatabaseTable table)
        => AddSqlServerSpecificAnnotations(base.Drop(table), table);

    protected override MigrationOperation Drop(DatabaseForeignKey foreignKey)
        => AddSqlServerSpecificAnnotations(base.Drop(foreignKey), foreignKey.Table);

    protected override MigrationOperation Drop(DatabaseIndex index)
        => AddSqlServerSpecificAnnotations(base.Drop(index), index.Table!);

    private static TOperation AddSqlServerSpecificAnnotations<TOperation>(TOperation operation, DatabaseTable table)
        where TOperation : MigrationOperation
    {
        operation[SqlServerAnnotationNames.MemoryOptimized]
            = table[SqlServerAnnotationNames.MemoryOptimized] as bool?;

        if (table[SqlServerAnnotationNames.IsTemporal] != null)
        {
            operation[SqlServerAnnotationNames.IsTemporal]
                = table[SqlServerAnnotationNames.IsTemporal];

            operation[SqlServerAnnotationNames.TemporalHistoryTableName]
                = table[SqlServerAnnotationNames.TemporalHistoryTableName];

            operation[SqlServerAnnotationNames.TemporalHistoryTableSchema]
                = table[SqlServerAnnotationNames.TemporalHistoryTableSchema];

            operation[SqlServerAnnotationNames.TemporalPeriodStartColumnName]
                = table[SqlServerAnnotationNames.TemporalPeriodStartColumnName];

            operation[SqlServerAnnotationNames.TemporalPeriodEndColumnName]
                = table[SqlServerAnnotationNames.TemporalPeriodEndColumnName];
        }

        return operation;
    }
}
