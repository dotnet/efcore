// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Xunit;

namespace Microsoft.Data.Entity.Metadata.Internal
{
    public class ConfigurationSourceExtensionsTest
    {
        [Fact]
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

        [Fact]
        public void Max_returns_expected_value()
        {
            Assert.Equal(ConfigurationSource.Explicit, ConfigurationSource.Explicit.Max(ConfigurationSource.Convention));
            Assert.Equal(ConfigurationSource.Explicit, ConfigurationSource.Convention.Max(ConfigurationSource.Explicit));
            Assert.Equal(ConfigurationSource.Convention, ConfigurationSource.Convention.Max(null));
        }

        [Fact]
        public void CanSet_returns_expected_value()
        {
            Assert.True(ConfigurationSource.Explicit.CanSet(ConfigurationSource.Convention, true));
            Assert.False(ConfigurationSource.Convention.CanSet(ConfigurationSource.Explicit, true));
            Assert.False(ConfigurationSource.Convention.CanSet(null, true));
            Assert.True(ConfigurationSource.Explicit.CanSet(ConfigurationSource.Convention, false));
            Assert.True(ConfigurationSource.Convention.CanSet(ConfigurationSource.Explicit, false));
            Assert.True(ConfigurationSource.Convention.CanSet(null, false));
        }
    }
}
