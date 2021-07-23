// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Microsoft.EntityFrameworkCore.TestUtilities.Xunit
{
    /// <summary>
    ///     Used dynamically from <see cref="ConditionalFactAttribute" />.
    ///     Make sure to update that class if you move this type.
    /// </summary>
    public class ConditionalFactDiscoverer : FactDiscoverer
    {
        public ConditionalFactDiscoverer(IMessageSink messageSink)
            : base(messageSink)
        {
        }

        protected override IXunitTestCase CreateTestCase(
            ITestFrameworkDiscoveryOptions discoveryOptions,
            ITestMethod testMethod,
            IAttributeInfo factAttribute)
            => new ConditionalFactTestCase(
                DiagnosticMessageSink,
                discoveryOptions.MethodDisplayOrDefault(),
                discoveryOptions.MethodDisplayOptionsOrDefault(),
                testMethod);
    }
}
