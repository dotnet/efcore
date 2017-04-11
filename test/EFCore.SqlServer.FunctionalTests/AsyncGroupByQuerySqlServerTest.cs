// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Specification.Tests;
using Microsoft.EntityFrameworkCore.Specification.Tests.TestModels.Northwind;
using Microsoft.EntityFrameworkCore.Specification.Tests.TestUtilities.Xunit;
using Xunit;
using Xunit.Abstractions;

// ReSharper disable AccessToDisposedClosure

#pragma warning disable 1998
namespace Microsoft.EntityFrameworkCore.SqlServer.FunctionalTests
{
    public class AsyncGroupByQuerySqlServerTest : AsyncGroupByQueryTestBase<NorthwindQuerySqlServerFixture>
    {
        public AsyncGroupByQuerySqlServerTest(NorthwindQuerySqlServerFixture fixture, ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
        }

        public override Task OrderBy_GroupBy_SelectMany()
        {
            return base.OrderBy_GroupBy_SelectMany();
        }
    }
}
