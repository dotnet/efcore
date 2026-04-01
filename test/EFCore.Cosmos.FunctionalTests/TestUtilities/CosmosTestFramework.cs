// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Reflection;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Microsoft.EntityFrameworkCore.TestUtilities;

/// <summary>
///     Custom xunit test framework that manages assembly-wide fixtures.
///     Registered via <c>[assembly: TestFramework(...)]</c> in TestAssemblyCondition.cs.
/// </summary>
public class CosmosTestFramework(IMessageSink messageSink) : XunitTestFramework(messageSink)
{
    protected override ITestFrameworkExecutor CreateExecutor(AssemblyName assemblyName)
        => new CosmosTestFrameworkExecutor(assemblyName, SourceInformationProvider, DiagnosticMessageSink);
}

internal class CosmosTestFrameworkExecutor(
    AssemblyName assemblyName,
    ISourceInformationProvider sourceInformationProvider,
    IMessageSink diagnosticMessageSink)
    : XunitTestFrameworkExecutor(assemblyName, sourceInformationProvider, diagnosticMessageSink)
{
    protected override async void RunTestCases(
        IEnumerable<IXunitTestCase> testCases,
        IMessageSink executionMessageSink,
        ITestFrameworkExecutionOptions executionOptions)
    {
        using var assemblyRunner = new CosmosTestAssemblyRunner(
            TestAssembly, testCases, DiagnosticMessageSink, executionMessageSink, executionOptions);
        await assemblyRunner.RunAsync();
    }
}

internal class CosmosTestAssemblyRunner(
    ITestAssembly testAssembly,
    IEnumerable<IXunitTestCase> testCases,
    IMessageSink diagnosticMessageSink,
    IMessageSink executionMessageSink,
    ITestFrameworkExecutionOptions executionOptions)
    : XunitTestAssemblyRunner(testAssembly, testCases, diagnosticMessageSink, executionMessageSink, executionOptions)
{
    private CosmosEmulatorFixture? _fixture;

    protected override async Task AfterTestAssemblyStartingAsync()
    {
        await base.AfterTestAssemblyStartingAsync();

        _fixture = new CosmosEmulatorFixture();
        await _fixture.InitializeAsync();
    }

    protected override async Task BeforeTestAssemblyFinishedAsync()
    {
        if (_fixture != null)
        {
            await _fixture.DisposeAsync();
        }

        await base.BeforeTestAssemblyFinishedAsync();
    }
}
