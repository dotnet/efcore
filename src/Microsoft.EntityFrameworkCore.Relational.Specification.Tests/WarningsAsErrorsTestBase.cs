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
    public abstract class WarningsAsErrorsTestBase<TFixture> : IClassFixture<TFixture>
        where TFixture : NorthwindQueryRelationalFixture, new()
    {
        [Fact]
        public virtual void Throws_when_warning_as_error()
        {
            using (var context = CreateContext())
            {
                Assert.Equal(CoreStrings.WarningAsError(
                    $"{nameof(RelationalLoggingEventId)}.{nameof(RelationalLoggingEventId.QueryClientEvaluationWarning)}",
                    RelationalStrings.ClientEvalWarning("[c].IsLondon")),
                    Assert.Throws<InvalidOperationException>(
                        () => context.Customers.Where(c => c.IsLondon).ToList()).Message);
            }
        }

        protected NorthwindContext CreateContext() => Fixture.CreateContext();

        protected WarningsAsErrorsTestBase(TFixture fixture)
        {
            Fixture = fixture;
        }

        protected TFixture Fixture { get; }
    }
}
