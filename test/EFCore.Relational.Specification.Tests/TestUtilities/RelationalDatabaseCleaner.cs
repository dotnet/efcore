// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore.Scaffolding.Metadata;

namespace Microsoft.EntityFrameworkCore.TestUtilities;

public abstract class RelationalDatabaseCleaner
{
    protected abstract IDatabaseModelFactory CreateDatabaseModelFactory(ILoggerFactory loggerFactory);

    protected virtual bool AcceptTable(DatabaseTable table)
        => true;

    protected virtual bool AcceptForeignKey(DatabaseForeignKey foreignKey)
        => true;

    protected virtual bool AcceptIndex(DatabaseIndex index)
        => true;

    protected virtual bool AcceptSequence(DatabaseSequence sequence)
        => true;

    protected virtual string? BuildCustomSql(DatabaseModel databaseModel)
        => null;

    protected virtual string? BuildCustomEndingSql(DatabaseModel databaseModel)
        => null;

    protected virtual void OpenConnection(IRelationalConnection connection)
        => connection.Open();

    public virtual void Clean(DatabaseFacade facade)
    {
        var creator = facade.GetService<IRelationalDatabaseCreator>();
        var sqlGenerator = facade.GetService<IMigrationsSqlGenerator>();
        var executor = facade.GetService<IMigrationCommandExecutor>();
        var connection = facade.GetService<IRelationalConnection>();
        var sqlBuilder = facade.GetService<IRawSqlCommandBuilder>();
        var loggerFactory = facade.GetService<ILoggerFactory>();

        if (!creator.Exists())
        {
            creator.Create();
        }
        else
        {
            var databaseModelFactory = CreateDatabaseModelFactory(loggerFactory);
            var databaseModel = databaseModelFactory.Create(
                connection.DbConnection,
                new DatabaseModelFactoryOptions());

            var operations = new List<MigrationOperation>();

            foreach (var index in databaseModel.Tables
                         .SelectMany(t => t.Indexes.Where(AcceptIndex)))
            {
                operations.Add(Drop(index));
            }

            foreach (var foreignKey in databaseModel.Tables
                         .SelectMany(t => t.ForeignKeys.Where(AcceptForeignKey)))
            {
                operations.Add(Drop(foreignKey));
            }

            foreach (var table in databaseModel.Tables.Where(AcceptTable))
            {
                operations.Add(Drop(table));
            }

            foreach (var sequence in databaseModel.Sequences.Where(AcceptSequence))
            {
                operations.Add(Drop(sequence));
            }

            OpenConnection(connection);

            try
            {
                var customSql = BuildCustomSql(databaseModel);
                if (!string.IsNullOrWhiteSpace(customSql))
                {
                    ExecuteScript(connection, sqlBuilder, customSql);
                }

                if (operations.Count > 0)
                {
                    var commands = sqlGenerator.Generate(operations);
                    executor.ExecuteNonQuery(commands, connection);
                }

                customSql = BuildCustomEndingSql(databaseModel);
                if (!string.IsNullOrWhiteSpace(customSql))
                {
                    ExecuteScript(connection, sqlBuilder, customSql);
                }
            }
            finally
            {
                connection.Close();
            }
        }

        creator.CreateTables();
    }

    private static void ExecuteScript(IRelationalConnection connection, IRawSqlCommandBuilder sqlBuilder, string customSql)
    {
        var batches = Regex.Split(
            Regex.Replace(
                customSql,
                @"\\\r?\n",
                string.Empty,
                default,
                TimeSpan.FromMilliseconds(1000.0)),
            @"^\s*(GO[ \t]+[0-9]+|GO)(?:\s+|$)",
            RegexOptions.IgnoreCase | RegexOptions.Multiline,
            TimeSpan.FromMilliseconds(1000.0));
        for (var i = 0; i < batches.Length; i++)
        {
            if (batches[i].StartsWith("GO", StringComparison.OrdinalIgnoreCase)
                || string.IsNullOrWhiteSpace(batches[i]))
            {
                continue;
            }

            sqlBuilder.Build(batches[i])
                .ExecuteNonQuery(new RelationalCommandParameterObject(connection, null, null, null, null));
        }
    }

    protected virtual MigrationOperation Drop(DatabaseSequence sequence)
        => new DropSequenceOperation { Name = sequence.Name, Schema = sequence.Schema };

    protected virtual MigrationOperation Drop(DatabaseTable table)
        => new DropTableOperation { Name = table.Name, Schema = table.Schema };

    protected virtual MigrationOperation Drop(DatabaseForeignKey foreignKey)
        => new DropForeignKeyOperation
        {
            Name = foreignKey.Name!,
            Table = foreignKey.Table.Name,
            Schema = foreignKey.Table.Schema
        };

    protected virtual MigrationOperation Drop(DatabaseIndex index)
        => new DropIndexOperation
        {
            Name = index.Name!,
            Table = index.Table!.Name,
            Schema = index.Table.Schema
        };
}
