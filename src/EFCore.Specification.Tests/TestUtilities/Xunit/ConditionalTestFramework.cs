// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Reflection;
using Xunit.Abstractions;
using Xunit.Sdk;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore.TestUtilities.Xunit
{
    public class ConditionalTestFramework : XunitTestFramework
    {
        public ConditionalTestFramework(IMessageSink messageSink)
            : base(messageSink)
        {
            messageSink.OnMessage(
                new DiagnosticMessage
                {
                    Message = "Using " + nameof(ConditionalTestFramework)
                });
        }

        protected override ITestFrameworkExecutor CreateExecutor(AssemblyName assemblyName)
            => new ConditionalTestFrameworkExecutor(assemblyName, SourceInformationProvider, DiagnosticMessageSink);
    }
}
