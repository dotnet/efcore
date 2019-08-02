// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Query
{
    public partial class SimpleQueryCosmosTest
    {
        // Set operations aren't supported on Cosmos
        public override Task Concat(bool isAsync) => Task.CompletedTask;
        public override Task Concat_nested(bool isAsync) => Task.CompletedTask;
        public override Task Concat_non_entity(bool isAsync) => Task.CompletedTask;
        public override Task Except(bool isAsync) => Task.CompletedTask;
        public override Task Except_simple_followed_by_projecting_constant(bool isAsync) => Task.CompletedTask;
        public override Task Except_nested(bool isAsync) => Task.CompletedTask;
        public override Task Except_non_entity(bool isAsync) => Task.CompletedTask;
        public override Task Intersect(bool isAsync) => Task.CompletedTask;
        public override Task Intersect_nested(bool isAsync) => Task.CompletedTask;
        public override Task Intersect_non_entity(bool isAsync) => Task.CompletedTask;
        public override Task Union(bool isAsync) => Task.CompletedTask;
        public override Task Union_nested(bool isAsync) => Task.CompletedTask;
        public override void Union_non_entity(bool isAsync) {}
        public override Task Union_OrderBy_Skip_Take(bool isAsync) => Task.CompletedTask;
        public override Task Union_Where(bool isAsync) => Task.CompletedTask;
        public override Task Union_Skip_Take_OrderBy_ThenBy_Where(bool isAsync) => Task.CompletedTask;
        public override Task Union_Union(bool isAsync) => Task.CompletedTask;
        public override Task Union_Intersect(bool isAsync) => Task.CompletedTask;
        public override Task Union_Take_Union_Take(bool isAsync) => Task.CompletedTask;
        public override Task Select_Union(bool isAsync) => Task.CompletedTask;
        public override Task Union_Select(bool isAsync) => Task.CompletedTask;
        public override Task Union_with_anonymous_type_projection(bool isAsync) => Task.CompletedTask;
        public override Task Select_Union_unrelated(bool isAsync) => Task.CompletedTask;
        public override Task Select_Union_different_fields_in_anonymous_with_subquery(bool isAsync) => Task.CompletedTask;
        public override Task Union_Include(bool isAsync) => Task.CompletedTask;
        public override Task Include_Union(bool isAsync) => Task.CompletedTask;
        public override Task Select_Except_reference_projection(bool isAsync) => Task.CompletedTask;
        public override void Include_Union_only_on_one_side_throws() {}
        public override void Include_Union_different_includes_throws() {}
        public override Task SubSelect_Union(bool isAsync) => Task.CompletedTask;
        public override Task Client_eval_Union_FirstOrDefault(bool isAsync) => Task.CompletedTask;
        public override Task GroupBy_Select_Union(bool isAsync) => Task.CompletedTask;
        public override Task Union_over_different_projection_types(bool isAsync, string leftType, string rightType) => Task.CompletedTask;
    }
}
