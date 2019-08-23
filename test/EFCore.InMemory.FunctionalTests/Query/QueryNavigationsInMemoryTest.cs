// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class QueryNavigationsInMemoryTest : QueryNavigationsTestBase<NorthwindQueryInMemoryFixture<NoopModelCustomizer>>
    {
        public QueryNavigationsInMemoryTest(
            NorthwindQueryInMemoryFixture<NoopModelCustomizer> fixture, ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
            //TestLoggerFactory.TestOutputHelper = testOutputHelper;
        }

        #region SingleResultProjection

        public override Task Collection_select_nav_prop_first_or_default(bool isAsync)
        {
            return Task.CompletedTask;
        }

        public override Task Collection_select_nav_prop_first_or_default_then_nav_prop(bool isAsync)
        {
            return Task.CompletedTask;
        }

        public override Task Project_single_entity_value_subquery_works(bool isAsync)
        {
            return Task.CompletedTask;
        }

        public override Task Select_collection_FirstOrDefault_project_anonymous_type(bool isAsync)
        {
            return Task.CompletedTask;
        }

        public override Task Select_collection_FirstOrDefault_project_entity(bool isAsync)
        {
            return Task.CompletedTask;
        }

        public override Task Skip_Select_Navigation(bool isAsync)
        {
            return Task.CompletedTask;
        }

        public override Task Take_Select_Navigation(bool isAsync)
        {
            return Task.CompletedTask;
        }

        #endregion

        [ConditionalTheory(Skip = "Issue#17386")]
        public override Task Where_subquery_on_navigation_client_eval(bool isAsync)
        {
            return Task.CompletedTask;
        }
    }
}
