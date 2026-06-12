// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Data;
using System.Text;
using Microsoft.EntityFrameworkCore.Scaffolding.Metadata;
using Microsoft.EntityFrameworkCore.Sqlite.Design.Internal;

namespace Microsoft.EntityFrameworkCore.TestUtilities;

public class SqliteDatabaseCleaner : RelationalDatabaseCleaner
{
    protected override IDatabaseModelFactory CreateDatabaseModelFactory(ILoggerFactory loggerFactory)
    {
        var services = new ServiceCollection();
        services.AddEntityFrameworkSqlite();

        new SqliteDesignTimeServices().ConfigureDesignTimeServices(services);

        return services
            .BuildServiceProvider() // No scope validation; cleaner violates scopes, but only resolve services once.
            .GetRequiredService<IDatabaseModelFactory>();
    }

    protected override bool AcceptTable(DatabaseTable table)
        => table is not DatabaseView;

    protected override bool AcceptForeignKey(DatabaseForeignKey foreignKey)
        => false;

    protected override bool AcceptIndex(DatabaseIndex index)
        => false;

    protected override string BuildCustomSql(DatabaseModel databaseModel)
    {
        var sb = new StringBuilder();
        sb.AppendLine("PRAGMA foreign_keys=OFF;");

        foreach (var view in databaseModel.Tables.OfType<DatabaseView>())
        {
            sb.AppendLine($"DROP VIEW IF EXISTS \"{view.Name}\";");
        }

        return sb.ToString();
    }

    protected override string BuildCustomEndingSql(DatabaseModel databaseModel)
        => "PRAGMA foreign_keys=ON;";

    public override void Clean(DatabaseFacade facade, bool createTables = true)
    {
        var connection = facade.GetDbConnection();

        var opened = false;
        if (connection.State != ConnectionState.Open)
        {
            connection.Open();
            opened = true;
        }

        var command = connection.CreateCommand();
        command.CommandText = "SELECT COUNT(*) FROM sqlite_master WHERE name = 'geometry_columns' AND type = 'table';";

        var hasGeometryColumns = (long)command.ExecuteScalar()! != 0L;
        if (hasGeometryColumns)
        {
            // NB: SUM forces DiscardGeometryColumn to evaluate for each row
            command.CommandText = "SELECT SUM(DiscardGeometryColumn(f_table_name, f_geometry_column)) FROM geometry_columns;";
            command.ExecuteNonQuery();
        }

        if (opened)
        {
            connection.Close();
        }

        base.Clean(facade, createTables);
    }
}
