// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Microsoft.Data.Entity.Design.Internal
{
    public class StartupInvokerTest
    {
        [Fact]
        public void ConfigureDesignTimeServices_uses_Development_environment_when_unspecified()
        {
            var services = new ServiceCollection();
            var startup = new StartupInvoker(
                typeof(StartupInvokerTest).Assembly.FullName,
                environment: null,
                dnxServices: null);

            startup.ConfigureDesignTimeServices(services);

            var service = services.BuildServiceProvider().GetRequiredService<TestService>();
            Assert.Equal("Development", service.Value);
        }

        [Fact]
        public void ConfigureDesignTimeServices_invokes_static_methods()
        {
            var services = new ServiceCollection();
            var startup = new StartupInvoker(
                typeof(StartupInvokerTest).Assembly.FullName,
                "Static",
                dnxServices: null);

            startup.ConfigureDesignTimeServices(services);

            var service = services.BuildServiceProvider().GetRequiredService<TestService>();
            Assert.Equal("Static", service.Value);
        }
    }

    public class StartupDevelopment
    {
        public void ConfigureDesignTimeServices(IServiceCollection services)
            => services.AddInstance(new TestService("Development"));
    }

    public class StartupStatic
    {
        public static void ConfigureDesignTimeServices(IServiceCollection services)
            => services.AddInstance(new TestService("Static"));
    }

    public class TestService
    {
        public TestService(string value)
        {
            Value = value;
        }

        public string Value { get; }
    }
}
