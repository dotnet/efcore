// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Infrastructure;

public interface ITestModelCustomizer : IModelCustomizer
{
    void ConfigureConventions(ModelConfigurationBuilder configurationBuilder);
}
