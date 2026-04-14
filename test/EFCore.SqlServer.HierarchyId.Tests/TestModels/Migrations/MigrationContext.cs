// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.SqlServer.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.SqlServer.TestModels.Migrations;

internal abstract class MigrationContext<TEntity1, TEntity2> : DbContext
    where TEntity1 : class
    where TEntity2 : class
{
    protected Type ModelType1 { get; } = typeof(TEntity1);
    protected Type ModelType2 { get; } = typeof(TEntity2);

    private Type _thisType;

    protected Type ThisType
        => _thisType ??= GetType();

    public DbSet<TEntity1> TestModels { get; set; }
    public DbSet<TEntity2> ConvertedTestModels { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
        => options
            .UseSqlServer(
                @"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=HierarchyIdMigrationTests",
                x => x.UseHierarchyId());

    /// <summary>
    ///     Removes annotations from the model that can
    ///     change between versions of ef.
    ///     This should be called during OnModelCreating
    /// </summary>
    protected void RemoveVariableModelAnnotations(ModelBuilder modelBuilder)
    {
        var model = modelBuilder.Model;

        //the values of these could change between versions
        //so get rid of them for the tests
        model.RemoveAnnotation(CoreAnnotationNames.ProductVersion);
        model.RemoveAnnotation(RelationalAnnotationNames.MaxIdentifierLength);
        model.RemoveAnnotation(SqlServerAnnotationNames.ValueGenerationStrategy);
    }

    public abstract string GetExpectedMigrationCode(string migrationName, string rootNamespace);
    public abstract string GetExpectedSnapshotCode(string rootNamespace);
}
