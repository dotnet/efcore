// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.TestUtilities;

public class TestRelationalMigrationSqlGenerator(MigrationsSqlGeneratorDependencies dependencies) : MigrationsSqlGenerator(dependencies)
{
    protected override void Generate(RenameTableOperation operation, IModel model, MigrationCommandListBuilder builder)
    {
    }

    protected override void Generate(
        DropIndexOperation operation,
        IModel model,
        MigrationCommandListBuilder builder,
        bool terminate = true)
    {
    }

    protected override void Generate(RenameSequenceOperation operation, IModel model, MigrationCommandListBuilder builder)
    {
    }

    protected override void Generate(RenameColumnOperation operation, IModel model, MigrationCommandListBuilder builder)
    {
    }

    protected override void Generate(EnsureSchemaOperation operation, IModel model, MigrationCommandListBuilder builder)
    {
    }

    protected override void Generate(RenameIndexOperation operation, IModel model, MigrationCommandListBuilder builder)
    {
    }

    protected override void Generate(AlterColumnOperation operation, IModel model, MigrationCommandListBuilder builder)
    {
    }
}
