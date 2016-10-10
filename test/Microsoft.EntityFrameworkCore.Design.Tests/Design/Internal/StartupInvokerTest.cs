// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore.Design.Internal;
using Microsoft.EntityFrameworkCore.Design.TestUtilities;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Tests.TestUtilities;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Design.Tests.Design.Internal
{
    public class StartupInvokerTest
    {
        [Fact]
        public void ConfigureDesignTimeServices_uses_Development_environment_when_unspecified()
        {
            var services = new ServiceCollection();
            var startup = CreateStartupInvoker(
                MockAssembly.Create(typeof(StartupDevelopment)),
                environment: null);

            startup.ConfigureDesignTimeServices(services);

            var service = services.BuildServiceProvider().GetRequiredService<TestService>();
            Assert.Equal("Development", service.Value);
        }

        private class StartupDevelopment
        {
            public void ConfigureDevelopmentServices(IServiceCollection services)
                => services.AddSingleton(new TestService("Development"));

            public void ConfigureDesignTimeServices(IServiceCollection services)
                => services.AddSingleton(new TestService("Development"));
        }

        [Fact]
        public void ConfigureDesignTimeServices_invokes_static_methods()
        {
            var services = new ServiceCollection();
            var startup = CreateStartupInvoker(
                MockAssembly.Create(typeof(StartupStatic)),
                "Static");

            startup.ConfigureDesignTimeServices(services);

            var service = services.BuildServiceProvider().GetRequiredService<TestService>();
            Assert.Equal("Static", service.Value);
        }

        private class StartupStatic
        {
            public static void ConfigureServices(IServiceCollection services)
                => services.AddSingleton(new TestService("Static"));

            public static void ConfigureDesignTimeServices(IServiceCollection services)
                => services.AddSingleton(new TestService("Static"));
        }

        [Fact]
        public void ConfigureDesignTimeServices_is_noop_when_not_found()
        {
            var startup = CreateStartupInvoker(
                MockAssembly.Create(),
                "Unknown");

            startup.ConfigureDesignTimeServices(new ServiceCollection());
        }

        [Fact]
        public void ConfigureServices_uses_Development_environment_when_unspecified()
        {
            var startup = CreateStartupInvoker(
                MockAssembly.Create(typeof(StartupDevelopment)),
                environment: null);

            var services = startup.ConfigureServices();

            var service = services.GetRequiredService<TestService>();
            Assert.Equal("Development", service.Value);
        }

        [Fact]
        public void ConfigureServices_is_noop_when_not_found()
        {
            var startup = CreateStartupInvoker(
                MockAssembly.Create(),
                "Unknown");

            var services = startup.ConfigureServices();

            Assert.NotNull(services);
        }

        [Fact]
        public void ConfigureServices_invokes_static_methods()
        {
            var startup = CreateStartupInvoker(
                MockAssembly.Create(typeof(StartupStatic)),
                "Static");

            var services = startup.ConfigureServices();

            var service = services.GetRequiredService<TestService>();
            Assert.Equal("Static", service.Value);
        }

        [Fact]
        public void ConfigureServices_invokes_method_with_alternative_signature()
        {
            var startup = CreateStartupInvoker(
                MockAssembly.Create(typeof(StartupAlternative)),
                "Alternative");

            var services = startup.ConfigureServices();

            var service = services.GetRequiredService<TestService>();
            Assert.Equal("Alternative", service.Value);
        }

        private class StartupAlternative
        {
            public IServiceProvider ConfigureServices()
                => new ServiceCollection()
                    .AddSingleton(new TestService("Alternative"))
                    .BuildServiceProvider();
        }

        private StartupInvoker CreateStartupInvoker(Assembly assembly, string environment)
            => new StartupInvoker(
                new TestOperationReporter(),
                assembly,
                environment,
                "Irrelevant");

        [Fact]
        public void ConfigureDesignTimeServices_works_on_other_types()
        {
            var services = new ServiceCollection();
            var startup = CreateStartupInvoker(
                MockAssembly.Create(typeof(NotStartup)),
                environment: null);

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
        public void ConfigureDesignTimeServices_works_on_IDesignTimeServices_implementations()
        {
            var services = new ServiceCollection();
            var startup = CreateStartupInvoker(
                MockAssembly.Create(typeof(DesignTimeServices)),
                "Irrelevant");

            startup.ConfigureDesignTimeServices(services);

            var service = services.BuildServiceProvider().GetRequiredService<TestService>();
            Assert.Equal("DesignTimeServices", service.Value);
        }

        private class DesignTimeServices : IDesignTimeServices
        {
            public void ConfigureDesignTimeServices(IServiceCollection services)
                => services.AddSingleton(new TestService("DesignTimeServices"));
        }

        [Fact]
        public void ConfigureServices_injects_services()
        {
            var assembly = MockAssembly.Create(typeof(StartupInjected));
            var startup = new StartupInvoker(
                new TestOperationReporter(),
                assembly,
                "Injected",
                @"C:\The\Right\Path");

            var services = startup.ConfigureServices();
            var service = services.GetRequiredService<TestService>();

            Assert.Equal("Injected", service.Value);
        }

        private class StartupInjected
        {
            public StartupInjected(IHostingEnvironment env)
            {
                Assert.Equal(@"C:\The\Right\Path", env.ContentRootPath);
                Assert.Equal("Injected", env.EnvironmentName);
                Assert.Equal("MockAssembly", env.ApplicationName);
            }

            private void ConfigureInjectedServices(IServiceCollection services)
                => services.AddSingleton(new TestService("Injected"));
        }

        [Fact]
        public void ConfigureServices_works_on_IStartup_implementations()
        {
            var startup = CreateStartupInvoker(
                MockAssembly.Create(typeof(MyStartup)),
                environment: null);

            var services = startup.ConfigureServices();
            var service = services.GetRequiredService<TestService>();

            Assert.Equal("MyStartup", service.Value);
        }

        private class MyStartup : IStartup
        {
            public void Configure(IApplicationBuilder app)
            {
                throw new NotImplementedException();
            }

            public IServiceProvider ConfigureServices(IServiceCollection services)
                => services.AddSingleton(new TestService("MyStartup")).BuildServiceProvider();
        }

        public class TestService
        {
            public TestService(string value)
            {
                Value = value;
            }

            public string Value { get; }
        }

        [Fact]
        public void Invoke_warns_on_error()
        {
            var reporter = new TestOperationReporter();

            var startup = new StartupInvoker(
                reporter,
                MockAssembly.Create(typeof(BadStartup)),
                /*environment:*/ null,
                "Irrelevant");

            var services = startup.ConfigureServices();

            Assert.NotNull(services);
            Assert.Equal(
                "warn: " + DesignStrings.InvokeStartupMethodFailed(
                    "ConfigureServices",
                    nameof(BadStartup),
                    "Something went wrong."),
                reporter.Messages[0]);
        }

        private class BadStartup : IStartup
        {
            public BadStartup()
            {
                throw new Exception("Something went wrong.");
            }

            public void Configure(IApplicationBuilder app)
            {
            }

            public IServiceProvider ConfigureServices(IServiceCollection services)
                => services.BuildServiceProvider();
        }
    }
}
