// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Xunit.Sdk;

namespace Microsoft.EntityFrameworkCore.TestUtilities.Xunit;

/// <summary>
///     Used dynamically from <see cref="ConditionalTheoryAttribute" />.
///     Make sure to update that class if you move this type.
/// </summary>
public class ConditionalTheoryDiscoverer(IMessageSink messageSink) : TheoryDiscoverer(messageSink)
{
    protected override IEnumerable<IXunitTestCase> CreateTestCasesForTheory(
        ITestFrameworkDiscoveryOptions discoveryOptions,
        ITestMethod testMethod,
        IAttributeInfo theoryAttribute)
    {
        yield return new ConditionalTheoryTestCase(
            DiagnosticMessageSink,
            discoveryOptions.MethodDisplayOrDefault(),
            discoveryOptions.MethodDisplayOptionsOrDefault(),
            testMethod);
    }

    protected override IEnumerable<IXunitTestCase> CreateTestCasesForDataRow(
        ITestFrameworkDiscoveryOptions discoveryOptions,
        ITestMethod testMethod,
        IAttributeInfo theoryAttribute,
        object[] dataRow)
    {
        yield return new ConditionalFactTestCase(
            DiagnosticMessageSink,
            discoveryOptions.MethodDisplayOrDefault(),
            discoveryOptions.MethodDisplayOptionsOrDefault(),
            testMethod,
            dataRow);
    }
}
