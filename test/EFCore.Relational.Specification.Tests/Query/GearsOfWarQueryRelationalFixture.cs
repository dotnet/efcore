// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.TestModels.GearsOfWarModel;
using Microsoft.EntityFrameworkCore.TestUtilities;

namespace Microsoft.EntityFrameworkCore.Query
{
    public abstract class GearsOfWarQueryRelationalFixture : GearsOfWarQueryFixtureBase
    {
        public new RelationalTestStore TestStore => (RelationalTestStore)base.TestStore;

        public TestSqlLoggerFactory TestSqlLoggerFactory => (TestSqlLoggerFactory)ListLoggerFactory;

        protected override bool ShouldLogCategory(string logCategory)
            => logCategory == DbLoggerCategory.Query.Name;

        protected override QueryAsserter<GearsOfWarContext> CreateQueryAsserter(
            Dictionary<Type, object> entitySorters,
            Dictionary<Type, object> entityAsserters)
            => new RelationalQueryAsserter<GearsOfWarContext>(
                CreateContext,
                new GearsOfWarData(),
                entitySorters,
                entityAsserters,
                CanExecuteQueryString);
    }
}
