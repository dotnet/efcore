// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.TestModels.InheritanceRelationships;

namespace Microsoft.EntityFrameworkCore.Query
{
    public abstract class InheritanceRelationshipsQueryFixtureBase : SharedStoreFixtureBase<InheritanceRelationshipsContext>
    {
        protected override string StoreName { get; } = "InheritanceRelationships";

        protected override void Seed(InheritanceRelationshipsContext context)
            => InheritanceRelationshipsContext.Seed(context);

        public override InheritanceRelationshipsContext CreateContext()
        {
            var context = base.CreateContext();
            context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
            return context;
        }
    }
}
