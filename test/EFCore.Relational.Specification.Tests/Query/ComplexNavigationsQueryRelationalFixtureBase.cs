// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.TestModels.ComplexNavigationsModel;
using Microsoft.EntityFrameworkCore.TestUtilities;

namespace Microsoft.EntityFrameworkCore.Query
{
    public abstract class ComplexNavigationsQueryRelationalFixtureBase : ComplexNavigationsQueryFixtureBase
    {
        public TestSqlLoggerFactory TestSqlLoggerFactory => (TestSqlLoggerFactory)ListLoggerFactory;

        protected override QueryAsserter<ComplexNavigationsContext> CreateQueryAsserter(
            Dictionary<Type, object> entitySorters,
            Dictionary<Type, object> entityAsserters)
            => new RelationalQueryAsserter<ComplexNavigationsContext>(
                CreateContext,
                new ComplexNavigationsDefaultData(),
                entitySorters,
                entityAsserters,
                CanExecuteQueryString,
                CreateExpectedQueryRewritingVisitor());
    }
}
