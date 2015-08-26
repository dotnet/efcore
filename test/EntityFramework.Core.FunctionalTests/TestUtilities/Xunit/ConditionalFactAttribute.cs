// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Xunit;
using Xunit.Sdk;

namespace Microsoft.Data.Entity.FunctionalTests.TestUtilities.Xunit
{
    [AttributeUsage(AttributeTargets.Method)]
    [XunitTestCaseDiscoverer("Microsoft.Data.Entity.FunctionalTests.TestUtilities.Xunit.ConditionalAttributeDiscoverer", "EntityFramework.Core.FunctionalTests")]
    public class ConditionalFactAttribute : FactAttribute
    {
    }
}
