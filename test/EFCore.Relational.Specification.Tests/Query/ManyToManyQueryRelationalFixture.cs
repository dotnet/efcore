// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.TestModels.ManyToManyModel;
using Microsoft.EntityFrameworkCore.TestUtilities;

namespace Microsoft.EntityFrameworkCore.Query
{
    public abstract class ManyToManyQueryRelationalFixture : ManyToManyQueryFixtureBase
    {
        public TestSqlLoggerFactory TestSqlLoggerFactory => (TestSqlLoggerFactory)ListLoggerFactory;

        protected override QueryAsserter<ManyToManyContext> CreateQueryAsserter(
            Dictionary<Type, object> entitySorters,
            Dictionary<Type, object> entityAsserters)
            => new RelationalQueryAsserter<ManyToManyContext>(
                CreateContext,
                new ManyToManyData(),
                entitySorters,
                entityAsserters,
                CanExecuteQueryString);
    }
}
