// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Specification.Tests;
using Microsoft.EntityFrameworkCore.Specification.Tests.TestModels.Northwind;
using Xunit;

#pragma warning disable 1998
namespace Microsoft.EntityFrameworkCore.Sqlite.FunctionalTests
{
    public class AsyncGroupByQuerySqliteTest : AsyncGroupByQueryTestBase<NorthwindQuerySqliteFixture>
    {
        public AsyncGroupByQuerySqliteTest(NorthwindQuerySqliteFixture fixture)
            : base(fixture)
        {
        }
    }
}
