// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Cosmos.Query
{
    public partial class SimpleQueryCosmosTest
    {
        [ConditionalTheory(Skip = "Issue #12086")]
        public override Task Union(bool isAsync)
            => base.Union(isAsync);

        [ConditionalTheory(Skip = "Issue #12086")]
        public override Task Concat(bool isAsync)
            => base.Concat(isAsync);

        [ConditionalTheory(Skip = "Issue #12086")]        
        public override Task Intersect(bool isAsync)
            => base.Intersect(isAsync);

        [ConditionalTheory(Skip = "Issue #12086")]        
        public override Task Except(bool isAsync)
            => base.Except(isAsync);

        [ConditionalTheory(Skip = "Issue #12086")]        
        public override Task Union_OrderBy_Skip_Take(bool isAsync)
            => base.Union_OrderBy_Skip_Take(isAsync);

        [ConditionalTheory(Skip = "Issue #12086")]        
        public override Task Union_Where(bool isAsync)
            => base.Union_Where(isAsync);

        [ConditionalTheory(Skip = "Issue #12086")]        
        public override Task Union_Skip_Take_OrderBy_ThenBy_Where(bool isAsync)
            => base.Union_Skip_Take_OrderBy_ThenBy_Where(isAsync);

        [ConditionalTheory(Skip = "Issue #12086")]        
        public override Task Union_Union(bool isAsync)
            => base.Union_Union(isAsync);

        [ConditionalTheory(Skip = "Issue #12086")]        
        public override Task Union_Intersect(bool isAsync)
            => base.Union_Intersect(isAsync);

        [ConditionalTheory(Skip = "Issue #12086")]        
        public override Task Union_Take_Union_Take(bool isAsync)
            => base.Union_Take_Union_Take(isAsync);

        [ConditionalTheory(Skip = "Issue #12086")]        
        public override Task Select_Union(bool isAsync)
            => base.Select_Union(isAsync);

        [ConditionalTheory(Skip = "Issue #12086")]        
        public override Task Union_Select(bool isAsync)
            => base.Union_Select(isAsync);

        [ConditionalTheory(Skip = "Issue #12086")]        
        public override Task Select_Union_unrelated(bool isAsync)
            => base.Select_Union_unrelated(isAsync);

        [ConditionalTheory(Skip = "Issue #12086")]        
        public override Task Select_Union_different_fields_in_anonymous_with_subquery(bool isAsync)
            => base.Select_Union_different_fields_in_anonymous_with_subquery(isAsync);
    }
}
