// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Infrastructure
{
    public static class ModelSourceHelpers
    {
        public static void OnModelCreating([NotNull] DbContext context, [NotNull] ModelBuilder modelBuilder)
        {
            Check.NotNull(context, "context");
            Check.NotNull(modelBuilder, "modelBuilder");

            context.OnModelCreating(modelBuilder);
        }
    }
}
