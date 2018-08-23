// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.TestUtilities;

namespace Microsoft.EntityFrameworkCore.TestModels.SpatialModel
{
    public class SpatialContext : PoolableDbContext
    {
        public SpatialContext(DbContextOptions options)
            : base(options)
        {
        }

        public static void Seed(SpatialContext context)
        {
            context.AddRange(SpatialData.CreatePointEntities());
            context.AddRange(SpatialData.CreateLineStringEntities());
            context.AddRange(SpatialData.CreatePolygonEntities());
            context.AddRange(SpatialData.CreateMultiLineStringEntities());
            context.SaveChanges();
        }
    }
}
