// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Xunit;
using Xunit.Sdk;

namespace Microsoft.EntityFrameworkCore.Specification.Tests.TestUtilities.Xunit
{
    [AttributeUsage(AttributeTargets.Method)]
    [XunitTestCaseDiscoverer("Microsoft.EntityFrameworkCore.Specification.Tests.TestUtilities.Xunit.ConditionalFactDiscoverer", "Microsoft.EntityFrameworkCore.Specification.Tests")]
    public class ConditionalFactAttribute : FactAttribute
    {
    }
}
