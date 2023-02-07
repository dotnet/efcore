// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestModels.Northwind;

namespace Microsoft.EntityFrameworkCore.Query;

public class NorthwindCompiledQuerySqliteTest : NorthwindCompiledQueryTestBase<NorthwindQuerySqliteFixture<NoopModelCustomizer>>
{
    public NorthwindCompiledQuerySqliteTest(NorthwindQuerySqliteFixture<NoopModelCustomizer> fixture)
        : base(fixture)
    {
    }

    public override void MakeBinary_does_not_throw_for_unsupported_operator()
        => Assert.Equal(
            CoreStrings.TranslationFailedWithDetails(
                "DbSet<Customer>()    .Where(c => c.CustomerID == (string)__parameters        .ElementAt(0))",
                CoreStrings.QueryUnableToTranslateMethod("System.Linq.Enumerable", nameof(Enumerable.ElementAt))),
            Assert.Throws<InvalidOperationException>(
                () => base.MakeBinary_does_not_throw_for_unsupported_operator()).Message.Replace("\r", "").Replace("\n", ""));

    public override void Query_with_array_parameter()
    {
        var query = EF.CompileQuery(
            (NorthwindContext context, string[] args)
                => context.Customers.Where(c => c.CustomerID == args[0]));

        using (var context = CreateContext())
        {
            Assert.Equal(
                CoreStrings.TranslationFailedWithDetails(
                    "DbSet<Customer>()    .Where(c => c.CustomerID == __args        .ElementAt(0))",
                    CoreStrings.QueryUnableToTranslateMethod("System.Linq.Enumerable", nameof(Enumerable.ElementAt))),
                Assert.Throws<InvalidOperationException>(
                    () => query(context, new[] { "ALFKI" }).First().CustomerID).Message.Replace("\r", "").Replace("\n", ""));
        }

        using (var context = CreateContext())
        {
            Assert.Equal(
                CoreStrings.TranslationFailedWithDetails(
                    "DbSet<Customer>()    .Where(c => c.CustomerID == __args        .ElementAt(0))",
                    CoreStrings.QueryUnableToTranslateMethod("System.Linq.Enumerable", nameof(Enumerable.ElementAt))),
                Assert.Throws<InvalidOperationException>(
                    () => query(context, new[] { "ANATR" }).First().CustomerID).Message.Replace("\r", "").Replace("\n", ""));
        }
    }

    public override async Task Query_with_array_parameter_async()
    {
        var query = EF.CompileAsyncQuery(
            (NorthwindContext context, string[] args)
                => context.Customers.Where(c => c.CustomerID == args[0]));

        using (var context = CreateContext())
        {
            Assert.Equal(
                CoreStrings.TranslationFailedWithDetails(
                    "DbSet<Customer>()    .Where(c => c.CustomerID == __args        .ElementAt(0))",
                    CoreStrings.QueryUnableToTranslateMethod("System.Linq.Enumerable", nameof(Enumerable.ElementAt))),
                (await Assert.ThrowsAsync<InvalidOperationException>(
                    () => Enumerate(query(context, new[] { "ALFKI" })))).Message.Replace("\r", "").Replace("\n", ""));
        }

        using (var context = CreateContext())
        {
            Assert.Equal(
                CoreStrings.TranslationFailedWithDetails(
                    "DbSet<Customer>()    .Where(c => c.CustomerID == __args        .ElementAt(0))",
                    CoreStrings.QueryUnableToTranslateMethod("System.Linq.Enumerable", nameof(Enumerable.ElementAt))),
                (await Assert.ThrowsAsync<InvalidOperationException>(
                    () => Enumerate(query(context, new[] { "ANATR" })))).Message.Replace("\r", "").Replace("\n", ""));
        }
    }
}
