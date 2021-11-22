// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Metadata.Internal;

public class ConfigurationSourceExtensionsTest
{
    [ConditionalFact]
    public void Overrides_returns_expected_value()
    {
        Assert.True(ConfigurationSource.Explicit.Overrides(ConfigurationSource.Explicit));
        Assert.True(ConfigurationSource.Explicit.Overrides(ConfigurationSource.DataAnnotation));
        Assert.True(ConfigurationSource.Explicit.Overrides(ConfigurationSource.Convention));
        Assert.False(ConfigurationSource.DataAnnotation.Overrides(ConfigurationSource.Explicit));
        Assert.True(ConfigurationSource.DataAnnotation.Overrides(ConfigurationSource.DataAnnotation));
        Assert.True(ConfigurationSource.DataAnnotation.Overrides(ConfigurationSource.Convention));
        Assert.False(ConfigurationSource.Convention.Overrides(ConfigurationSource.Explicit));
        Assert.False(ConfigurationSource.Convention.Overrides(ConfigurationSource.DataAnnotation));
        Assert.True(ConfigurationSource.Convention.Overrides(ConfigurationSource.Convention));
    }

    [ConditionalFact]
    public void Max_returns_expected_value()
    {
        Assert.Equal(ConfigurationSource.Explicit, ConfigurationSource.Explicit.Max(ConfigurationSource.Convention));
        Assert.Equal(ConfigurationSource.Explicit, ConfigurationSource.Convention.Max(ConfigurationSource.Explicit));
        Assert.Equal(ConfigurationSource.Convention, ConfigurationSource.Convention.Max(null));
    }
}
