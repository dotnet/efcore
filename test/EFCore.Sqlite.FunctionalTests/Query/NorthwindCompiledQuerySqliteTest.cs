// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.TestModels.Northwind;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class NorthwindCompiledQuerySqliteTest : NorthwindCompiledQueryTestBase<NorthwindQuerySqliteFixture<NoopModelCustomizer>>
    {
        public NorthwindCompiledQuerySqliteTest(NorthwindQuerySqliteFixture<NoopModelCustomizer> fixture)
            : base(fixture)
        {
        }

        public override void MakeBinary_does_not_throw_for_unsupported_operator()
            => Assert.Equal(
                CoreStrings.TranslationFailed("DbSet<Customer>()    .Where(c => c.CustomerID == (string)(__parameters[0]))"),
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
                    CoreStrings.TranslationFailed("DbSet<Customer>()    .Where(c => c.CustomerID == __args[0])"),
                    Assert.Throws<InvalidOperationException>(
                        () => query(context, new[] { "ALFKI" }).First().CustomerID).Message.Replace("\r", "").Replace("\n", ""));
            }

            using (var context = CreateContext())
            {
                Assert.Equal(
                    CoreStrings.TranslationFailed("DbSet<Customer>()    .Where(c => c.CustomerID == __args[0])"),
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
                    CoreStrings.TranslationFailed("DbSet<Customer>()    .Where(c => c.CustomerID == __args[0])"),
                    (await Assert.ThrowsAsync<InvalidOperationException>(
                        () => Enumerate(query(context, new[] { "ALFKI" })))).Message.Replace("\r", "").Replace("\n", ""));
            }

            using (var context = CreateContext())
            {
                Assert.Equal(
                    CoreStrings.TranslationFailed("DbSet<Customer>()    .Where(c => c.CustomerID == __args[0])"),
                    (await Assert.ThrowsAsync<InvalidOperationException>(
                        () => Enumerate(query(context, new[] { "ANATR" })))).Message.Replace("\r", "").Replace("\n", ""));
            }
        }
    }
}
