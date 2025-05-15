// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.TestUtilities;

public class NoopModelCustomizer : ITestModelCustomizer
{
    public virtual void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
    }

    public virtual void Customize(ModelBuilder modelBuilder, DbContext context)
    {
    }
}
