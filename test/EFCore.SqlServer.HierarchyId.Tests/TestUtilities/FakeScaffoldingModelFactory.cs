// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Design.Internal;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Scaffolding;
using Microsoft.EntityFrameworkCore.Scaffolding.Internal;
using Microsoft.EntityFrameworkCore.Scaffolding.Metadata;

namespace Microsoft.EntityFrameworkCore.SqlServer.TestUtilities;

public class FakeScaffoldingModelFactory(
    IOperationReporter reporter,
    ICandidateNamingService candidateNamingService,
    IPluralizer pluralizer,
    ICSharpUtilities cSharpUtilities,
    IScaffoldingTypeMapper scaffoldingTypeMapper,
    IModelRuntimeInitializer modelRuntimeInitializer) : RelationalScaffoldingModelFactory(
        reporter, candidateNamingService, pluralizer, cSharpUtilities, scaffoldingTypeMapper,
        modelRuntimeInitializer)
{
    public override IModel Create(DatabaseModel databaseModel, ModelReverseEngineerOptions options)
    {
        foreach (var sequence in databaseModel.Sequences)
        {
            sequence.Database = databaseModel;
        }

        foreach (var table in databaseModel.Tables)
        {
            table.Database = databaseModel;

            foreach (var column in table.Columns)
            {
                column.Table = table;
            }

            if (table.PrimaryKey != null)
            {
                table.PrimaryKey.Table = table;
                FixupColumns(table, table.PrimaryKey.Columns);
            }

            foreach (var index in table.Indexes)
            {
                index.Table = table;
                FixupColumns(table, index.Columns);
            }

            foreach (var uniqueConstraints in table.UniqueConstraints)
            {
                uniqueConstraints.Table = table;
                FixupColumns(table, uniqueConstraints.Columns);
            }

            foreach (var foreignKey in table.ForeignKeys)
            {
                foreignKey.Table = table;
                FixupColumns(table, foreignKey.Columns);

                if (foreignKey.PrincipalTable is DatabaseTableRef tableRef)
                {
                    foreignKey.PrincipalTable = databaseModel.Tables
                        .First(t => t.Name == tableRef.Name && t.Schema == tableRef.Schema);
                }

                FixupColumns(foreignKey.PrincipalTable, foreignKey.PrincipalColumns);
            }
        }

        return base.Create(databaseModel, options);
    }

    private static void FixupColumns(DatabaseTable table, IList<DatabaseColumn> columns)
    {
        for (var i = 0; i < columns.Count; i++)
        {
            if (columns[i] is DatabaseColumnRef columnRef)
            {
                columns[i] = table.Columns.First(c => c.Name == columnRef.Name);
            }

            columns[i].Table = table;
        }
    }
}

internal class DatabaseTableRef : DatabaseTable
{
    public DatabaseTableRef(string name, string schema = null)
    {
        Name = name;
        Schema = schema;
    }

    public override DatabaseModel Database
    {
        get => throw new NotImplementedException();
        set => throw new NotImplementedException();
    }

    public override string Comment
    {
        get => throw new NotImplementedException();
        set => throw new NotImplementedException();
    }

    public override DatabasePrimaryKey PrimaryKey
    {
        get => throw new NotImplementedException();
        set => throw new NotImplementedException();
    }

    public override IList<DatabaseColumn> Columns
        => throw new NotImplementedException();

    public override IList<DatabaseUniqueConstraint> UniqueConstraints
        => throw new NotImplementedException();

    public override IList<DatabaseIndex> Indexes
        => throw new NotImplementedException();

    public override IList<DatabaseForeignKey> ForeignKeys
        => throw new NotImplementedException();
}

internal class DatabaseColumnRef : DatabaseColumn
{
    public DatabaseColumnRef(string name)
    {
        Name = name;
    }

    public override DatabaseTable Table
    {
        get => throw new NotImplementedException();
        set => throw new NotImplementedException();
    }

    public override bool IsNullable
    {
        get => throw new NotImplementedException();
        set => throw new NotImplementedException();
    }

    public override string StoreType
    {
        get => throw new NotImplementedException();
        set => throw new NotImplementedException();
    }

    public override string DefaultValueSql
    {
        get => throw new NotImplementedException();
        set => throw new NotImplementedException();
    }

    public override string ComputedColumnSql
    {
        get => throw new NotImplementedException();
        set => throw new NotImplementedException();
    }

    public override string Comment
    {
        get => throw new NotImplementedException();
        set => throw new NotImplementedException();
    }

    public override ValueGenerated? ValueGenerated
    {
        get => throw new NotImplementedException();
        set => throw new NotImplementedException();
    }
}
