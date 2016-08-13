// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Specification.Tests.TestModels.Northwind;
using Xunit;

// ReSharper disable AccessToDisposedClosure
namespace Microsoft.EntityFrameworkCore.Specification.Tests
{
    public abstract class WarningsTestBase<TFixture> : IClassFixture<TFixture>
        where TFixture : NorthwindQueryRelationalFixture, new()
    {
        [Fact]
        public virtual void Throws_when_warning_as_error()
        {
            using (var context = CreateContext())
            {
                Assert.Equal(CoreStrings.WarningAsErrorTemplate(
                    $"{nameof(RelationalEventId)}.{nameof(RelationalEventId.QueryClientEvaluationWarning)}",
                    RelationalStrings.ClientEvalWarning("[c].IsLondon")),
                    Assert.Throws<InvalidOperationException>(
                        () => context.Customers.Where(c => c.IsLondon).ToList()).Message);
            }
        }

        [Fact]
        public virtual void Does_not_throw_for_top_level_single()
        {
            using (var context = CreateContext())
            {
                var query = context.Orders.Single(x => x.OrderID == 10248);

                Assert.NotNull(query);
            }
        }

        protected NorthwindContext CreateContext() => Fixture.CreateContext();

        protected WarningsTestBase(TFixture fixture)
        {
            Fixture = fixture;
        }

        protected TFixture Fixture { get; }
    }
}