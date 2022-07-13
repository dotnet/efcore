// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Design;

public abstract class DesignTimeProviderServicesTest
{
    protected abstract Assembly GetRuntimeAssembly();
    protected abstract Type GetDesignTimeServicesType();

    [ConditionalFact]
    public void Ensure_assembly_identity_matches()
    {
        var runtimeAssembly = GetRuntimeAssembly();
        var dtAttribute = runtimeAssembly.GetCustomAttribute<DesignTimeProviderServicesAttribute>();
        var dtType = GetDesignTimeServicesType();
        Assert.NotNull(dtType);

        Assert.NotNull(dtAttribute);
        Assert.Equal(dtType.FullName, dtAttribute.TypeName);
    }
}
