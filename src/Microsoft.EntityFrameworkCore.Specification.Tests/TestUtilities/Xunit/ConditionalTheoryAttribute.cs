// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Xunit;
using Xunit.Sdk;

namespace Microsoft.EntityFrameworkCore.FunctionalTests.TestUtilities.Xunit
{
    [AttributeUsage(AttributeTargets.Method)]
    [XunitTestCaseDiscoverer("Microsoft.EntityFrameworkCore.FunctionalTests.TestUtilities.Xunit.ConditionalTheoryDiscoverer", "Microsoft.EntityFrameworkCore.Specification.Tests")]
    public class ConditionalTheoryAttribute : TheoryAttribute
    {
    }
}
