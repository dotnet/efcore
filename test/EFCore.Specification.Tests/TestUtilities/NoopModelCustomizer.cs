// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Microsoft.EntityFrameworkCore.TestUtilities
{
    public class NoopModelCustomizer : IModelCustomizer
    {
        public void Customize(ModelBuilder modelBuilder, DbContext context)
        {
        }
    }
}
