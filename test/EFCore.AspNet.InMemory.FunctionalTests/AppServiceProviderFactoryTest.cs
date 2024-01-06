// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Design.Internal;
using Microsoft.EntityFrameworkCore.Internal;

namespace Microsoft.EntityFrameworkCore;

public class AppServiceProviderFactoryTest
{
    [ConditionalFact]
    public void Create_services_from_template_method()
    {
        TestCreateServices(typeof(ProgramWithBuildWebHost));
        TestCreateServices(typeof(ProgramWithCreateWebHostBuilder));
        TestCreateServices(typeof(ProgramWithCreateHostBuilder));
    }

    private static void TestCreateServices(Type programType)
    {
        var factory = new TestAppServiceProviderFactory(
            MockAssembly.Create(programType));

        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", null);
        Environment.SetEnvironmentVariable("DOTNET_ENVIRONMENT", null);
        var services = factory.Create(["arg1"]);

        Assert.NotNull(services.GetRequiredService<TestService>());
    }

    private class ProgramWithBuildWebHost
    {
        public static TestWebHost BuildWebHost(string[] args)
        {
            ValidateEnvironmentAndArgs(args);

            return new TestWebHost(BuildTestServiceProvider());
        }
    }

    private class ProgramWithCreateWebHostBuilder
    {
        public static TestWebHostBuilder CreateWebHostBuilder(string[] args)
        {
            ValidateEnvironmentAndArgs(args);

            return new TestWebHostBuilder(BuildTestServiceProvider());
        }
    }

    private class ProgramWithCreateHostBuilder
    {
        public static TestWebHostBuilder CreateHostBuilder(string[] args)
        {
            ValidateEnvironmentAndArgs(args);

            return new TestWebHostBuilder(BuildTestServiceProvider());
        }
    }

    [ConditionalFact]
    public void Create_with_no_builder_method()
    {
        var factory = new TestAppServiceProviderFactory(
            MockAssembly.Create(
                [typeof(ProgramWithNoHostBuilder)],
                new MockMethodInfo(typeof(ProgramWithNoHostBuilder), InjectHostIntoDiagnostics)));

        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", null);
        Environment.SetEnvironmentVariable("DOTNET_ENVIRONMENT", null);
        var services = factory.Create(["arg1"]);

        Assert.NotNull(services.GetRequiredService<TestService>());
    }

    private static void InjectHostIntoDiagnostics(object[] args)
    {
        Assert.Equal("Development", Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"));
        Assert.Equal("Development", Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT"));
        Assert.Single(args);
        Assert.Equal((string[])args[0], new[] { "arg1", "--applicationName", "MockAssembly" });

        using var diagnosticListener = new DiagnosticListener("Microsoft.Extensions.Hosting");

        diagnosticListener.Write(
            "HostBuilt",
            new TestWebHost(BuildTestServiceProvider()));
    }

    private class ProgramWithNoHostBuilder;

    private static void ValidateEnvironmentAndArgs(string[] args)
    {
        Assert.Equal("Development", Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"));
        Assert.Equal("Development", Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT"));
        Assert.Equal(args, new[] { "arg1" });
    }

    private static ServiceProvider BuildTestServiceProvider()
        => new ServiceCollection()
            .AddScoped<TestService>()
            .BuildServiceProvider(validateScopes: true);

    private class TestService;

    [ConditionalFact]
    public void Create_works_when_no_BuildWebHost()
    {
        var factory = new TestAppServiceProviderFactory(
            MockAssembly.Create(typeof(ProgramWithoutBuildWebHost)));

        var services = factory.Create([]);

        Assert.NotNull(services);
    }

    private class ProgramWithoutBuildWebHost;

    [ConditionalFact]
    public void Create_works_when_BuildWebHost_throws()
    {
        var reporter = new TestOperationReporter();
        var factory = new TestAppServiceProviderFactory(
            MockAssembly.Create(typeof(ProgramWithThrowingBuildWebHost)),
            reporter);

        var services = factory.Create([]);

        Assert.NotNull(services);
        Assert.Contains(
            "warn: " + DesignStrings.InvokeCreateHostBuilderFailed("This is a test."),
            reporter.Messages);
    }

    private static class ProgramWithThrowingBuildWebHost
    {
        public static TestWebHost BuildWebHost(string[] args)
            => throw new Exception("This is a test.");
    }
}

public class TestAppServiceProviderFactory(Assembly startupAssembly, IOperationReporter reporter = null) : AppServiceProviderFactory(startupAssembly, reporter ?? new TestOperationReporter());

public class TestWebHost(IServiceProvider services)
{
    public IServiceProvider Services { get; } = services;
}

public class TestWebHostBuilder(IServiceProvider services)
{
    public IServiceProvider Services { get; } = services;

    public TestWebHost Build()
        => new(Services);
}

public class TestOperationReporter : IOperationReporter
{
    private readonly List<string> _messages = [];

    public IReadOnlyList<string> Messages
        => _messages;

    public void Clear()
        => _messages.Clear();

    public void WriteInformation(string message)
        => _messages.Add("info: " + message);

    public void WriteVerbose(string message)
        => _messages.Add("verbose: " + message);

    public void WriteWarning(string message)
        => _messages.Add("warn: " + message);

    public void WriteError(string message)
        => _messages.Add("error: " + message);
}
