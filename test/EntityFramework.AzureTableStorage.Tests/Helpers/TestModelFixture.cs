// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq.Expressions;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Query;
using Moq;
using Remotion.Linq.Clauses;

namespace Microsoft.Data.Entity.AzureTableStorage.Tests.Helpers
{
    public class TestModelFixture
    {
        public Model CreateTestModel(string name)
        {
            var model = new Model();
            model.StorageName = name;
            return model;
        }

        public MainFromClause CreateWithEntityQueryable<T>()
        {
            var queryable = new EntityQueryable<T>(new EntityQueryExecutor(Mock.Of<DbContext>()));
            return new MainFromClause("MainFrom", typeof(T), Expression.Constant(queryable));
        }
    }
}
