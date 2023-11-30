// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query;

public abstract class AdHocAdvancedMappingsQueryRelationalTestBase : AdHocAdvancedMappingsQueryTestBase
{
    protected TestSqlLoggerFactory TestSqlLoggerFactory
        => (TestSqlLoggerFactory)ListLoggerFactory;

    protected void ClearLog()
        => TestSqlLoggerFactory.Clear();

    protected void AssertSql(params string[] expected)
        => TestSqlLoggerFactory.AssertBaseline(expected);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Hierarchy_query_with_abstract_type_sibling_TPC(bool async)
        => Hierarchy_query_with_abstract_type_sibling_helper(
            async,
            mb =>
            {
                mb.Entity<Context28196.Animal>().UseTpcMappingStrategy();
                mb.Entity<Context28196.Pet>().ToTable("Pets");
                mb.Entity<Context28196.Cat>().ToTable("Cats");
                mb.Entity<Context28196.Dog>().ToTable("Dogs");
                mb.Entity<Context28196.FarmAnimal>().ToTable("FarmAnimals");
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Hierarchy_query_with_abstract_type_sibling_TPT(bool async)
        => Hierarchy_query_with_abstract_type_sibling_helper(
            async,
            mb =>
            {
                mb.Entity<Context28196.Animal>().UseTptMappingStrategy();
                mb.Entity<Context28196.Pet>().ToTable("Pets");
                mb.Entity<Context28196.Cat>().ToTable("Cats");
                mb.Entity<Context28196.Dog>().ToTable("Dogs");
                mb.Entity<Context28196.FarmAnimal>().ToTable("FarmAnimals");
            });
}
