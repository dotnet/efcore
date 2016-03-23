// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Reflection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore.Tests.TestUtilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.PlatformAbstractions;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Design.Internal
{
    public class StartupInvokerTest
    {
        [Fact]
        public void ConfigureDesignTimeServices_uses_Development_environment_when_unspecified()
        {
            var services = new ServiceCollection();
            var startup = CreateStartupInvoker(environment: null);

            startup.ConfigureDesignTimeServices(services);

            var service = services.BuildServiceProvider().GetRequiredService<TestService>();
            Assert.Equal("Development", service.Value);
        }

        [Fact]
        public void ConfigureDesignTimeServices_invokes_static_methods()
        {
            var services = new ServiceCollection();
            var startup = CreateStartupInvoker("Static");

            startup.ConfigureDesignTimeServices(services);

            var service = services.BuildServiceProvider().GetRequiredService<TestService>();
            Assert.Equal("Static", service.Value);
        }

        [Fact]
        public void ConfigureDesignTimeServices_is_noop_when_not_found()
        {
            var startup = CreateStartupInvoker("Unknown");

            startup.ConfigureDesignTimeServices(new ServiceCollection());
        }

        [Fact]
        public void ConfigureServices_uses_Development_environment_when_unspecified()
        {
            var startup = CreateStartupInvoker(environment: null);

            var services = startup.ConfigureServices();

            var service = services.GetRequiredService<TestService>();
            Assert.Equal("Development", service.Value);
        }

        [Fact]
        public void ConfigureServices_is_noop_when_not_found()
        {
            var startup = CreateStartupInvoker("Unknown");

            var services = startup.ConfigureServices();

            Assert.NotNull(services);
        }

        [Fact]
        public void ConfigureServices_invokes_static_methods()
        {
            var startup = CreateStartupInvoker("Static");

            var services = startup.ConfigureServices();

            var service = services.GetRequiredService<TestService>();
            Assert.Equal("Static", service.Value);
        }

        [Fact]
        public void ConfigureServices_invokes_method_with_alternative_signature()
        {
            var startup = CreateStartupInvoker("Alternative");

            var services = startup.ConfigureServices();

            var service = services.GetRequiredService<TestService>();
            Assert.Equal("Alternative", service.Value);
        }

        private StartupInvoker CreateStartupInvoker(string environment)
            => new StartupInvoker(
                typeof(StartupInvokerTest).GetTypeInfo().Assembly,
                environment,
                "Irrelevant");

        [Fact]
        public void ConfigureDesignTimeServices_works_on_other_types()
        {
            var services = new ServiceCollection();
            var startup = CreateStartupInvoker(environment: null);

            startup.ConfigureDesignTimeServices(typeof(NotStartup), services);

            var service = services.BuildServiceProvider().GetRequiredService<TestService>();
            Assert.Equal("NotStartup", service.Value);
        }

        private class NotStartup
        {
            public void ConfigureDesignTimeServices(IServiceCollection services)
                => services.AddSingleton(new TestService("NotStartup"));
        }

        [Fact]
        public void ConfigureServices_injects_services()
        {
            var assembly = MockAssembly.Create(typeof(StartupInjected));
            var startup = new StartupInvoker(assembly, "Injected", @"C:\The\Right\Path");

            var services = startup.ConfigureServices();
            var service = services.GetRequiredService<TestService>();

            Assert.Equal("Injected", service.Value);
        }

        private class StartupInjected
        {
            public StartupInjected(IHostingEnvironment env, IApplicationEnvironment appEnv)
            {
                Assert.Equal(@"C:\The\Right\Path", env.ContentRootPath);
                Assert.Equal("Injected", env.EnvironmentName);
                Assert.Equal(@"C:\The\Right\Path", appEnv.ApplicationBasePath);
            }

            private void ConfigureInjectedServices(IServiceCollection services)
                => services.AddSingleton(new TestService("Injected"));
        }
    }

    public class StartupDevelopment
    {
        public void ConfigureDevelopmentServices(IServiceCollection services)
            => services.AddSingleton(new TestService("Development"));

        public void ConfigureDesignTimeServices(IServiceCollection services)
            => services.AddSingleton(new TestService("Development"));
    }

    public class StartupStatic
    {
        public static void ConfigureServices(IServiceCollection services)
            => services.AddSingleton(new TestService("Static"));

        public static void ConfigureDesignTimeServices(IServiceCollection services)
            => services.AddSingleton(new TestService("Static"));
    }

    public class StartupAlternative
    {
        public IServiceProvider ConfigureServices()
            => new ServiceCollection()
                .AddSingleton(new TestService("Alternative"))
                .BuildServiceProvider();
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
