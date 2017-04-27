// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
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
        public void ConfigureDesignTimeServices_works()
        {
            var services = new ServiceCollection();
            var startup = CreateStartupInvoker(MockAssembly.Create(typeof(StartupDevelopment)));

            startup.ConfigureDesignTimeServices(services);

            var service = services.BuildServiceProvider().GetRequiredService<TestService>();
            Assert.Equal("Development", service.Value);
        }

        private class StartupDevelopment : IStartup, IDesignTimeServices
        {
            public IServiceProvider ConfigureServices(IServiceCollection services)
                => services.AddSingleton(new TestService("Development")).BuildServiceProvider();

            public void ConfigureDesignTimeServices(IServiceCollection services)
                => services.AddSingleton(new TestService("Development"));

            public void Configure(IApplicationBuilder app)
                => throw new NotImplementedException();
        }

        [Fact]
        public void ConfigureDesignTimeServices_is_noop_when_not_found()
        {
            var startup = CreateStartupInvoker(MockAssembly.Create());

            startup.ConfigureDesignTimeServices(new ServiceCollection());
        }

        [Fact]
        public void ConfigureServices_works()
        {
            var startup = CreateStartupInvoker(MockAssembly.Create(typeof(StartupDevelopment)));

            var services = startup.ConfigureServices();

            var service = services.GetRequiredService<TestService>();
            Assert.Equal("Development", service.Value);
        }

        [Fact]
        public void ConfigureServices_is_noop_when_not_found()
        {
            var startup = CreateStartupInvoker(MockAssembly.Create());

            var services = startup.ConfigureServices();

            Assert.NotNull(services);
        }

        private StartupInvoker CreateStartupInvoker(Assembly assembly)
            => new StartupInvoker(new TestOperationReporter(), assembly);

        [Fact]
        public void ConfigureDesignTimeServices_works_on_IDesignTimeServices_implementations()
        {
            var services = new ServiceCollection();
            var startup = CreateStartupInvoker(MockAssembly.Create(typeof(DesignTimeServices)));

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
                assembly);

            var services = startup.ConfigureServices();
            var service = services.GetRequiredService<TestService>();

            Assert.Equal("Injected", service.Value);
        }

        private class StartupInjected : IStartup
        {
            public StartupInjected(IHostingEnvironment env)
            {
                Assert.Equal(Directory.GetCurrentDirectory(), env.ContentRootPath);
                Assert.Equal(
                    Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development",
                    env.EnvironmentName);
                Assert.Equal("MockAssembly", env.ApplicationName);
            }

            public void Configure(IApplicationBuilder app)
                => throw new NotImplementedException();

            public IServiceProvider ConfigureServices(IServiceCollection services)
                => services.AddSingleton(new TestService("Injected")).BuildServiceProvider();
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
                MockAssembly.Create(typeof(BadStartup)));

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
